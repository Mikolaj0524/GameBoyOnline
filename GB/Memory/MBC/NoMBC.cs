namespace GB.Memory.MBC
{
	public class NoMBC : MBC
	{
		public NoMBC(byte[] rom, byte[]? ram, int romBanks, int ramBanks) : base(rom, ram, romBanks, ramBanks)
		{
			RamEnabled = true;
		}

		public override void WriteControl(ushort address, byte value)
		{
			
		}
	}
}
