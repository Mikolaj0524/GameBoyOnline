namespace GB.Communication
{
	public class DMA(BUS bus)
	{
		private readonly BUS _bus = bus;


		/// <summary>Transfers data to OAM.</summary>
		public void Transfer(byte value)
		{
			ushort source = (ushort)(value << 8);

			for (int i = 0; i < 160; i++)
			{
				byte data = _bus.ReadDirect8((ushort)(source + i));
				_bus.WriteDirect8((ushort)(0xFE00 + i), data);
				_bus.Tick(4);
			}
		}
	}
}
