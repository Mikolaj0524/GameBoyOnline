using GB.Interfaces;

namespace GB.Controls
{
	public class Joypad : IRWInterface
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
