using GB.Communication;
using System.Diagnostics;

namespace GB
{
	public class GameBoy
	{
		private const int FREQUENCY = 4194304;
		public readonly BUS Bus = new();
		private Thread? _thread;
		public bool _pause, _running;

		public bool Run()
		{
			if (_running)
			{
				Console.WriteLine("[GameBoy] Emulator is already running!");
				return false;
			}

			if (Bus.Bios == null)
			{
				Console.WriteLine("[GameBoy] Missing bios.gb file!");
				return false;
			}

			if (Bus.Cartridge == null)
			{
				Console.WriteLine("[GameBoy] Missing cartridge file!");
				return false;
			}

			_running = true;
			_pause = false;

			_thread = new Thread(ThreadLoop){ 
				IsBackground = true 
			};

			_thread.Start();
			return true;
		}

		public void ThreadLoop() {
			Stopwatch stopwatch = Stopwatch.StartNew();
			ulong executedCycles = 0;

			while (true)
			{
				if (_pause)
				{
					Thread.Sleep(10);
					continue;
				}

				ulong targetCycles = (ulong)(stopwatch.Elapsed.TotalSeconds * FREQUENCY);
				if (executedCycles < targetCycles)
				{
					int cpuCycles = Bus.Cpu.Step();
					executedCycles += (ulong)cpuCycles;
				}
				else
				{
					Thread.SpinWait(1);
				}
			}
		}

		public bool Dispose()
		{
			if (!_running)
			{
				Console.WriteLine("[GameBoy] Unable to dispose emulator.\n\t- Emulator is not running!");
				return false;
			}

			_pause = true;
			if (_thread != null && _thread.IsAlive)
				_thread.Join();

			_thread = null;
			_running = false;
			_pause = false;

			return true;
		}

		public void Step(int cycles)
		{
			if (!_running)
				return;

			int executedCycles = 0;
			while (executedCycles < cycles)
			{
				int cpuCycles = Bus.Cpu.Step();
				if (cpuCycles <= 0)
					cpuCycles = 4;

				executedCycles += cpuCycles;
			}
		}

		public bool Pause(bool state)
		{
			if (!_running)
			{
				Console.WriteLine("[GameBoy] Unable to change Pause state.\n\t- Emulator is not running!");
				return false;
			}

			_pause = state;
			return true;
		}

		public void SetCartridge(byte[] rom) => Bus.SetCartridge(rom);
		public void SetBios(byte[] rom) => Bus.SetBios(rom);
	}
}
