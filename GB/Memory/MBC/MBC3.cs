namespace GB.Memory.MBC
{
	public class MBC3 : MBC
	{
		/// <summary>Real-time clock.</summary>
		private readonly RTC _rtc = new();


		/// <summary>Selected RTC register.</summary>
		private byte _rtcReg = 0;


		/// <summary>Previous RTC latch value.</summary>
		private byte _latchPrev = 0xFF;


		/// <summary>Creates an MBC3 controller.</summary>
		public MBC3(byte[] rom, byte[]? ram, int romBanks, int ramBanks) : base(rom, ram, romBanks, ramBanks)
		{
			
		}


		/// <summary>Handles MBC3 control writes.</summary>
		public override void WriteControl(ushort address, byte value)
		{
			if (address <= 0x1FFF)
			{
				RamEnabled = (value & 0x0F) == 0x0A;
			}
			else if (address <= 0x3FFF)
			{
				RomBank = value & 0x7F;
				if (RomBank == 0)
					RomBank = 1;
			}
			else if (address <= 0x5FFF)
			{
				if (value <= 0x03)
				{
					RamBank = value;
					_rtcReg = 0;
				}
				else if (value >= 0x08 && value <= 0x0C)
				{
					_rtcReg = value;
				}
			}
			else
			{
				if (_latchPrev == 0x00 && value == 0x01)
					_rtc.Latch();

				_latchPrev = value;
			}
		}


		/// <summary>Reads from cartridge RAM or RTC.</summary>
		/// <returns>Read byte.</returns>
		public override byte ReadRam(ushort address)
		{
			if (!RamEnabled)
				return 0xFF;

			if (_rtcReg != 0)
				return _rtc.Read(_rtcReg);

			return base.ReadRam(address);
		}


		/// <summary>Writes to cartridge RAM or RTC.</summary>
		public override void WriteRam(ushort address, byte value)
		{
			if (!RamEnabled)
				return;

			if (_rtcReg != 0)
			{
				_rtc.Write(_rtcReg, value);
				return;
			}

			base.WriteRam(address, value);
		}
	}
}
