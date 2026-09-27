using GB.CPU;
using GB.Interfaces;
using GB.Memory;

namespace GB.Communication
{
	public class BUS : IRWInterface
	{
		public CPU.CPU Cpu;
		public Cartridge? Cartridge;
		public BIOS? Bios;
		public VRAM VRam = new();
		public IO IO;
		public HRAM HRam = new();
		public WRAM WRam = new();
		public OAM Oam = new();
		public InterruptController InterruptController = new();

		public BUS()
		{
			Cpu = new CPU.CPU();
			IO = new IO();
		}

		public byte Read8(ushort address) {
			return ReadDirect8(address);
		}

		public void Write8(ushort address, byte value) {
			WriteDirect8(address, value);
		}

		public byte ReadDirect8(ushort address)
		{
			if (address == 0xFF4D)
				return 0xFF;

			if (address <= 0x00FF && Bios?.Enabled == true)
				return Bios?.Read8(address) ?? 0xFF;

			if (address <= 0x7FFF)
				return Cartridge?.Read8(address) ?? 0xFF;

			if (address >= 0x8000 && address <= 0x9FFF)
				return VRam.Read8(address);

			if (address >= 0xA000 && address <= 0xBFFF)
				return Cartridge?.Read8(address) ?? 0xFF;

			if (address >= 0xC000 && address <= 0xDFFF)
				return WRam.Read8(address);

			if (address >= 0xFF80 && address <= 0xFFFE)
				return HRam.Read8(address);

			if (address < 0xFE00)
				return 0;

			if (address >= 0xFE00 && address <= 0xFE9F)
				return Oam.Read8(address);

			if (address < 0xFF00)
				return 0;

			if (address >= 0xFF00 && address <= 0xFF7F)
				return IO.Read8(address);

			if (address == 0xFFFF)
				return InterruptController.IE;

			throw new NotImplementedException($"[BUS] Unimplemented Read8 address: 0x{address:X4}");
		}

		public void WriteDirect8(ushort address, byte value)
		{
			if (address == 0xFF4D)
				return;

			if (address == 0xFF50)
			{
				Bios?.Enabled = false;
				return;
			}

			if (address <= 0x7FFF)
			{
				Cartridge?.Write8(address, value);
				return;
			}

			if (address >= 0x8000 && address <= 0x9FFF)
			{
				VRam.Write8(address, value);
				return;
			}

			if (address >= 0xA000 && address <= 0xBFFF)
			{
				Cartridge?.Write8(address, value);
				return;
			}

			if (address >= 0xC000 && address <= 0xDFFF)
			{
				WRam.Write8(address, value);
				return;
			}

			if (address >= 0xFF80 && address <= 0xFFFE)
			{
				HRam.Write8(address, value);
				return;
			}

			if (address < 0xFE00)
				return;

			if (address >= 0xFE00 && address <= 0xFE9F)
			{
				Oam.Write8(address, value);
				return;
			}

			if (address < 0xFF00)
				return;

			if (address >= 0xFF00 && address <= 0xFF7F)
			{
				IO.Write8(address, value);
				return;
			}

			if (address == 0xFFFF)
			{
				InterruptController.IE = value;
				return;
			}

			throw new NotImplementedException($"[BUS] Unimplemented Write8 address: 0x{address:X4}, Value: 0x{value:X2}");
		}

		public ushort Read16(ushort address)
		{
			byte low = Read8(address);
			byte high = Read8((ushort)(address + 1));
			return (ushort)(low | (high << 8));
		}

		public void Write16(ushort address, ushort value)
		{
			Write8(address, (byte)value);
			Write8((ushort)(address + 1), (byte)(value >> 8));
		}


		public void SetCartridge(byte[] rom) => Cartridge = new Cartridge(rom);
		public void SetBios(byte[] bios) => Bios = new BIOS(bios);
	}
}