using GB.Memory;

namespace GB.Audio
{
	public class SquareChannel1 : SquareChannel2
	{
		/// <summary>Sweep state.</summary>
		private int _sweepTimer, _sweepPace, _shadowFreq;


		/// <summary>Checks if sweep is enabled.</summary>
		private bool _sweepEnabled;


		/// <summary>Triggers the square channel.</summary>
		public override void Trigger(WaveRAM waveRam)
		{
			base.Trigger(waveRam);

			// Sweep shift
			int shift = (Registers[0] & 0b0000_0111);

			// Current freq
			_shadowFreq = GetFreq();

			// Sweep period
			_sweepPace = (Registers[0] >> 4) & 0b0000_0111;

			// Sweep timer
			_sweepTimer = _sweepPace != 0 ? _sweepPace : 8;

			// Enable sweep
			_sweepEnabled = _sweepPace != 0 || shift != 0;

			// Calculate first sweep freq
			if (shift != 0)
				SweepFreq();
		}


		/// <summary>Updates the frequency sweep.</summary>
		public override void ClockSweep()
		{
			_sweepTimer--;
			if (_sweepTimer > 0)
				return;

			// Sweep period
			_sweepPace = (Registers[0] >> 4) & 0b0000_0111;

			// Reset sweep timer
			_sweepTimer = _sweepPace != 0 ? _sweepPace : 8;

			if (!_sweepEnabled || _sweepPace == 0)
				return;
				
			// Next freq
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


		/// <summary>Calculates the next sweep frequency.</summary>
		/// <returns>New frequency.</returns>
		private int SweepFreq()
		{
			// Sweep shift
			int shift = Registers[0] & 0b0000_0111;

			// Freq change
			int delta = _shadowFreq >> shift;

			// Sweep direction
			bool sub = (Registers[0] & 0b0000_1000) != 0;

			// New frequency
			int freq = sub ? _shadowFreq - delta : _shadowFreq + delta;
			if (freq > 2047)
				Enabled = false;

			return freq;
		}


		/// <summary>Disables the square channel.</summary>
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