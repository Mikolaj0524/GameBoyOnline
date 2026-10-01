using GB.Utils;

namespace GB.GPU
{
	public partial class PPU
	{
		private bool _windowTriggered;
		private int _windowLine;
		private readonly int[] _bgColors = new int[160];

		private int GetBackgroundColor(int screenX, byte ly)
		{
			int scrollX = (_screen.Scx + screenX) & 0xFF;
			int scrollY = (_screen.Scy + ly) & 0xFF;

			int tileX = scrollX / 8;
			int tileY = scrollY / 8;

			int pixelX = scrollX % 8;
			int pixelY = scrollY % 8;

			ushort mapBase = (ushort)(_screen.Lcdc.IsBitSet(3) ? 0x9C00 : 0x9800);
			ushort mapAddress = (ushort)(mapBase + tileY * 32 + tileX);

			byte tile = _io.Bus.ReadDirect8(mapAddress);
			ushort tileAddress = GetTileAddress(_screen.Lcdc, tile, pixelY);

			byte low = _io.Bus.ReadDirect8(tileAddress);
			byte high = _io.Bus.ReadDirect8((ushort)(tileAddress + 1));

			int bit = 7 - pixelX;
			int highBit = high.IsBitSet(bit) ? 1 : 0;
			int lowBit = low.IsBitSet(bit) ? 1 : 0;

			return (highBit << 1) | lowBit;
		}

		private int GetWindowColor(int screenX, byte wx)
		{
			int x = screenX - (wx - 7);
			int y = _windowLine;

			int tileX = x / 8;
			int tileY = y / 8;

			int pixelX = x % 8;
			int pixelY = y % 8;

			ushort mapBase = (ushort)(_screen.Lcdc.IsBitSet(6) ? 0x9C00 : 0x9800);
			ushort mapAddress = (ushort)(mapBase + tileY * 32 + tileX);

			byte tile = _io.Bus.ReadDirect8(mapAddress);
			ushort tileAddress = GetTileAddress(_screen.Lcdc, tile, pixelY);

			byte low = _io.Bus.ReadDirect8(tileAddress);
			byte high = _io.Bus.ReadDirect8((ushort)(tileAddress + 1));

			int bit = 7 - pixelX;
			int highBit = high.IsBitSet(bit) ? 1 : 0;
			int lowBit = low.IsBitSet(bit) ? 1 : 0;

			return (highBit << 1) | lowBit;
		}
	}
}
