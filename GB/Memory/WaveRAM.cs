namespace GB.Memory
{
	public class WaveRAM : Memory
	{
		public WaveRAM() : base(16, 0xFF30)
		{
			// 16 - size
			// 0xFF30 - offset
		}
	}
}