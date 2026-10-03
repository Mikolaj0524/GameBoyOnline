
using GB.Utils;

namespace GB.GPU
{
	public partial class PPU
	{
		private readonly SpriteInfo[] _sprites = new SpriteInfo[10];
		private int _spriteCount;

		/// <summary>Finds sprites visible on the current line.</summary>
		/// <param name="ly">Current screen line.</param>
		private void FindSprites(byte ly)
		{
			int height = _screen.Lcdc.IsBitSet(2) ? 16 : 8;
			_spriteCount = 0;

			for (int i = 0; i < 40 && _spriteCount < 10; i++)
			{
				// Calcs sprite OAM address
				ushort address = (ushort)(0xFE00 + i * 4);

				// Sprite pos
				byte y = _bus.ReadDirect8(address);
				byte x = _bus.ReadDirect8((ushort)(address + 1));
				if (y == 0 || y >= 168)
					continue;

				// Sprite tile and flags
				byte tile = _bus.ReadDirect8((ushort)(address + 2));
				byte flags = _bus.ReadDirect8((ushort)(address + 3));

				int top = y - 16;
				if (ly >= top && ly < top + height)
				{
					// Save sprite info
					_sprites[_spriteCount] = new SpriteInfo
					{
						Y = y,
						X = x,
						Tile = tile,
						Flags = flags,
						Index = i
					};
					_spriteCount++;
				}
			}

			Array.Sort(_sprites, 0, _spriteCount, Comparer<SpriteInfo>.Create((a, b) =>
			{
				int compare = a.X.CompareTo(b.X);
				return compare != 0 ? compare : a.Index.CompareTo(b.Index);
			}));
		}
	}
}
