using GB.Communication;
using GB.Utils;
using System.Runtime.InteropServices;

namespace GB.GPU
{
	public partial class PPU
	{
		private readonly IO _io;
		private readonly BUS _bus;
		private readonly Screen _screen;

		/// <summary>Current cycle counter.</summary>
		private int _dotCounter;

		/// <summary>Current screen line.</summary>
		private byte _ly;
		private bool _statInterrupt;

		/// <summary>Called when a frame is ready.</summary>
		public Action<byte[]>? FrameReady;

		private readonly byte[] _framebuffer = GC.AllocateArray<byte>(160 * 144 * 3, pinned: true);
		private static PPU? _instance;
		private readonly IntPtr _frameBufferPtr;

		public static IntPtr GetFrameBufferPtr() => _instance?._frameBufferPtr ?? IntPtr.Zero;

		public PPU(IO io)
		{
			_io = io;
			_screen = io.Screen;
			_bus = io.Bus;

			_instance = this;
			_frameBufferPtr = Marshal.UnsafeAddrOfPinnedArrayElement(_framebuffer, 0);
		}


		/// <summary>Updates the PPU.</summary>
		public void Step(int cycles)
		{
			if (!_screen.Lcdc.IsBitSet(7))
			{
				_dotCounter = 0;
				_ly = 0;
				_screen.Ly = 0;

				// Reset the mode.
				_screen.Stat &= 0b1111_1100;
				_statInterrupt = false;
				return;
			}

			_dotCounter += cycles;

			if (_dotCounter >= 456)
			{
				_dotCounter -= 456;
				_ly++;

				// Start a new frame
				if (_ly > 153)
				{
					_ly = 0;
					_windowTriggered = false;
					_windowLine = 0;
				}

				// Update current screen line.
				_io.Screen.Ly = _ly;

				// VBlank
				if (_ly == 144)
				{
					_bus.InterruptController.SetInterrupt(CPU.Interrupt.VBlank);

					// Frame completed
					if(!OperatingSystem.IsBrowser())
						FrameReady?.Invoke(_framebuffer);
				}
			}

			// Update STAT
			_screen.Stat = (byte)((_ly == _screen.Lyc) ? (_screen.Stat | 0b0000_0100) : (_screen.Stat & 0b1111_1011));
			UpdateStat();
		}

		private void UpdateStat()
		{
			// Get the current mode.
			int mode = (_ly >= 144 ? 1 : (_dotCounter < 80 ? 2 : (_dotCounter < 252 ? 3 : 0)));
			if ((_screen.Stat & 0b0000_0011) != mode)
			{
				// Update the current mode.
				_screen.Stat = (byte)((_screen.Stat & ~0b0000_0011) | mode);

				// Draw scanline when drawing is finished
				if (mode == 0 && _ly < 144)
					RenderScanline(_ly);
			}

			// Check for STAT interrupt
			bool lycEqLy = _screen.Stat.IsBitSet(6) && _screen.Stat.IsBitSet(2);
			bool mode2 = _screen.Stat.IsBitSet(5) && mode == 2;
			bool mode1 = _screen.Stat.IsBitSet(4) && mode == 1;
			bool mode0 = _screen.Stat.IsBitSet(3) && mode == 0;

			bool stat = lycEqLy || mode2 || mode1 || mode0;
			if (!_statInterrupt && stat)
				_bus.InterruptController.SetInterrupt(CPU.Interrupt.LCDStat);

			// Save interrupt state
			_statInterrupt = stat;
		}
	}
}
