using GB.Memory;

namespace GB.Audio
{
	public class WaveChannel : Channel
	{
		public override int MaxLength => 256;
		private int _waveIndex, _sampleBuffer;

		public override bool EnabledDac => (Registers[0] & 0b1000_0000) != 0;

		public override int GetCycles()
		{
			return (2048 - GetFreq()) * 2;
		}

		public override void SetLength(byte value)
		{
			LengthCounter = MaxLength - value;
		}

		public override void Trigger(WaveRAM waveRam)
		{
			base.Trigger(waveRam);
			_waveIndex = 0;
		}

		public override void Step(int cycles, WaveRAM waveRam)
		{
			Timer -= cycles;
			while (Timer <= 0)
			{
				Timer += GetCycles();
				_waveIndex = (_waveIndex + 1) & 0b0001_1111;

				byte sample = waveRam.ReadDirect((ushort)(_waveIndex / 2));
				_sampleBuffer = (_waveIndex & 0b0000_0001) == 0 ? (sample >> 4) : (sample & 0b0000_1111);
			}
		}

		public override int GetSample(WaveRAM waveRam)
		{
			if (!Enabled)
				return 0;

			int code = (Registers[2] >> 5) & 0b0000_0011;
			int shift = code switch
			{
				0 => 4,
				1 => 0,
				2 => 1,
				_ => 2
			};

			return _sampleBuffer >> shift;
		}

		public override void Disable()
		{
			base.Disable();
			_waveIndex = 0;
			_sampleBuffer = 0;
		}
	}
}