using GB.Memory;

namespace GB.Audio
{
	public class SquareChannel1 : SquareChannel2
	{
		private int _sweepTimer, _sweepPace, _shadowFreq;
		private bool _sweepEnabled;

		public override void Trigger(WaveRAM waveRam)
		{
			base.Trigger(waveRam);

			int shift = (Registers[0] & 0b0000_0111);

			_shadowFreq = GetFreq();
			_sweepPace = (Registers[0] >> 4) & 0b0000_0111;
			_sweepTimer = _sweepPace != 0 ? _sweepPace : 8;
			_sweepEnabled = _sweepPace != 0 || shift != 0;

			if (shift != 0)
				SweepFreq();
		}

		public override void ClockSweep()
		{
			_sweepTimer--;
			if (_sweepTimer > 0)
				return;

			_sweepPace = (Registers[0] >> 4) & 0b0000_0111;
			_sweepTimer = _sweepPace != 0 ? _sweepPace : 8;

			if (!_sweepEnabled || _sweepPace == 0)
				return;

			int freq = SweepFreq();
			int shift = Registers[0] & 0b0000_0111;
			if (freq <= 2047 && shift != 0)
			{
				_shadowFreq = freq;
				Registers[3] = (byte)(freq & 0b1111_1111);
				Registers[4] = (byte)((Registers[4] & 0b1111_1000) | ((freq >> 8) & 0b0000_0111));

				SweepFreq();
			}
		}

		private int SweepFreq()
		{
			int shift = Registers[0] & 0b0000_0111;
			int delta = _shadowFreq >> shift;
			bool sub = (Registers[0] & 0b0000_1000) != 0;

			int freq = sub ? _shadowFreq - delta : _shadowFreq + delta;
			if (freq > 2047)
				Enabled = false;

			return freq;
		}

		public override void Disable()
		{
			base.Disable();
			_sweepTimer = 0;
			_sweepPace = 0;
			_shadowFreq = 0;
			_sweepEnabled = false;
		}
	}
}