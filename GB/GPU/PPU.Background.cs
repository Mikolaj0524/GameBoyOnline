using GB.Utils;

namespace GB.GPU
{
	public partial class PPU
	{
		/// <summary>Window started?</summary>
		private bool _windowTriggered;


		/// <summary>Current window line.</summary>
		private int _windowLine;

		private readonly int[] _bgColors = new int[160];

		private int GetBackgroundColor(int screenX, byte ly)
		{
			// Scrolling
			int scrollX = (_screen.Scx + screenX) & 0xFF;
			int scrollY = (_screen.Scy + ly) & 0xFF;

			// Tile position
			int tileX = scrollX / 8;
			int tileY = scrollY / 8;

			// Pixel position
			int pixelX = scrollX % 8;
			int pixelY = scrollY % 8;

			// Tile map.
			ushort mapBase = (ushort)(_screen.Lcdc.IsBitSet(3) ? 0x9C00 : 0x9800);
			ushort mapAddress = (ushort)(mapBase + tileY * 32 + tileX);

			// Tile
			byte tile = _io.Bus.ReadDirect8(mapAddress);
			ushort tileAddress = GetTileAddress(_screen.Lcdc, tile, pixelY);

			// Tile data
			byte low = _io.Bus.ReadDirect8(tileAddress);
			byte high = _io.Bus.ReadDirect8((ushort)(tileAddress + 1));

			// Pixel color
			int bit = 7 - pixelX;
			int highBit = high.IsBitSet(bit) ? 1 : 0;
			int lowBit = low.IsBitSet(bit) ? 1 : 0;

			return (highBit << 1) | lowBit;
		}

		/// <summary>Gets the window color.</summary>
		/// <returns>Window color index.</returns>
		private int GetWindowColor(int screenX, byte wx)
		{
			// Window position
			int x = screenX - (wx - 7);
			int y = _windowLine;

			// Tile position
			int tileX = x / 8;
			int tileY = y / 8;

			// Pixel position
			int pixelX = x % 8;
			int pixelY = y % 8;

			// Tile map
			ushort mapBase = (ushort)(_screen.Lcdc.IsBitSet(6) ? 0x9C00 : 0x9800);
			ushort mapAddress = (ushort)(mapBase + tileY * 32 + tileX);

			// Tile
			byte tile = _io.Bus.ReadDirect8(mapAddress);
			ushort tileAddress = GetTileAddress(_screen.Lcdc, tile, pixelY);

			// Tile data
			byte low = _io.Bus.ReadDirect8(tileAddress);
			byte high = _io.Bus.ReadDirect8((ushort)(tileAddress + 1));

			// Pixel color
			int bit = 7 - pixelX;
			int highBit = high.IsBitSet(bit) ? 1 : 0;
			int lowBit = low.IsBitSet(bit) ? 1 : 0;

			return (highBit << 1) | lowBit;
		}
	}
}
