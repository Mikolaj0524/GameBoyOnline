namespace GB.Memory
{
	public class OAM : Memory
	{
		public OAM() : base(160, 0xFE00)
		{
			// 160 - size
			// 0xFE00 - offset
		}
	}
}