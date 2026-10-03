using GB.Memory;

namespace GB.Audio
{
	public abstract class Channel
	{
		/// <summary>Channel registers.</summary>
		public byte[] Registers = new byte[5];


		/// <summary>Channel state.</summary>
		public bool Enabled, LengthEnabled;


		/// <summary>Length counter and frequency timer.</summary>
		public int LengthCounter, Timer;


		/// <summary>Maximum channel length.</summary>
		public virtual int MaxLength => 64;


		/// <summary>Checks if the DAC is enabled.</summary>
		public abstract bool EnabledDac { get; }


		/// <summary>Gets the channel frequency.</summary>
		/// <returns>Frequency value.</returns>
		public int GetFreq()
		{
			return Registers[3] | ((Registers[4] & 0b0000_0111) << 8);
		}


		/// <summary>Gets the number of cycles between channel updates.</summary>
		/// <returns>Number of cycles.</returns>
		public abstract int GetCycles();


		/// <summary>Sets the channel length.</summary>
		public virtual void SetLength(byte value)
		{
			LengthCounter = MaxLength - (value & 0b0011_1111);
		}


		/// <summary>Triggers the channel.</summary>
		public virtual void Trigger(WaveRAM waveRam)
		{
			Enabled = true;

			if (LengthCounter == 0)
				LengthCounter = MaxLength;

			Timer = GetCycles();

			if (!EnabledDac)
				Enabled = false;
		}


		/// <summary>Updates the channel length.</summary>
		public void ClockLength()
		{
			if (!LengthEnabled || LengthCounter <= 0)
				return;

			LengthCounter--;
			if (LengthCounter == 0)
				Enabled = false;
		}


		/// <summary>Updates the audio channel.</summary>
		public abstract void Step(int cycles, WaveRAM waveRam);


		/// <summary>Gets the current audio sample.</summary>
		/// <returns>Current sample value.</returns>
		public abstract int GetSample(WaveRAM waveRam);


		/// <summary>Updates the volume envelope.</summary>
		public virtual void ClockEnvelope() { }


		/// <summary>Updates the frequency sweep.</summary>
		public virtual void ClockSweep() { }


		/// <summary>Disables the channel.</summary>
		public virtual void Disable()
		{
			Array.Clear(Registers, 0, Registers.Length);
			Enabled = false;
			LengthEnabled = false;
		}
	}
}