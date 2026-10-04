namespace GB.Memory.MBC
{
	public class MBCCamera : MBC
	{
		private bool _cameraRegisters;
		public MBCCamera(byte[] rom, byte[]? ram, int romBanks, int ramBanks) : base(rom, ram, romBanks, ramBanks)
		{
			
		}


		/// <summary>Handles bank control writes.</summary>
		public override void WriteControl(ushort address, byte value)
		{
			switch (address & 0x6000)
			{
				case 0x0000:
					RamEnabled = (value & 0x0F) == 0x0A;
					break;
				case 0x2000:
					RomBank = value & 0x7F;
					if (RomBank == 0)
						RomBank = 1;
					break;
				case 0x4000:
					_cameraRegisters = (value & 0x10) != 0;
					RamBank = value & 0x0F;
					break;
				case 0x6000:
					break;
			}
		}


		/// <summary>Reads from cartridge RAM.</summary>
		public override byte ReadRam(ushort address)
		{
			if (_cameraRegisters)
				return 0x00;

			return base.ReadRam(address);
		}


		/// <summary>Writes to cartridge RAM.</summary>
		public override void WriteRam(ushort address, byte value)
		{
			if (_cameraRegisters)
				return;

			base.WriteRam(address, value);
		}

	}
}