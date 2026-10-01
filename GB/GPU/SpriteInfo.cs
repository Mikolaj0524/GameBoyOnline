using GB.Utils;

namespace GB.GPU
{
	public struct SpriteInfo
	{
		public byte X, Y, Tile, Flags;
		public int Index;

		public readonly bool BehindBg => Flags.IsBitSet(7);
		public readonly bool FlipY => Flags.IsBitSet(6);
		public readonly bool FlipX => Flags.IsBitSet(5);
		public readonly bool UsePalette => Flags.IsBitSet(4);
	}
}
