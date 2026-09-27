using GB.Interfaces;

namespace GB.Memory
{
	public class Cartridge : IRWInterface
	{
		private readonly byte[] _rom;

		public Cartridge(byte[] rom)
		{
			_rom = rom;
		}

		public byte Read8(ushort address)
		{
			return 0xFF;
		}

		public void Write8(ushort address, byte value)
		{

		}
	}
}
