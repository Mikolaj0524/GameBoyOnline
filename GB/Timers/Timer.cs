using GB.Interfaces;

namespace GB.Timers
{
	public class Timer : IRWInterface
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
