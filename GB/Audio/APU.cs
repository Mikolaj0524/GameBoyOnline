using GB.Interfaces;

namespace GB.Audio
{
	public class APU : IRWInterface
	{
		private byte[] mem = new byte[2048];
		public byte Read8(ushort address)
		{
			return mem[address - 0xFF10];
		}

		public void Write8(ushort address, byte value)
		{
			mem[address - 0xFF10] = value;
		}
	}
}
