using GB.Memory;

namespace GB.Audio
{
	public class SquareChannel2 : EnvelopeChannel
	{
		/// <summary>Square wave duty patterns.</summary>
		protected static readonly byte[][] DutyPatterns = [
			[0, 0, 0, 0, 0, 0, 0, 1], // 12.5 %
			[1, 0, 0, 0, 0, 0, 0, 1], // 25 %
			[1, 0, 0, 0, 0, 1, 1, 1], // 50 %
			[0, 1, 1, 1, 1, 1, 1, 0]  // 75 %
		];

		/// <summary>Current duty cycle position.</summary>
		public int DutyStep;


		/// <summary>Gets the number of cycles between duty steps.</summary>
		public override int GetCycles()
		{
			return (2048 - GetFreq()) * 4;
		}


		/// <summary>Updates the square wave.</summary>
		public override void Step(int cycles, WaveRAM waveRam)
		{
			// Remove elapsed cycles.
			Timer -= cycles;

			while (Timer <= 0)
			{
				// Next sample time
				Timer += GetCycles();

				// Next step
				DutyStep = (DutyStep + 1) & 0b0000_0111;
			}
		}


		/// <summary>Gets the current audio sample.</summary>
		/// <returns>Current sample value.</returns>
		public override int GetSample(WaveRAM waveRam)
		{
			if (!Enabled)
				return 0;

			// Get pattern
			int index = (Registers[1] >> 6) & 0b0000_0011;
			return DutyPatterns[index][DutyStep] * Volume;
		}


		/// <summary>Disables the square channel.</summary>
		public override void Disable()
		{
			base.Disable();
			DutyStep = 0;
		}
	}
}