using System.Numerics;
using System.Runtime.CompilerServices;

namespace GB.Utils
{
	public static class BitUtils
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetBit<T>(this T value, int bit) where T : struct, IBinaryInteger<T>
		{
			return int.CreateTruncating((value >>> bit) & T.One);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsBitSet<T>(this T value, int bit) where T : struct, IBinaryInteger<T>
		{
			return ((value >>> bit) & T.One) != T.Zero;
		}
	}
}
