using GB.Interfaces;

namespace GB.Audio
{
	public class APU : IRWInterface
	{
		public byte Read8(ushort address)
		{
			return 0xFF;
		}

		public void Write8(ushort address, byte value)
		{

		}
	}
}
