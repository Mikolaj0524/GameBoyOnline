using System.Numerics;
using System.Runtime.CompilerServices;

namespace GB.Utils
{
	public static class BitUtils
	{

		/// <summary>Gets the value of a bit.</summary>
		/// <returns>Bit value.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetBit<T>(this T value, int bit) where T : struct, IBinaryInteger<T>
		{
			return int.CreateTruncating((value >>> bit) & T.One);
		}

		/// <summary>Checks if a bit is set.</summary>
		/// <returns>True if the bit is set.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsBitSet<T>(this T value, int bit) where T : struct, IBinaryInteger<T>
		{
			return ((value >>> bit) & T.One) != T.Zero;
		}
	}
}
