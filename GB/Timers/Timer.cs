using GB.CPU;
using GB.Interfaces;
using GB.Utils;

namespace GB.Timers
{
	public class Timer(InterruptController interruptController) : IRWInterface
	{
		private ushort _div;
		private byte _tima, _tma, _tac;

		private readonly InterruptController _interruptController = interruptController;


		/// <summary>Reads a timer register.</summary>
		/// <returns>Register value.</returns>
		public byte Read8(ushort address) => address switch
		{
			0xFF04 => (byte)(_div >> 8),
			0xFF05 => _tima,
			0xFF06 => _tma,
			0xFF07 => (byte)(_tac | 0b1111_1000),
			_ => throw new NotImplementedException($"[TIMER] Unimplemented Read8 address: 0x{address:X4}")
		};


		/// <summary>Writes to a timer register.</summary>
		public void Write8(ushort address, byte value)
		{
			switch (address)
			{
				case 0xFF04:
					bool current = TimerSignal();
					_div = 0;

					if (current && !TimerSignal())
						IncrementTima();
					return;
				case 0xFF05: _tima = value; return;
				case 0xFF06: _tma = value; return;
				case 0xFF07:
					current = TimerSignal();
					_tac = (byte)(value & 0b0000_0111);

					if (current && !TimerSignal())
						IncrementTima();
					return;
				default: throw new NotImplementedException($"[TIMER] Unimplemented Write8 address: 0x{address:X4}, Value: 0x{value:X2}");
			}
		}


		/// <summary>Updates the timer.</summary>
		public void Step(int cycles)
		{
			for (int i = 0; i < cycles; i++)
			{
				bool current = TimerSignal();
				_div++;

				if (current && !TimerSignal())
					IncrementTima();
			}
		}


		/// <summary>Gets current timer signal.</summary>
		/// <returns>Returns True when timer signal is active.</returns>
		private bool TimerSignal()
		{
			if (!_tac.IsBitSet(2))
				return false;

			int index = (_tac & 0b11) switch
			{
				0 => 9,
				1 => 3,
				2 => 5,
				_ => 7
			};

			return _div.IsBitSet(index);
		}


		/// <summary>Increments TIMA.</summary>
		private void IncrementTima()
		{
			if (_tima == 0xFF)
			{
				_tima = _tma;
				_interruptController.SetInterrupt(Interrupt.Timer);
				return;
			}

			_tima++;
		}
	}
}
