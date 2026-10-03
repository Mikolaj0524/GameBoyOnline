namespace GB.CPU.Instructions
{
	public partial class Instructions
	{
		/// <summary>Halts the CPU.</summary>
		private void Halt() => _cpu.Halted = true;


		/// <summary>Disables interrupts.</summary>
		private void Di() => _registers.IME = false;


		/// <summary>Enables interrupts after a delay.</summary>
		private void Ei() => _cpu.ImeDelay = 2;


		/// <summary>Stops the CPU.</summary>
		private void Stop()
		{
			ReadPc8();
			_cpu.Stopped = true;
		}


		/// <summary>Checks a CPU condition.</summary>
		private bool Cond(int cond) => cond switch
		{
			0 => !_registers.z,
			1 => _registers.z,
			2 => !_registers.c,
			3 => _registers.c,
			_ => false
		};


		/// <summary>Performs a relative jump.</summary>
		private void Jr(bool cond)
		{
			sbyte offset = (sbyte)ReadPc8();
			if (!cond)
				return;

			_bus.Tick(4);
			_registers.PC = (ushort)(_registers.PC + offset);

		}


		/// <summary>Performs an absolute jump.</summary>
		private void Jp(bool cond)
		{
			ushort address = ReadPc16();
			if (!cond)
				return;

			_bus.Tick(4);
			_registers.PC = address;
		}


		/// <summary>Jumps to the address stored in HL.</summary>
		private void JpHl() {
			_registers.PC = _registers.HL;
		}


		/// <summary>Calls a subroutine.</summary>
		private void Call(bool cond)
		{
			ushort address = ReadPc16();
			if (!cond)
				return;

			_bus.Tick(4);
			Push(_registers.PC);
			_registers.PC = address;
		}


		/// <summary>Returns from a subroutine.</summary>
		private void Ret() {
			_registers.PC = Pop();
		}


		/// <summary>Returns from a subroutine.</summary>
		private void RetUncond()
		{
			Ret();
			_bus.Tick(4);
		}


		/// <summary>Returns if the condition is met.</summary>
		private void RetCond(bool cond)
		{
			_bus.Tick(4);
			if (!cond)
				return;

			Ret();
			_bus.Tick(4);
		}


		/// <summary>Returns from an interrupt.</summary>
		private void Reti()
		{
			Ret();
			_bus.Tick(4);
			_registers.IME = true;
		}


		/// <summary>Restarts the CPU at an address.</summary>
		private void Rst(ushort address)
		{
			_bus.Tick(4);
			Push(_registers.PC);
			_registers.PC = address;
		}
	}
}
