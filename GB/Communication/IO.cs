using GB.Interfaces;

namespace GB.Communication
{
	public class IO : IRWInterface
	{
		public byte Read8(ushort address)
		{
			return 0x00;
		}

		public void Write8(ushort address, byte value)
		{

		}
	}
}