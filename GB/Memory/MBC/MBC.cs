
namespace GB.Memory.MBC
{
	public abstract class MBC
	{
		/// <summary>Cartridge ROM data.</summary>
		protected readonly byte[] Rom;


		/// <summary>Cartridge RAM data.</summary>
		protected readonly byte[]? Ram;


		/// <summary>RAM access state.</summary>
		protected bool RamEnabled = false;


		/// <summary>Current ROM and RAM banks.</summary>
		protected int RomBank = 1, RamBank = 0;


		/// <summary>Number of ROM and RAM banks.</summary>
		protected readonly int RomBanks, RamBanks;


		/// <summary>Creates a memory bank controller.</summary>
		protected MBC(byte[] rom, byte[]? ram, int romBanks, int ramBanks)
		{
			Rom = rom;
			Ram = ram;
			RomBanks = romBanks;
			RamBanks = ramBanks;
		}


		/// <summary>Handles a memory bank control write.</summary>
		public abstract void WriteControl(ushort address, byte value);


		/// <summary>Reads from the lower ROM bank.</summary>
		/// <returns>Read byte.</returns>
		public virtual byte ReadLowRom(ushort address) => ReadRomBank(0, address);


		/// <summary>Reads from the current ROM bank.</summary>
		/// <returns>Read byte.</returns>
		public virtual byte ReadHighRom(ushort address) => ReadRomBank(RomBank, address - 0x4000);


		/// <summary>Reads from cartridge RAM.</summary>
		/// <returns>Read byte.</returns>
		public virtual byte ReadRam(ushort address)
		{
			if (!RamEnabled || Ram == null || RamBanks == 0)
				return 0xFF;

			int offset = RamOffset(address);
			return offset < Ram.Length ? Ram[offset] : (byte)0xFF;
		}


		/// <summary>Writes to cartridge RAM.</summary>
		public virtual void WriteRam(ushort address, byte value)
		{
			if (!RamEnabled || Ram == null || RamBanks == 0)
				return;

			int offset = RamOffset(address);
			if (offset < Ram.Length)
				Ram[offset] = value;
		}


		/// <summary>Reads from a ROM bank.</summary>
		/// <returns>Read byte.</returns>
		protected byte ReadRomBank(int bank, int offsetInBank)
		{
			int offset = ((bank % RomBanks) * 0x4000) + offsetInBank;
			return offset < Rom.Length ? Rom[offset] : (byte)0xFF;
		}


		/// <summary>Gets the current RAM bank.</summary>
		/// <returns>RAM bank number.</returns>
		protected virtual int CurrentRamBank() => RamBanks > 0 ? RamBank % RamBanks : 0;


		/// <summary>Gets the RAM offset.</summary>
		/// <returns>RAM offset.</returns>
		protected int RamOffset(ushort address)
		{
			int offset = (CurrentRamBank() * 0x2000) + (address - 0xA000);
			if (Ram != null && Ram.Length < 0x2000)
				offset = (address - 0xA000) % Ram.Length;

			return offset;
		}
	}
}
