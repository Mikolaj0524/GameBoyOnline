using GB.Utils;

namespace GB.Memory.MBC
{
	public class MBC5 : MBC
	{
		/// <summary>Creates an MBC5 controller.</summary>
		public MBC5(byte[] rom, byte[]? ram, int romBanks, int ramBanks) : base(rom, ram, romBanks, ramBanks)
		{
			
		}


		/// <summary>Handles MBC5 control writes.</summary>
		public override void WriteControl(ushort address, byte value)
		{
			if (address <= 0x1FFF)
			{
				RamEnabled = (value & 0x0F) == 0x0A;
			}
			else if (address <= 0x2FFF)
			{
				RomBank = (RomBank & 0x100) | value;
			}
			else if (address <= 0x3FFF)
			{
				RomBank = (RomBank & 0xFF) | (value.GetBit(0) << 8);
			}
			else if (address <= 0x5FFF)
			{
				RamBank = value & 0x0F;
			}
		}
	}
}
