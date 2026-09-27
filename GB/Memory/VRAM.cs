namespace GB.Memory
{
	public class VRAM : Memory
	{
		public VRAM() : base(8192, 0x8000)
		{
			// 8192 - size
			// 0x8000 - offset
		}
	}
}