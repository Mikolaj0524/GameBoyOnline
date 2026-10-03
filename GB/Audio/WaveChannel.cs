using GB.Memory;

namespace GB.Audio
{
	public class WaveChannel : Channel
	{
		/// <summary>Maximum channel length.</summary>
		public override int MaxLength => 256;

		/// <summary>Current wave position and sample.</summary>
		private int _waveIndex, _sampleBuffer;

		public override bool EnabledDac => (Registers[0] & 0b1000_0000) != 0;


		/// <summary>Gets the number of cycles between samples.</summary>
		public override int GetCycles()
		{
			return (2048 - GetFreq()) * 2;
		}


		/// <summary>Sets the channel length.</summary>
		public override void SetLength(byte value)
		{
			LengthCounter = MaxLength - value;
		}


		/// <summary>Triggers the wave channel.</summary>
		public override void Trigger(WaveRAM waveRam)
		{
			base.Trigger(waveRam);
			_waveIndex = 0;
		}


		/// <summary>Updates the wave channel.</summary>
		public override void Step(int cycles, WaveRAM waveRam)
		{
			// Remove elapsed cycles.
			Timer -= cycles;

			while (Timer <= 0)
			{
				// Next sample time
				Timer += GetCycles();

				// Next sample.
				_waveIndex = (_waveIndex + 1) & 0b0001_1111;

				// Wave byte
				byte sample = waveRam.ReadDirect((ushort)(_waveIndex / 2));

				// High or low sample
				_sampleBuffer = (_waveIndex & 0b0000_0001) == 0 ? (sample >> 4) : (sample & 0b0000_1111);
			}
		}


		/// <summary>Gets the current audio sample.</summary>
		/// <returns>Current sample value.</returns>
		public override int GetSample(WaveRAM waveRam)
		{
			if (!Enabled)
				return 0;

			// Volume code
			int code = (Registers[2] >> 5) & 0b0000_0011;
			int shift = code switch
			{
				0 => 4,
				1 => 0,
				2 => 1,
				_ => 2
			};

			// Apply volume
			return _sampleBuffer >> shift;
		}


		/// <summary>Disables the wave channel.</summary>
		public override void Disable()
		{
			base.Disable();
			_waveIndex = 0;
			_sampleBuffer = 0;
		}
	}
}