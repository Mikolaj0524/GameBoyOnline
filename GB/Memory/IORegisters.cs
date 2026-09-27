namespace GB.Memory
{
	public class IORegisters : Memory
	{
		public IORegisters() : base(128, 0xFF00)
		{
			// 128 - size (0xFF00 - 0xFF7F)
			// 0xFF00 - offset
		}
	}
}