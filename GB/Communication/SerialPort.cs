using GB.CPU;
using GB.Interfaces;

namespace GB.Communication
{
	public class SerialPort(InterruptController interruptController) : IRWInterface
	{
		private readonly InterruptController _interruptController = interruptController;

		/// <summary>Serial transfer registers.</summary>
		public byte Stc, Stb;

		private bool _transferring;
		private int _counter;


		/// <summary>Reads a serial register.</summary>
		/// <returns>Register value.</returns>
		public byte Read8(ushort address)
		{
			if (address == 0xFF01)
				return Stb;

			if (address == 0xFF02)
				return (byte)(Stc | 0b0111_1110);

			return 0xFF;
		}


		/// <summary>Writes to a serial register.</summary>
		public void Write8(ushort address, byte value)
		{
			if (address == 0xFF01)
			{
				Stb = value;
				return;
			}

			if (address == 0xFF02) {
				Stc = value;

				if ((value & 0b1000_0001) == 0b1000_0001)
				{
					Console.Write((char)Stb);
					_transferring = true;
					_counter = 8192;
				}
			}
		}

		public void Step(int cycles)
		{
			if (!_transferring)
				return;

			_counter -= cycles;
			if (_counter <= 0)
			{
				_transferring = false;
				Stb = 0xFF;
				Stc &= 0b0111_1111;
				_interruptController.SetInterrupt(Interrupt.Serial);
			}
		}
	}
}
