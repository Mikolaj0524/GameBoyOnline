using GB.Utils;

namespace GB.GPU
{
	public partial class PPU
	{
		/// <summary>Renders current scanline.</summary>
		private void RenderScanline(byte ly)
		{
			// Is background or window enabled?
			bool bg = _screen.Lcdc.IsBitSet(0);
			bool window = _screen.Lcdc.IsBitSet(5) && bg;

			// Start the window at WY.
			if (window && ly == _screen.Wy)
				_windowTriggered = true;

			// Is window visible?
			bool visible = window && _windowTriggered && _screen.Wx <= 166;
			bool drawn = false;

			for (int x = 0; x < 160; x++)
			{
				int color = 0;

				// Is pixel inside the window?
				bool drawWindow = visible && (x + 7) >= _screen.Wx;

				if (bg)
				{
					if (drawWindow)
					{
						// Get window color.
						color = GetWindowColor(x, _screen.Wx);
						drawn = true;
					}
					else
					{
						// Get background color.
						color = GetBackgroundColor(x, ly);
					}
				}

				// Save background color
				_bgColors[x] = color;

				// Color from palette
				int index = (_screen.Bgp >> (color * 2)) & 0b0000_0011;
				byte[] colors = _screen.Palette[index];

				// Write pixel to the framebuffer.
				int offset = (x + ly * 160) * 3;
				_framebuffer[offset] = colors[0];
				_framebuffer[offset + 1] = colors[1];
				_framebuffer[offset + 2] = colors[2];
			}

			// Next line
			if (drawn)
				_windowLine++;

			// Are sprites enabled?
			if (!_screen.Lcdc.IsBitSet(1))
				return;

			// Find and render sprites
			FindSprites(ly);
			RenderSprites(ly);
		}

		/// <summary>Renders sprites on the current line.</summary>
		private void RenderSprites(byte ly)
		{
			int height = _screen.Lcdc.IsBitSet(2) ? 16 : 8;

			// Already drawn pixels
			bool[] drawn = new bool[160];

			for (int i = 0; i < _spriteCount; i++)
			{
				var sprite = _sprites[i];

				// Sprite position and line
				int top = sprite.Y - 16;
				int inLine = ly - top;

				// Flip Y
				if (sprite.FlipY)
					inLine = height - 1 - inLine;

				byte tile = sprite.Tile;

				// Get tile for 8x16 sprites
				if (height == 16)
				{
					tile &= 0b1111_1110;
					if (inLine >= 8)
					{
						tile |= 0b0000_0001;
						inLine -= 8;
					}
				}

				// Tile row address
				ushort address = (ushort)(0x8000 + tile * 16 + inLine * 2);

				// Tile data
				byte low = _bus.ReadDirect8(address);
				byte high = _bus.ReadDirect8((ushort)(address + 1));
				byte palette = sprite.UsePalette ? _screen.Obp1 : _screen.Obp0;

				for (int px = 0; px < 8; px++)
				{
					// Flip X
					int bit = sprite.FlipX ? px : 7 - px;

					int highBit = high.IsBitSet(bit) ? 1 : 0;
					int lowBit = low.IsBitSet(bit) ? 1 : 0;

					int color = (highBit << 1) | lowBit;
					if (color == 0)
						continue;

					int screenX = sprite.X - 8 + px;

					// Skip pixels outside the screen or already drawn.
					if (screenX < 0 || screenX >= 160 || drawn[screenX] || (sprite.BehindBg && _bgColors[screenX] != 0))
						continue;

					// Get color from palette
					int index = (palette >> (color * 2)) & 0x03;
					byte[] colors = _screen.Palette[index];

					// Write pixel to the framebuffer
					int offset = (screenX + ly * 160) * 3;
					_framebuffer[offset] = colors[0];
					_framebuffer[offset + 1] = colors[1];
					_framebuffer[offset + 2] = colors[2];

					// Mark pixel as drawn
					drawn[screenX] = true;
				}
			}
		}

		/// <summary>Gets the tile address.</summary>
		/// <returns>Tile address.</returns>
		private static ushort GetTileAddress(byte lcdc, byte tile, int y)
		{
			if (lcdc.IsBitSet(4))
				return (ushort)(0x8000 + tile * 16 + y * 2);

			sbyte sTile = (sbyte)tile;
			return (ushort)(0x9000 + sTile * 16 + y * 2);
		}
	}
}
