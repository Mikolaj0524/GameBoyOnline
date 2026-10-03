using GB.Utils;

namespace GB.Memory.MBC
{
	public class MBC1 : MBC
	{
		/// <summary>Current MBC1 banking mode.</summary>
		private int _mode = 0;


		/// <summary>Creates an MBC1 controller.</summary>
		public MBC1(byte[] rom, byte[]? ram, int romBanks, int ramBanks) : base(rom, ram, romBanks, ramBanks)
		{

		}


		/// <summary>Handles MBC1 control writes.</summary>
		public override void WriteControl(ushort address, byte value)
		{
			if (address <= 0x1FFF)
			{
				RamEnabled = (value & 0x0F) == 0x0A;
			}
			else if (address <= 0x3FFF)
			{
				int lower = value & 0x1F;
				RomBank = lower == 0 ? 1 : lower;
			}
			else if (address <= 0x5FFF)
			{
				RamBank = value & 0x03;
			}
			else
			{
				_mode = value.GetBit(0);
			}
		}


		/// <summary>Reads from the lower ROM area.</summary>
		/// <returns>Read byte.</returns>
		public override byte ReadLowRom(ushort address)
		{
			int bank = _mode == 1 ? (RamBank << 5) : 0;
			return ReadRomBank(bank, address);
		}


		/// <summary>Reads from the upper ROM area.</summary>
		/// <returns>Read byte.</returns>
		public override byte ReadHighRom(ushort address){
			return ReadRomBank((RamBank << 5) | RomBank, address - 0x4000);
		}


		/// <summary>Gets the current RAM bank.</summary>
		/// <returns>RAM bank number.</returns>
		protected override int CurrentRamBank() => _mode == 0 ? 0 : base.CurrentRamBank();
	}
}
