namespace GB.Memory.MBC
{
	public class MBCManager
	{
		/// <summary>Creates an MBC for the cartridge type.</summary>
		/// <returns>Created memory bank controller.</returns>
		public static MBC Create(byte type, byte[] rom, byte[]? ram, int romBanks, int ramBanks) => type switch
		{
			>= 0x01 and <= 0x03 => new MBC1(rom, ram, romBanks, ramBanks),
			0x05 or 0x06 => new MBC2(rom, ram, romBanks, ramBanks),
			>= 0x0F and <= 0x13 => new MBC3(rom, ram, romBanks, ramBanks),
			>= 0x19 and <= 0x1E => new MBC5(rom, ram, romBanks, ramBanks),
			0x20 => new MBC6(rom, ram, romBanks, ramBanks),
			0x22 => new MBC7(rom, ram, romBanks, ramBanks),
			_ => new NoMBC(rom, ram, romBanks, ramBanks)
		};
	}
}
