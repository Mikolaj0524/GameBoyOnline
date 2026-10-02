using GB.Interfaces;
using GB.Memory;

namespace GB.Audio
{
	public class APU : IRWInterface
	{
		public const int SAMPLE_RATE = 48000;
		public const double CLOCK_RATE = 4194304.0;
		public const double CYCLES_PER_SAMPLE = CLOCK_RATE / SAMPLE_RATE;
		private static readonly float CHARGE_FACTOR = (float)Math.Pow(0.999958, CLOCK_RATE / SAMPLE_RATE);

		protected WaveRAM WaveRAM;
		public AudioBuffer OutputBuffer { get; } = new AudioBuffer(4096);
		private readonly Channel[] _channels = new Channel[4];
		protected byte _nr50, _nr51, _nr52;

		private int _fsCounter, _fsStep; // Frame Sequencer
		private double _sampleCounter, _totalLeft, _totalRight;
		private float _filterLeft, _filterRight; // HPFilter

		public APU(WaveRAM waveRAM)
		{
			WaveRAM = waveRAM;

			_channels[0] = new SquareChannel1();
			_channels[1] = new SquareChannel2();
			_channels[2] = new WaveChannel();
			_channels[3] = new NoiseChannel();

			_nr52 = 0b1111_0001;
		}

		public bool PoweredOn => (_nr52 & 0x80) != 0;

		public bool DacEnabled(int channel) => channel >= 0 && channel < 4 && _channels[channel].EnabledDac;

		public byte Read8(ushort address) => address switch
		{
			// Channel 1
			0xFF10 => (byte)(_channels[0].Registers[0] | 0x80),
			0xFF11 => (byte)(_channels[0].Registers[1] | 0x3F),
			0xFF12 => _channels[0].Registers[2],
			0xFF14 => (byte)(_channels[0].Registers[4] | 0xBF),

			// Channel 2
			0xFF16 => (byte)(_channels[1].Registers[1] | 0x3F),
			0xFF17 => _channels[1].Registers[2],
			0xFF19 => (byte)(_channels[1].Registers[4] | 0xBF),

			// Channel 3
			0xFF1A => (byte)(_channels[2].Registers[0] | 0x7F),
			0xFF1C => (byte)(_channels[2].Registers[2] | 0x9F),
			0xFF1E => (byte)(_channels[2].Registers[4] | 0xBF),

			// Channel 4
			0xFF21 => _channels[3].Registers[2],
			0xFF22 => _channels[3].Registers[3],
			0xFF23 => (byte)(_channels[3].Registers[4] | 0xBF),

			// Global Channels
			0xFF24 => _nr50,
			0xFF25 => _nr51,
			0xFF26 => GetNr52(),

			_ => 0xFF
		};

		public void Write8(ushort address, byte value)
		{
			if (!PoweredOn && address != 0xFF26)
				return;

			switch (address)
			{
				case 0xFF10:
					_channels[0].Registers[0] = value;

					break;
				case 0xFF11:
					_channels[0].Registers[1] = value;
					_channels[0].SetLength(value);

					break;
				case 0xFF12:
					_channels[0].Registers[2] = value;
					if (!DacEnabled(0))
						_channels[0].Enabled = false;

					break;
				case 0xFF13:
					_channels[0].Registers[3] = value;

					break;
				case 0xFF14:
					_channels[0].Registers[4] = value;
					_channels[0].LengthEnabled = (value & 0b0100_0000) != 0;
					if ((value & 0b1000_0000) != 0 && DacEnabled(0))
						_channels[0].Trigger(WaveRAM);

					break;
				case 0xFF16:
					_channels[1].Registers[1] = value;
					_channels[1].SetLength(value);

					break;
				case 0xFF17:
					_channels[1].Registers[2] = value;
					if (!DacEnabled(1))
						_channels[1].Enabled = false;

					break;
				case 0xFF18:
					_channels[1].Registers[3] = value;

					break;
				case 0xFF19:
					_channels[1].Registers[4] = value;
					_channels[1].LengthEnabled = (value & 0b0100_0000) != 0;
					if ((value & 0b1000_0000) != 0 && DacEnabled(1))
						_channels[1].Trigger(WaveRAM);

					break;
				case 0xFF1A:
					_channels[2].Registers[0] = value;
					if (!DacEnabled(2))
						_channels[2].Enabled = false;

					break;
				case 0xFF1B:
					_channels[2].Registers[1] = value;
					_channels[2].SetLength(value);

					break;
				case 0xFF1C:
					_channels[2].Registers[2] = value;

					break;
				case 0xFF1D:
					_channels[2].Registers[3] = value;

					break;
				case 0xFF1E:
					_channels[2].Registers[4] = value;
					_channels[2].LengthEnabled = (value & 0b0100_0000) != 0;
					if ((value & 0b1000_0000) != 0 && DacEnabled(2))
						_channels[2].Trigger(WaveRAM);

					break;
				case 0xFF20:
					_channels[3].Registers[1] = value;
					_channels[3].SetLength(value);

					break;
				case 0xFF21:
					_channels[3].Registers[2] = value;
					if (!DacEnabled(3))
						_channels[3].Enabled = false;

					break;
				case 0xFF22:
					_channels[3].Registers[3] = value;

					break;
				case 0xFF23:
					_channels[3].Registers[4] = value;
					_channels[3].LengthEnabled = (value & 0b0100_0000) != 0;
					if ((value & 0b1000_0000) != 0 && DacEnabled(3))
						_channels[3].Trigger(WaveRAM);

					break;
				case 0xFF24:
					_nr50 = value;
					break;
				case 0xFF25:
					_nr51 = value;
					break;
				case 0xFF26:
					bool on = (value & 0b1000_0000) != 0;
					if (!on && PoweredOn)
					{
						PowerDown();
					}
					else if (on && !PoweredOn)
					{
						_nr52 |= 0x80;
						_fsStep = 0;
					}
					break;
			}
		}

		public void Step(int cycles)
		{
			if (PoweredOn)
			{
				for (int i = 0; i < 4; i++)
					_channels[i].Step(cycles, WaveRAM);

				_fsCounter += cycles;
				if (_fsCounter >= 8192)
				{
					_fsCounter -= 8192;
					ClockFrameSequencer();
				}
			}

			MixChannels(out float left, out float right);

			_totalLeft += left * (double)cycles;
			_totalRight += right * (double)cycles;
			_sampleCounter += cycles;


			while (_sampleCounter >= CYCLES_PER_SAMPLE)
			{
				EmitSample((float)(_totalLeft / _sampleCounter), (float)(_totalRight / _sampleCounter), PoweredOn);

				_sampleCounter -= CYCLES_PER_SAMPLE;
				_totalLeft = 0;
				_totalRight = 0;
			}
		}

		private void ClockFrameSequencer()
		{
			if ((_fsStep & 0b0000_0001) == 0)
				for (int i = 0; i < 4; i++)
					_channels[i].ClockLength();

			if (_fsStep == 2 || _fsStep == 6)
				_channels[0].ClockSweep();

			if (_fsStep == 7)
				for (int i = 0; i < 4; i++)
					_channels[i].ClockEnvelope();

			_fsStep = (_fsStep + 1) & 0b0000_0111;
		}

		private bool MixChannels(out float left, out float right)
		{
			left = 0f;
			right = 0f;
			bool anyDac = false;

			for (int i = 0; i < 4; i++)
			{
				Channel ch = _channels[i];
				if (!ch.EnabledDac)
					continue;

				anyDac = true;
				float analog = 1.0f - ch.GetSample(WaveRAM) / 7.5f;

				if ((_nr51 & (0b0001_0000 << i)) != 0)
					left += analog;

				if ((_nr51 & (0b0000_0001 << i)) != 0)
					right += analog;
			}

			if (!anyDac)
				return false;

			float masterLeft = (((_nr50 >> 4) & 0b0000_0111) + 1) / 8.0f;
			float masterRight = ((_nr50 & 0b0000_0111) + 1) / 8.0f;

			left = (left / 4.0f) * masterLeft;
			right = (right / 4.0f) * masterRight;
			return true;
		}

		private void EmitSample(float left, float right, bool dacEnabled)
		{
			float leftOut = ApplyHpf(left, ref _filterLeft, dacEnabled);
			float rightOut = ApplyHpf(right, ref _filterRight, dacEnabled);

			leftOut = Math.Clamp(leftOut, -1.0f, 1.0f);
			rightOut = Math.Clamp(rightOut, -1.0f, 1.0f);

			OutputBuffer.Write(leftOut, rightOut);
		}

		private static float ApplyHpf(float sample, ref float cap, bool dacEnabled)
		{
			if (!dacEnabled)
				return 0.0f;

			float outSample = sample - cap;
			cap = sample - outSample * CHARGE_FACTOR;
			return outSample;
		}

		private byte GetNr52()
		{
			byte status = (byte)(_nr52 & 0b1000_0000);
			status |= 0b0111_0000;

			for (int i = 0; i < 4; i++)
				if (_channels[i].Enabled) status |= (byte)(1 << i);

			return status;
		}

		private void PowerDown()
		{
			_nr50 = 0;
			_nr51 = 0;
			_nr52 = 0;

			for (int i = 0; i < 4; i++)
				_channels[i].Disable();

			_filterLeft = 0f;
			_filterRight = 0f;
		}
	}
}