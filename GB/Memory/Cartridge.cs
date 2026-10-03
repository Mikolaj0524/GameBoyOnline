using GB.Interfaces;
using GB.Utils;
using System.Text;

namespace GB.Memory
{
	public class Cartridge : IRWInterface
	{
		/// <summary>ROM data.</summary>
		private readonly byte[] _rom;

		/// <summary>RAM data.</summary>
		private byte[]? _ram;

		/// <summary>MBC type.</summary>
		private int _mbc = 0;

		// Memory and license variables
		private byte _type, _romSize, _ramSize, _license, _rtcReg = 0;
		private string _newLicense = "00";
		private int _romBanks = 2, _ramBanks = 0, _romBank = 1, _ramBank = 0, _mode = 0;
		private bool _ramEnabled = false;


		// RTC variables
		private byte _latchPrev = 0xFF;
		private long _rtcTotal = 0;
		private DateTime _rtcStamp = DateTime.UtcNow;
		private bool _rtcHalt = false, _rtcCarry = false;
		private byte _sec, _min, _hour, _dayLow, _dayHigh;

		/// <summary>Cartridge title.</summary>
		public string? Title;

		/// <summary>Cartridge version.</summary>
		public int Version;

		/// <summary>Battery status.</summary>
		public bool HasBattery { get; private set; }

		public Cartridge(byte[] rom)
		{
			if (rom == null || rom.Length < 0x0150)
				throw new ArgumentException("Plik ROM jest za krótki (brak pełnego nagłówka).", nameof(rom));

			_rom = rom;
			ReadHeader();
		}


		/// <summary>Reads the cartridge header.</summary>
		private void ReadHeader()
		{
			Title = Encoding.ASCII.GetString(_rom, 0x0134, 16).TrimEnd('\0');

			_type = _rom[0x0147];
			_romSize = _rom[0x0148];
			_ramSize = _rom[0x0149];
			_license = _rom[0x014B];
			Version = _rom[0x014C];
			_newLicense = Encoding.ASCII.GetString(_rom, 0x0144, 2);

			_mbc = GetMbcIndex(_type);

			HasBattery = _type == 0x03 || _type == 0x06 || _type == 0x09 || _type == 0x0D || _type == 0x0F || _type == 0x10 || _type == 0x13 || _type == 0x1B || _type == 0x1E || _type == 0x22 || _type == 0xFF;

			_romBanks = Math.Max(2, _romSize <= 8 ? 2 << _romSize : 2);
			int ramBytes = _ramSize switch
			{
				0x01 => 2 * 1024,
				0x02 => 8 * 1024,
				0x03 => 32 * 1024,
				0x04 => 128 * 1024,
				0x05 => 64 * 1024,
				_ => 0
			};

			if (_mbc == 2)
				ramBytes = 512;

			_ram = ramBytes > 0 ? new byte[ramBytes] : null;
			_ramBanks = ramBytes > 0 ? Math.Max(1, ramBytes / 0x2000) : 0;

			if (_mbc == 0)
				_ramEnabled = true;
		}


		/// <summary>Saves cartridge RAM.</summary>
		/// <returns>RAM data.</returns>
		public byte[]? SaveRam() => _ram == null ? null : (byte[])_ram.Clone();


		/// <summary>Loads cartridge RAM.</summary>
		public void LoadRam(byte[] data)
		{
			if (_ram == null || data == null)
				return;

			Array.Copy(data, _ram, Math.Min(data.Length, _ram.Length));
		}


		/// <summary>Reads from the cartridge.</summary>
		/// <returns>Read byte.</returns>
		public byte Read8(ushort address)
		{
			if (address <= 0x3FFF)
				return ReadLowRom(address);

			if (address <= 0x7FFF)
				return ReadHighRom(address);

			if (address >= 0xA000 && address <= 0xBFFF)
				return ReadRam(address);

			return 0xFF;
		}


		/// <summary>Writes to the cartridge.</summary>
		public void Write8(ushort address, byte value)
		{
			if (address >= 0xA000 && address <= 0xBFFF)
			{
				WriteRam(address, value);
				return;
			}

			if (address > 0x7FFF || _mbc == 0)
				return;

			if (_mbc == 2)
			{
				if (address <= 0x3FFF)
				{
					if (address.IsBitSet(8)){ 
						WriteRomBank(address, value);
						return;
					}
					
					WriteRamEnable(address, value);
				}
				return;
			}

			if (address <= 0x1FFF)
			{
				WriteRamEnable(address, value);
				return;
			}

			if (address <= 0x3FFF)
			{
				WriteRomBank(address, value);
				return;
			}

			if (address <= 0x5FFF)
			{
				WriteRamBank(address, value);
				return;
			}

			WriteModeLatch(value);
		}


		/// <summary>Reads from ROM bank 0.</summary>
		private byte ReadLowRom(ushort address)
		{
			int bank = 0;
			if (_mbc == 1 && _mode == 1)
				bank = (_ramBank << 5) % _romBanks;

			int offset = (bank * 0x4000) + address;
			return offset < _rom.Length ? _rom[offset] : (byte)0xFF;
		}


		/// <summary>Reads from current ROM bank.</summary>
		private byte ReadHighRom(ushort address)
		{
			int bank = _mbc == 1 ? ((_ramBank << 5) | _romBank) : _romBank;
			bank %= _romBanks;

			int offset = (bank * 0x4000) + (address - 0x4000);
			return offset < _rom.Length ? _rom[offset] : (byte)0xFF;
		}


		/// <summary>Gets current RAM bank.</summary>
		private int CurrentRamBank()
		{
			int bank = (_mbc == 1 && _mode == 0) ? 0 : _ramBank;
			return _ramBanks > 0 ? bank % _ramBanks : 0;
		}


		/// <summary>Gets RAM offset.</summary>
		private int RamOffset(ushort address)
		{
			int offset = (CurrentRamBank() * 0x2000) + (address - 0xA000);
			if (_ram != null && _ram.Length < 0x2000)
				offset = (address - 0xA000) % _ram.Length;

			return offset;
		}


		/// <summary>Reads from RAM.</summary>
		private byte ReadRam(ushort address)
		{
			if (!_ramEnabled)
				return 0xFF;

			if (_mbc == 2)
			{
				if (_ram == null) return 0xFF;
				return (byte)(_ram[(address - 0xA000) & 0x01FF] | 0xF0);
			}

			if (_mbc == 3 && _rtcReg != 0)
				return ReadRtc();

			if (_ram == null || _ramBanks == 0)
				return 0xFF;

			int offset = RamOffset(address);
			return offset < _ram.Length ? _ram[offset] : (byte)0xFF;
		}


		/// <summary>Writes to RAM.</summary>
		private void WriteRam(ushort address, byte value)
		{
			if (!_ramEnabled)
				return;

			if (_mbc == 2)
			{
				_ram?[(address - 0xA000) & 0x01FF] = (byte)(value & 0x0F);
				return;
			}

			if (_mbc == 3 && _rtcReg != 0)
			{
				WriteRtc(_rtcReg, value);
				return;
			}

			if (_ram == null || _ramBanks == 0)
				return;

			int offset = RamOffset(address);
			if (offset < _ram.Length)
				_ram[offset] = value;
		}


		/// <summary>Toggles RAM.</summary>
		private void WriteRamEnable(ushort address, byte value)
		{
			if (_mbc == 7)
			{
				_ramEnabled = value == 0x0A;
				return;
			}

			_ramEnabled = (value & 0x0F) == 0x0A;
		}


		/// <summary>Changes ROM bank.</summary>
		private void WriteRomBank(ushort address, byte value)
		{
			switch (_mbc)
			{
				case 1:
					int lower = value & 0x1F;
					_romBank = lower == 0 ? 1 : lower;
					break;
				case 2:
					_romBank = value & 0x0F;
					if (_romBank == 0) _romBank = 1;
					break;
				case 3:
					_romBank = value & 0x7F;
					if (_romBank == 0) _romBank = 1;
					break;
				case 5:
					_romBank = (address <= 0x2FFF) ? ((_romBank & 0x100) | value) : ((_romBank & 0xFF) | ((value & 0x01) << 8));
					break;
				case 6:
					_romBank = value & 0x3F;
					if (_romBank == 0) 
						_romBank = 1;
					break;
				default:
					_romBank = value & 0x7F;
					if (_romBank == 0) 
						_romBank = 1;
					break;
			}
		}


		/// <summary>Changes RAM bank.</summary>
		private void WriteRamBank(ushort address, byte value)
		{
			switch (_mbc)
			{
				case 1:
					_ramBank = value & 0x03;
					break;
				case 3:
					if (value <= 0x03)
					{
						_ramBank = value;
						_rtcReg = 0;
					}
					else if (value >= 0x08 && value <= 0x0C)
					{
						_rtcReg = value;
					}
					break;
				case 5:
					_ramBank = value & 0x0F;
					break;
				case 6:
					_ramBank = value;
					break;
				default:
					if (value <= 0x03)
						_ramBank = value;
					break;
			}
		}


		/// <summary>Changes MBC mode.</summary>
		private void WriteModeLatch(byte value)
		{
			if (_mbc == 3)
			{
				if (_latchPrev == 0x00 && value == 0x01)
					LatchRtc();

				_latchPrev = value;
			}
			else if (_mbc == 1)
			{
				_mode = value & 0x01;
			}
		}


		/// <summary>Updates RTC.</summary>
		private void UpdateRtc()
		{
			DateTime now = DateTime.UtcNow;

			if (!_rtcHalt)
			{
				long elapsed = (long)(now - _rtcStamp).TotalSeconds;
				if (elapsed > 0)
				{
					_rtcTotal += elapsed;
					_rtcStamp = _rtcStamp.AddSeconds(elapsed);
				}
			}
			else
			{
				_rtcStamp = now;
			}

			const long maxSeconds = 512L * 86400L;
			if (_rtcTotal >= maxSeconds)
			{
				_rtcCarry = true;
				_rtcTotal %= maxSeconds;
			}
		}


		/// <summary>Saves current RTC values.</summary>
		private void LatchRtc()
		{
			UpdateRtc();

			long days = _rtcTotal / 86400;
			_sec = (byte)(_rtcTotal % 60);
			_min = (byte)((_rtcTotal / 60) % 60);
			_hour = (byte)((_rtcTotal / 3600) % 24);
			_dayLow = (byte)(days & 0xFF);
			_dayHigh = (byte)(((days >> 8) & 0x01) | (_rtcHalt ? 0x40 : 0) | (_rtcCarry ? 0x80 : 0));
		}


		/// <summary>Reads RTC.</summary>
		private byte ReadRtc()
		{
			return _rtcReg switch
			{
				0x08 => _sec,
				0x09 => _min,
				0x0A => _hour,
				0x0B => _dayLow,
				0x0C => _dayHigh,
				_ => 0xFF
			};
		}


		/// <summary>Writes to RTC.</summary>
		private void WriteRtc(byte reg, byte value)
		{
			UpdateRtc();

			long s = _rtcTotal % 60;
			long m = (_rtcTotal / 60) % 60;
			long h = (_rtcTotal / 3600) % 24;
			long d = _rtcTotal / 86400;

			switch (reg)
			{
				case 0x08: s = value & 0x3F; break;
				case 0x09: m = value & 0x3F; break;
				case 0x0A: h = value & 0x1F; break;
				case 0x0B: d = (d & 0x100) | value; break;
				case 0x0C:
					d = (d & 0xFF) | ((long)(value & 0x01) << 8);
					_rtcHalt = (value & 0x40) != 0;
					_rtcCarry = (value & 0x80) != 0;
					break;
				default: return;
			}

			_rtcTotal = (d * 86400) + (h * 3600) + (m * 60) + s;
			_rtcStamp = DateTime.UtcNow;
		}


		/// <summary>Gets MBC type.</summary>
		/// <returns>MBC type.</returns>
		private static int GetMbcIndex(byte type) => type switch
		{
			>= 0x01 and <= 0x03 => 1,
			0x05 or 0x06 => 2,
			>= 0x0F and <= 0x13 => 3,
			>= 0x19 and <= 0x1E => 5,
			0x20 => 6,
			0x22 => 7,
			_ => 0
		};


		/// <summary>Gets ROM type.</summary>
		/// <returns>ROM type.</returns>
		public string GetRomType() => _type switch
		{
			0x00 => "ROM ONLY",
			0x01 => "MBC1",
			0x02 => "MBC1 + RAM",
			0x03 => "MBC1 + RAM + BATTERY",
			0x05 => "MBC2",
			0x06 => "MBC2 + BATTERY",
			0x08 => "ROM + RAM",
			0x09 => "ROM + RAM + BATTERY",
			0x0B => "MMM01",
			0x0C => "MMM01 + RAM",
			0x0D => "MMM01 + RAM + BATTERY",
			0x0F => "MBC3 + TIMER + BATTERY",
			0x10 => "MBC3 + TIMER + RAM + BATTERY",
			0x11 => "MBC3",
			0x12 => "MBC3 + RAM",
			0x13 => "MBC3 + RAM + BATTERY",
			0x19 => "MBC5",
			0x1A => "MBC5 + RAM",
			0x1B => "MBC5 + RAM + BATTERY",
			0x1C => "MBC5 + RUMBLE",
			0x1D => "MBC5 + RUMBLE + RAM",
			0x1E => "MBC5 + RUMBLE + RAM + BATTERY",
			0x20 => "MBC6",
			0x22 => "MBC7 + SENSOR + RUMBLE + RAM + BATTERY",
			0xFE => "HuC3",
			0xFF => "HuC1 + RAM + BATTERY",
			_ => $"Undefined (0x{_type:X2})"
		};


		/// <summary>Gets publisher.</summary>
		/// <returns>Publisher name.</returns>
		public string GetPublisher() => (_license == 0x33) ? GetNewLicense(_newLicense) : GetOldLicense(_license);


		/// <summary>Gets publisher from new license code.</summary>
		/// <returns>Publisher name.</returns>
		private static string GetNewLicense(string index) => index switch
		{
			"01" => "Nintendo R&D1",
			"08" => "Capcom",
			"13" => "Electronic Arts",
			"18" => "Hudson Soft",
			"19" => "b-ai",
			"20" => "KSS",
			"22" => "Planning Office WADA",
			"24" => "Tokuma Shoten Intermedia",
			"25" => "Bust-A-Move",
			"28" => "Kotobuki Systems",
			"29" => "Seta",
			"30" => "Viacom",
			"32" => "Ocean",
			"33" => "Sierra",
			"34" => "Memory Corp",
			"35" => "Hanbar",
			"37" => "PCM Complete",
			"38" => "Sanrio",
			"39" => "Kotobuki Systems",
			"41" => "Taito",
			"42" => "Indie Games",
			"44" => "Flux",
			"46" => "Game Arts",
			"47" => "Biox",
			"49" => "Square Enix",
			"50" => "Absolute",
			"51" => "Acclaim",
			"52" => "Activision",
			"53" => "American Sammy",
			"54" => "GameTek",
			"55" => "Park Place",
			"56" => "LJN",
			"57" => "Matchbox",
			"58" => "Mattel",
			"59" => "Milton Bradley",
			"60" => "Titus",
			"61" => "Virgin",
			"64" => "LucasArts",
			"67" => "Ocean",
			"69" => "Electronic Arts",
			"70" => "Infogrames",
			"71" => "Interplay",
			"72" => "Broderbund",
			"73" => "Sculptured Software",
			"75" => "The Sales Curve",
			"78" => "THQ",
			"79" => "Accolade",
			"80" => "Misawa",
			"83" => "Lozc",
			"86" => "Tokuma Shoten Intermedia",
			"87" => "Tsukuda Original",
			"91" => "Chunsoft",
			"92" => "Video System",
			"93" => "Tsubaraya Productions",
			"95" => "Varie",
			"96" => "Yonezawa/S'Pal",
			"97" => "Kaneko",
			"99" => "Pack-In-Video",
			"9H" => "Bottom Up",
			"A4" => "Konami (Yu-Gi-Oh!)",
			"BL" => "MTO",
			"DK" => "Kodansha",
			_ => $"Unknown New Code"
		};


		/// <summary>Gets publisher from old license code.</summary>
		/// <returns>Publisher name.</returns>
		private static string GetOldLicense(byte index) => index switch
		{
			0x00 => "None",
			0x01 => "Nintendo",
			0x08 => "Capcom",
			0x09 => "Hot-B",
			0x0A => "Jaleco",
			0x0B => "Coconuts Japan",
			0x0C => "Elite Systems",
			0x13 => "EA (Electronic Arts)",
			0x18 => "Hudson Soft",
			0x19 => "ITC Entertainment",
			0x1A => "Yanoman",
			0x1D => "Japan Clary",
			0x1F => "Virgin Interactive",
			0x24 => "PCM Complete",
			0x25 => "SanX",
			0x28 => "Kotobuki Systems",
			0x29 => "Seta",
			0x30 => "Infogrames",
			0x31 => "Nintendo",
			0x32 => "Bandai",
			0x34 => "Konami",
			0x35 => "HectorSoft",
			0x38 => "Capcom",
			0x39 => "Banpresto",
			0x3E => "Gremlin",
			0x41 => "Ubisoft",
			0x42 => "Atlus",
			0x44 => "Malibu",
			0x46 => "Angel",
			0x47 => "Bullet-Proof Software",
			0x49 => "Irem",
			0x50 => "Absolute",
			0x51 => "Acclaim",
			0x52 => "Activision",
			0x53 => "American Sammy",
			0x54 => "GameTek",
			0x55 => "Park Place",
			0x56 => "LJN",
			0x57 => "Matchbox",
			0x58 => "Mattel",
			0x59 => "Milton Bradley",
			0x60 => "Titus",
			0x61 => "Virgin Interactive",
			0x67 => "LucasArts",
			0x6F => "Ocean",
			0x70 => "Infogrames",
			0x71 => "Interplay",
			0x72 => "Broderbund",
			0x73 => "Sculptured Software",
			0x75 => "The Sales Curve",
			0x78 => "THQ",
			0x79 => "Accolade",
			0x80 => "Misawa Entertainment",
			0x83 => "Lozc",
			0x86 => "Tokuma Shoten Intermedia",
			0x8B => "Bullet-Proof Software",
			0x8C => "Vic Tokai",
			0x8E => "Ape",
			0x8F => "I'Max",
			0x91 => "Chunsoft",
			0x92 => "Video System",
			0x93 => "Tsubaraya Productions",
			0x95 => "Varie",
			0x96 => "Yonezawa/S'Pal",
			0x97 => "Kaneko",
			0x99 => "Pack-In-Video",
			0x9A => "Nichibutsu",
			0x9B => "Tecmo",
			0x9C => "Imagineer",
			0x9D => "Banpresto",
			0x9F => "Nova",
			0xA1 => "Hori Electric",
			0xA2 => "Bandai",
			0xA4 => "Konami",
			0xA6 => "Kawada",
			0xA7 => "Takara",
			0xA9 => "Technos Japan",
			0xAA => "Broderbund",
			0xAC => "Toei Animation",
			0xAD => "Toho",
			0xAF => "Namco",
			0xB1 => "Asmic",
			0xB2 => "Bandai",
			0xB4 => "Enix",
			0xB6 => "HAL Laboratory",
			0xB7 => "SNK",
			0xB9 => "Pony Canyon",
			0xBA => "Culture Brain",
			0xBB => "Sunsoft",
			0xBD => "Sony Imagesoft",
			0xBF => "Sammy",
			0xC0 => "Taito",
			0xC2 => "Kemco",
			0xC3 => "Square",
			0xC4 => "Tokuma Shoten Intermedia",
			0xC5 => "Data East",
			0xC6 => "Tonkin House",
			0xC8 => "Koei",
			0xC9 => "UFL",
			0xCA => "Ultra",
			0xCB => "Vap",
			0xCC => "Use",
			0xCD => "Meldac",
			0xCF => "Pony Canyon",
			0xD0 => "Angel",
			0xD1 => "Taito",
			0xD2 => "Sofel",
			0xD3 => "Quest",
			0xD4 => "Sigma Enterprises",
			0xD6 => "Ask Kodansha",
			0xD7 => "Naxat Soft",
			0xD8 => "Copya System",
			0xD9 => "Banpresto",
			0xDA => "Tomy",
			0xDB => "LJN",
			0xDD => "NCS",
			0xDE => "Human",
			0xDF => "Altron",
			0xE0 => "Jaleco",
			0xE1 => "Towa Chiki",
			0xE2 => "Yutaka",
			0xE3 => "Varie",
			0xE5 => "Epoch",
			0xE7 => "Athena",
			0xE8 => "Asmik ACE Entertainment",
			0xE9 => "Natsume",
			0xEA => "King Records",
			0xEB => "Atlus",
			0xEC => "Epic/Sony Records",
			0xEE => "IGS",
			0xF0 => "A Wave",
			0xF3 => "Extreme Entertainment",
			0xFF => "LJN",
			_ => $"Unknown Old Code"
		};
	}
}