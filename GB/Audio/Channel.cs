using GB.Memory;

namespace GB.Audio
{
	public abstract class Channel
	{
		public byte[] Registers = new byte[5];
		public bool Enabled, LengthEnabled;
		public int LengthCounter, Timer;

		public virtual int MaxLength => 64;
		public abstract bool EnabledDac { get; }

		public int GetFreq()
		{
			return Registers[3] | ((Registers[4] & 0b0000_0111) << 8);
		}

		public abstract int GetCycles();

		public virtual void SetLength(byte value)
		{
			LengthCounter = MaxLength - (value & 0b0011_1111);
		}

		public virtual void Trigger(WaveRAM waveRam)
		{
			Enabled = true;

			if (LengthCounter == 0)
				LengthCounter = MaxLength;

			Timer = GetCycles();

			if (!EnabledDac)
				Enabled = false;
		}

		public void ClockLength()
		{
			if (!LengthEnabled || LengthCounter <= 0)
				return;

			LengthCounter--;
			if (LengthCounter == 0)
				Enabled = false;
		}

		public abstract void Step(int cycles, WaveRAM waveRam);
		public abstract int GetSample(WaveRAM waveRam);
		public virtual void ClockEnvelope() { }
		public virtual void ClockSweep() { }
		public virtual void Disable()
		{
			Array.Clear(Registers, 0, Registers.Length);
			Enabled = false;
			LengthEnabled = false;
		}
	}
}