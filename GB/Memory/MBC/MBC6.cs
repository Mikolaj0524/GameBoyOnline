namespace GB.Memory.MBC
{
	public class MBC6 : MBC
	{
		/// <summary>Creates an MBC6 controller.</summary>
		public MBC6(byte[] rom, byte[]? ram, int romBanks, int ramBanks) : base(rom, ram, romBanks, ramBanks)
		{
			
		}

		/// <summary>Handles MBC6 control writes.</summary>
		public override void WriteControl(ushort address, byte value)
		{
			if (address <= 0x1FFF)
			{
				RamEnabled = (value & 0x0F) == 0x0A;
			}
			else if (address <= 0x3FFF)
			{
				RomBank = value & 0x3F;
				if (RomBank == 0)
					RomBank = 1;
			}
			else if (address <= 0x5FFF)
			{
				RamBank = value;
			}
		}
	}
}
