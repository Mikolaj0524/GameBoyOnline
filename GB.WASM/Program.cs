using System;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.InteropServices;
using GB.GPU;

namespace GB.WASM
{
	public partial class Program
	{
		private static GameBoy? _gameBoy;

		private static readonly byte[] _registerBuffer = new byte[13], _timerBuffer = new byte[5], _interruptBuffer = new byte[2];
		private static readonly float[] _audioSamples = new float[2048];
		private static readonly IntPtr _audioPtr = Marshal.AllocHGlobal(2048 * sizeof(float) * sizeof(byte));

		public static void Main() { }

		[JSExport]
		public static void Init() => _gameBoy = new GameBoy();

		[JSExport]
		public static void SetBios(byte[] biosBytes)
		{
			if (_gameBoy == null)
				return;

			_gameBoy?.SetBios(biosBytes);
		}

		[JSExport]
		public static void SetRom(byte[] romBytes)
		{
			if (_gameBoy == null)
				return;

			_gameBoy?.SetCartridge(romBytes);
		}

		[JSExport]
		public static bool Run() => _gameBoy?.Run() ?? false;

		[JSExport]
		public static void StepCpu(int targetCycles)
		{
			if (_gameBoy == null)
				return;

			try
			{
				int totalCycles = 0;
				int errors = 0;

				while (totalCycles < targetCycles)
				{
					int stepCycles = _gameBoy.Bus.Cpu.Step();
					if (stepCycles <= 0)
					{
						stepCycles = 4;
						errors++;

						if (errors > 10000)
							break;
					}
					else
					{
						errors = 0;
					}

					totalCycles += stepCycles;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[GB.WASM] Step error occurred: {ex.Message}");
			}
		}

		[JSExport]
		public static void SetButtonState(int button, bool state)
		{
			if (_gameBoy == null)
				return;

			if (Enum.IsDefined(typeof(Controls.Button), button))
				_gameBoy.Bus.IO.Joypad.Press((Controls.Button)button, state);
		}

		[JSExport]
		public static void AdjustContrast(float delta)
		{
			if (_gameBoy == null)
				return;

			_gameBoy.Bus.IO.Screen.Contrast += delta;
		}

		[JSExport]
		public static int GetFrameBufferPtr() => (int)PPU.GetFrameBufferPtr();

		[JSExport]
		public static int GetAudioBufferPtr() => _audioPtr.ToInt32();

		[JSExport]
		public static int ReadAudioSamples()
		{
			if (_gameBoy == null)
				return 0;

			int samples = _gameBoy.Bus.IO.Apu.OutputBuffer.Read(_audioSamples, 0, _audioSamples.Length);

			if (samples > 0)
				Marshal.Copy(_audioSamples, 0, _audioPtr, samples);

			return samples;
		}

		[JSExport]
		public static int GetMemPtr(int type) => type switch
		{
			0 => (int)_gameBoy.Bus.VRam.GetPtr(),
			1 => (int)_gameBoy.Bus.WRam.GetPtr(),
			2 => (int)_gameBoy.Bus.HRam.GetPtr(),
			3 => (int)_gameBoy.Bus.Oam.GetPtr(),
			4 => (int)_gameBoy.Bus.IO.Registers.GetPtr(),
			_ => 0
		};

		[JSExport]
		public static int GetMemSize(int type) => type switch
		{
			0 => _gameBoy.Bus.VRam.Data.Length,
			1 => _gameBoy.Bus.WRam.Data.Length,
			2 => _gameBoy.Bus.HRam.Data.Length,
			3 => _gameBoy.Bus.Oam.Data.Length,
			4 => _gameBoy.Bus.IO.Registers.Data.Length,
			_ => 0
		};

		[JSExport]
		[return: JSMarshalAs<JSType.Array<JSType.Number>>]
		public static int[] GetRegisters()
		{
			if (_gameBoy == null)
				return new int[13];

			var r = _gameBoy.Bus.Cpu.Registers;
			return [
				(byte)(r.AF >> 8), (byte)r.AF,
				(byte)(r.BC >> 8), (byte)r.BC,
				(byte)(r.DE >> 8), (byte)r.DE,
				(byte)(r.HL >> 8), (byte)r.HL,
				(byte)(r.SP >> 8), (byte)r.SP,
				(byte)(r.PC >> 8), (byte)r.PC,
				r.IME ? 1 : 0
			];
		}

		[JSExport]
		[return: JSMarshalAs<JSType.Array<JSType.Number>>]
		public static int[] GetTimers()
		{
			if (_gameBoy == null)
				return new int[5];

			var timer = _gameBoy.Bus.IO.Timer;
			return [timer.Read8(0xFF04), timer.Read8(0xFF05), timer.Read8(0xFF06), timer.Read8(0xFF07)];
		}

		[JSExport]
		[return: JSMarshalAs<JSType.Array<JSType.Number>>]
		public static int[] GetInterrupts()
		{
			if (_gameBoy == null)
				return new int[2];

			var interrupts = _gameBoy.Bus.InterruptController;
			return [interrupts.IE, interrupts.IF];
		}

		[JSExport]
		public static byte[] GetSaveRam()
		{
			return _gameBoy?.Bus.Cartridge?.SaveRam();
		}

		[JSExport]
		public static void LoadSaveRam(byte[] saveData)
		{
			if (_gameBoy == null || saveData == null)
				return;

			_gameBoy?.Bus.Cartridge?.LoadRam(saveData);
		}
	}
}