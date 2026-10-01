using GB.Communication;
using GB.Utils;

namespace GB.GPU
{
	public partial class PPU
	{
		private void RenderScanline(byte ly)
		{
			bool bg = _screen.Lcdc.IsBitSet(0);
			bool window = _screen.Lcdc.IsBitSet(5) && bg;

			if (window && ly == _screen.Wy)
				_windowTriggered = true;

			bool visible = window && _windowTriggered && _screen.Wx <= 166;
			bool drawn = false;

			for (int x = 0; x < 160; x++)
			{
				int color = 0;
				bool drawWindow = visible && (x + 7) >= _screen.Wx;

				if (bg)
				{
					if (drawWindow)
					{
						color = GetWindowColor(x, _screen.Wx);
						drawn = true;
					}
					else
					{
						color = GetBackgroundColor(x, ly);
					}
				}

				_bgColors[x] = color;

				int index = (_screen.Bgp >> (color * 2)) & 0b0000_0011;
				byte[] colors = _screen.Palette[index];

				int offset = (x + ly * 160) * 3;
				_framebuffer[offset] = colors[0];
				_framebuffer[offset + 1] = colors[1];
				_framebuffer[offset + 2] = colors[2];
			}

			if (drawn)
				_windowLine++;

			if (!_screen.Lcdc.IsBitSet(1))
				return;

			FindSprites(ly);
			RenderSprites(ly);
		}

		private void RenderSprites(byte ly)
		{
			int height = _screen.Lcdc.IsBitSet(2) ? 16 : 8;
			bool[] drawn = new bool[160];

			for (int i = 0; i < _spriteCount; i++)
			{
				var sprite = _sprites[i];

				int top = sprite.Y - 16;
				int inLine = ly - top;

				if (sprite.FlipY)
					inLine = height - 1 - inLine;

				byte tile = sprite.Tile;
				if (height == 16)
				{
					tile &= 0b1111_1110;
					if (inLine >= 8)
					{
						tile |= 0b0000_0001;
						inLine -= 8;
					}
				}

				ushort address = (ushort)(0x8000 + tile * 16 + inLine * 2);

				byte low = _bus.ReadDirect8(address);
				byte high = _bus.ReadDirect8((ushort)(address + 1));
				byte palette = sprite.UsePalette ? _screen.Obp1 : _screen.Obp0;

				for (int px = 0; px < 8; px++)
				{
					int bit = sprite.FlipX ? px : 7 - px;
					int highBit = high.IsBitSet(bit) ? 1 : 0;
					int lowBit = low.IsBitSet(bit) ? 1 : 0;

					int color = (highBit << 1) | lowBit;
					if (color == 0)
						continue;

					int screenX = sprite.X - 8 + px;
					if (screenX < 0 || screenX >= 160 || drawn[screenX] || (sprite.BehindBg && _bgColors[screenX] != 0))
						continue;

					int index = (palette >> (color * 2)) & 0x03;
					byte[] colors = _screen.Palette[index];

					int offset = (screenX + ly * 160) * 3;
					_framebuffer[offset] = colors[0];
					_framebuffer[offset + 1] = colors[1];
					_framebuffer[offset + 2] = colors[2];

					drawn[screenX] = true;
				}
			}
		}

		private static ushort GetTileAddress(byte lcdc, byte tile, int y)
		{
			if (lcdc.IsBitSet(4))
				return (ushort)(0x8000 + tile * 16 + y * 2);

			sbyte sTile = (sbyte)tile;
			return (ushort)(0x9000 + sTile * 16 + y * 2);
		}
	}
}
