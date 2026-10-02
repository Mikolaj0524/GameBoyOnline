using GB.Memory;

namespace GB.Audio
{
	public class NoiseChannel : EnvelopeChannel
	{
		private static readonly int[] _divisors = [8, 16, 32, 48, 64, 80, 96, 112];
		private ushort _lfsr = 0b0111_1111_1111_1111;

		public override int GetCycles()
		{
			int divisor = Registers[3] & 0b0000_0111;
			int shift = (Registers[3] >> 4) & 0b0000_1111;
			return _divisors[divisor] << shift;
		}

		public override void Trigger(WaveRAM waveRam)
		{
			base.Trigger(waveRam);
			_lfsr = 0b0111_1111_1111_1111;
		}

		public override void Step(int cycles, WaveRAM waveRam)
		{
			Timer -= cycles;
			while (Timer <= 0)
			{
				Timer += GetCycles();

				int shift = (Registers[3] >> 4) & 0b0000_1111;
				if (shift < 14)
					ClockLfsr();
			}
		}

		private void ClockLfsr()
		{
			int xor = (_lfsr & 0b0000_0001) ^ ((_lfsr >> 1) & 0b0000_0001);
			int next = (_lfsr >> 1) | (xor << 14);

			if ((Registers[3] & 0b0000_1000) != 0)
				next = (next & ~(1 << 6)) | (xor << 6);

			_lfsr = (ushort)next;
		}

		public override int GetSample(WaveRAM waveRam)
		{
			if (!Enabled)
				return 0;

			return (_lfsr & 0b0000_0001) == 0 ? Volume : 0;
		}

		public override void Disable()
		{
			base.Disable();
			_lfsr = 0b0111_1111_1111_1111;
		}
	}
}