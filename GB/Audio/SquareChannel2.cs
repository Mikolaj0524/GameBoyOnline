using GB.Memory;

namespace GB.Audio
{
	public class SquareChannel2 : EnvelopeChannel
	{
		protected static readonly byte[][] DutyPatterns = [
			[0, 0, 0, 0, 0, 0, 0, 1], // 12.5 %
			[1, 0, 0, 0, 0, 0, 0, 1], // 25 %
			[1, 0, 0, 0, 0, 1, 1, 1], // 50 %
			[0, 1, 1, 1, 1, 1, 1, 0]  // 75 %
		];

		public int DutyStep;

		public override int GetCycles()
		{
			return (2048 - GetFreq()) * 4;
		}

		public override void Step(int cycles, WaveRAM waveRam)
		{
			Timer -= cycles;
			while (Timer <= 0)
			{
				Timer += GetCycles();
				DutyStep = (DutyStep + 1) & 0b0000_0111;
			}
		}

		public override int GetSample(WaveRAM waveRam)
		{
			if (!Enabled)
				return 0;

			int index = (Registers[1] >> 6) & 0b0000_0011;
			return DutyPatterns[index][DutyStep] * Volume;
		}

		public override void Disable()
		{
			base.Disable();
			DutyStep = 0;
		}
	}
}