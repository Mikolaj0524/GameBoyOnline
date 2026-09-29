namespace GB.CPU.Instructions
{
	public partial class Instructions
	{
		private void Halt() => _cpu.Halted = true;
		private void Di() => _registers.IME = false;
		private void Ei() => _cpu.ImeDelay = 2;

		private void Stop()
		{
			ReadPc8();
			_cpu.Stopped = true;
		}

		private bool Cond(int cond) => cond switch
		{
			0 => !_registers.z,
			1 => _registers.z,
			2 => !_registers.c,
			3 => _registers.c,
			_ => false
		};

		private void Jr(bool cond)
		{
			sbyte offset = (sbyte)ReadPc8();
			if (!cond)
				return;

			_bus.Tick(4);
			_registers.PC = (ushort)(_registers.PC + offset);

		}

		private void Jp(bool cond)
		{
			ushort address = ReadPc16();
			if (!cond)
				return;

			_bus.Tick(4);
			_registers.PC = address;
		}

		private void JpHl() {
			_registers.PC = _registers.HL;
		}

		private void Call(bool cond)
		{
			ushort address = ReadPc16();
			if (!cond)
				return;

			_bus.Tick(4);
			Push(_registers.PC);
			_registers.PC = address;
		}

		private void Ret() {
			_registers.PC = Pop();
		}

		private void RetUncond()
		{
			Ret();
			_bus.Tick(4);
		}

		private void RetCond(bool cond)
		{
			_bus.Tick(4);
			if (!cond)
				return;

			Ret();
			_bus.Tick(4);
		}

		private void Reti()
		{
			Ret();
			_bus.Tick(4);
			_registers.IME = true;
		}

		private void Rst(ushort address)
		{
			_bus.Tick(4);
			Push(_registers.PC);
			_registers.PC = address;
		}
	}
}
