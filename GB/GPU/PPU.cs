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

		private int _dotCounter;
		private byte _ly;
		private bool _statInterrupt;

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

		public void Step(int cycles)
		{
			if (!_screen.Lcdc.IsBitSet(7))
			{
				_dotCounter = 0;
				_ly = 0;
				_screen.Ly = 0;

				_screen.Stat &= 0b1111_1100;
				_statInterrupt = false;
				return;
			}

			_dotCounter += cycles;
			if (_dotCounter >= 456)
			{
				_dotCounter -= 456;
				_ly++;

				if (_ly > 153)
				{
					_ly = 0;
					_windowTriggered = false;
					_windowLine = 0;
				}

				_io.Screen.Ly = _ly;
				if (_ly == 144)
				{
					_bus.InterruptController.SetInterrupt(CPU.Interrupt.VBlank);

					FrameReady?.Invoke(_framebuffer);
				}
			}

			_screen.Stat = (byte)((_ly == _screen.Lyc) ? (_screen.Stat | 0b0000_0100) : (_screen.Stat & 0b1111_1011));
			UpdateStat();
		}

		private void UpdateStat()
		{
			int mode = (_ly >= 144 ? 1 : (_dotCounter < 80 ? 2 : (_dotCounter < 252 ? 3 : 0)));
			if ((_screen.Stat & 0b0000_0011) != mode)
			{
				_screen.Stat = (byte)((_screen.Stat & ~0b0000_0011) | mode);
				if (mode == 0 && _ly < 144)
					RenderScanline(_ly);
			}

			bool lycEqLy = _screen.Stat.IsBitSet(6) && _screen.Stat.IsBitSet(2);
			bool mode2 = _screen.Stat.IsBitSet(5) && mode == 2;
			bool mode1 = _screen.Stat.IsBitSet(4) && mode == 1;
			bool mode0 = _screen.Stat.IsBitSet(3) && mode == 0;

			bool stat = lycEqLy || mode2 || mode1 || mode0;
			if (!_statInterrupt && stat)
				_bus.InterruptController.SetInterrupt(CPU.Interrupt.LCDStat);

			_statInterrupt = stat;
		}
	}
}
