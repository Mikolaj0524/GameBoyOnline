using GB.Utils;

namespace GB.CPU
{
	public class Registers
	{
		/// <summary>Stack and program counter.</summary>
		public ushort SP, PC = 0x0000;

		/// <summary>CPU registers.</summary>
		public byte A, B, C, D, E, F, H, L;

		/// <summary>Interrupt master enable</summary>
		public bool IME;

		public ushort AF
		{
			get => (ushort)((A << 8) | F);
			set
			{
				A = (byte)(value >> 8);
				F = (byte)(value & 0b1111_0000);
			}
		}

		public ushort BC
		{
			get => (ushort)((B << 8) | C);
			set
			{
				B = (byte)(value >> 8);
				C = (byte)value;
			}
		}

		public ushort DE
		{
			get => (ushort)((D << 8) | E);
			set
			{
				D = (byte)(value >> 8);
				E = (byte)value;
			}
		}

		public ushort HL
		{
			get => (ushort)((H << 8) | L);
			set
			{
				H = (byte)(value >> 8);
				L = (byte)value;
			}
		}

		/// <summary>Zero flag</summary>
		public bool z
		{
			get => F.IsBitSet(7);
			set => F = value ? (byte)(F | 0b1000_0000) : (byte)(F & 0b0111_1111);
		}

		/// <summary>Subtract flag</summary>
		public bool n
		{
			get => F.IsBitSet(6);
			set => F = value ? (byte)(F | 0b0100_0000) : (byte)(F & 0b1011_1111);
		}

		/// <summary>Half-carry flag</summary>
		public bool h
		{
			get => F.IsBitSet(5);
			set => F = value ? (byte)(F | 0b0010_0000) : (byte)(F & 0b1101_1111);
		}

		/// <summary>Carry flag</summary>
		public bool c
		{
			get => F.IsBitSet(4);
			set => F = value ? (byte)(F | 0b0001_0000) : (byte)(F & 0b1110_1111);
		}

		/// <summary>Updates the CPU flags.</summary>
		public void WriteFlags(bool? z, bool? n, bool? h, bool? c)
		{
			if (z != null)
				this.z = z.Value;

			if (n != null)
				this.n = n.Value;

			if (h != null)
				this.h = h.Value;

			if (c != null)
				this.c = c.Value;
		}
	}
}
