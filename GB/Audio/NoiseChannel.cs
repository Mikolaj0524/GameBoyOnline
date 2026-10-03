using GB.Memory;

namespace GB.Audio
{
	public class NoiseChannel : EnvelopeChannel
	{
		/// <summary>Noise channel frequency divisors.</summary>
		private static readonly int[] _divisors = [8, 16, 32, 48, 64, 80, 96, 112];


		/// <summary>Linear feedback shift register.</summary>
		private ushort _lfsr = 0b0111_1111_1111_1111;


		/// <summary>Gets the number of cycles between LFSR steps.</summary>
		public override int GetCycles()
		{
			// Get divisor and shift
			int divisor = Registers[3] & 0b0000_0111;
			int shift = (Registers[3] >> 4) & 0b0000_1111;
			return _divisors[divisor] << shift;
		}


		/// <summary>Triggers the noise channel.</summary>
		public override void Trigger(WaveRAM waveRam)
		{
			base.Trigger(waveRam);
			_lfsr = 0b0111_1111_1111_1111;
		}


		/// <summary>Updates the noise channel.</summary>
		public override void Step(int cycles, WaveRAM waveRam)
		{
			// Remove elapsed cycles.
			Timer -= cycles;

			while (Timer <= 0)
			{
				// Next sample time
				Timer += GetCycles();

				// Freq shift
				int shift = (Registers[3] >> 4) & 0b0000_1111;
				if (shift < 14)
					ClockLfsr();
			}
		}


		/// <summary>Updates the noise shift register.</summary>
		private void ClockLfsr()
		{
			int xor = (_lfsr & 0b0000_0001) ^ ((_lfsr >> 1) & 0b0000_0001);

			// Shift and add the new bit
			int next = (_lfsr >> 1) | (xor << 14);

			// Use 7-bit mode if enabled
			if ((Registers[3] & 0b0000_1000) != 0)
				next = (next & ~(1 << 6)) | (xor << 6);

			// Save new value
			_lfsr = (ushort)next;
		}


		/// <summary>Gets the current audio sample.</summary>
		/// <returns>Current sample value.</returns>
		public override int GetSample(WaveRAM waveRam)
		{
			if (!Enabled)
				return 0;

			return (_lfsr & 0b0000_0001) == 0 ? Volume : 0;
		}

		/// <summary>Disables the noise channel.</summary>
		public override void Disable()
		{
			base.Disable();
			_lfsr = 0b0111_1111_1111_1111;
		}
	}
}