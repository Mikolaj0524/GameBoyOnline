using GB.Memory;

namespace GB.Audio
{
	public abstract class EnvelopeChannel : Channel
	{
		public int Volume;
		private int _envTimer, _envelopePace;
		private bool _envIncrease;

		public override bool EnabledDac => (Registers[2] & 0b1111_1000) != 0;

		public override void Trigger(WaveRAM waveRam)
		{
			base.Trigger(waveRam);

			Volume = (Registers[2] >> 4) & 0b0000_1111;
			_envIncrease = (Registers[2] & 0b0000_1000) != 0;
			_envelopePace = Registers[2] & 0b0000_0111;
			_envTimer = _envelopePace != 0 ? _envelopePace : 8;
		}

		public override void ClockEnvelope()
		{
			if (_envelopePace == 0)
				return;

			_envTimer--;
			if (_envTimer > 0)
				return;

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