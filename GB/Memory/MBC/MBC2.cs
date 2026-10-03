using GB.Utils;

namespace GB.Memory.MBC
{
	public class MBC2 : MBC
	{
		/// <summary>Creates an MBC2 controller.</summary>
		public MBC2(byte[] rom, byte[]? ram, int romBanks, int ramBanks) : base(rom, ram, romBanks, ramBanks)
		{
			
		}


		/// <summary>Handles MBC2 control writes.</summary>
		public override void WriteControl(ushort address, byte value)
		{
			if (address > 0x3FFF)
				return;

			if (address.IsBitSet(8))
			{
				RomBank = value & 0x0F;
				if (RomBank == 0)
					RomBank = 1;
			}
			else
			{
				RamEnabled = (value & 0x0F) == 0x0A;
			}
		}


		/// <summary>Reads from MBC2 RAM.</summary>
		/// <returns>Read byte.</returns>
		public override byte ReadRam(ushort address)
		{
			if (!RamEnabled || Ram == null)
				return 0xFF;

			return (byte)(Ram[(address - 0xA000) & 0x01FF] | 0xF0);
		}


		/// <summary>Writes to MBC2 RAM.</summary>
		public override void WriteRam(ushort address, byte value)
		{
			if (!RamEnabled || Ram == null)
				return;

			Ram[(address - 0xA000) & 0x01FF] = (byte)(value & 0x0F);
		}
	}
}
