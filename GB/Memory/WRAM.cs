namespace GB.Memory
{
	public class WRAM : Memory
	{
		public WRAM() : base(8192, 0xC000)
		{
			// 8192 - size
			// 0xC000 - offset
		}
	}
}