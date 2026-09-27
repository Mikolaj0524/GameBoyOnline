namespace GB.Memory
{
	public class BIOS(byte[] bios)
	{
		private readonly byte[] _memory = bios;
		public byte Read8(ushort address)
		{
			return _memory[address];
		}
	}
}
