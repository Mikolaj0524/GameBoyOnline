using GB.Memory;

namespace GB.Audio
{
	public abstract class EnvelopeChannel : Channel
	{
		public int Volume;

		/// <summary>Envelope timer and period.</summary>
		private int _envTimer, _envelopePace;

		/// <summary>Checks if the envelope increases.</summary>
		private bool _envIncrease;

		public override bool EnabledDac => (Registers[2] & 0b1111_1000) != 0;


		/// <summary>Triggers the envelope channel.</summary>
		public override void Trigger(WaveRAM waveRam)
		{
			base.Trigger(waveRam);

			Volume = (Registers[2] >> 4) & 0b0000_1111;

			// Envelope direction
			_envIncrease = (Registers[2] & 0b0000_1000) != 0;

			// Envelope period
			_envelopePace = Registers[2] & 0b0000_0111;

			// Envelope timer
			_envTimer = _envelopePace != 0 ? _envelopePace : 8;
		}


		/// <summary>Updates the volume envelope.</summary>
		public override void ClockEnvelope()
		{
			if (_envelopePace == 0)
				return;

			_envTimer--;
			if (_envTimer > 0)
				return;

			// Reset envelope timer.
			_envTimer = _envelopePace;

			if (_envIncrease)
			{
				if (Volume < 15)
					Volume++;
			}
			else if (Volume > 0)
			{
				Volume--;
			}
		}


		/// <summary>Disables the envelope channel.</summary>
		public override void Disable()
		{
			base.Disable();
			Volume = 0;
			_envTimer = 0;
			_envelopePace = 0;
			_envIncrease = false;
		}
	}
}