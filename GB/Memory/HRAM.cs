namespace GB.Memory
{
	public class HRAM : Memory
	{
		public HRAM() : base(128, 0xFF80)
		{
			// 128 - size
			// 0xFF80 - offset
		}
	}
}
