namespace GB.CPU.Instructions;

public partial class Instructions
{
	/// <summary>Executes an ALU operation on a register.</summary>
	private void AluReg(byte op, byte src)
	{
		byte val = Read8(src);
		AluOp(op, val);
	}


	/// <summary>Executes an ALU operation with an immediate value.</summary>
	private void AluImm(byte op) { 
		AluOp(op, ReadPc8());
	}


	/// <summary>Increments an 8-bit register.</summary>
	private void IncReg(byte index)
	{
		byte val = Read8(index);
		bool h = (val & 0b0000_1111) == 0b0000_1111;
		val++;
		Write8(index, val);
		_registers.WriteFlags(val == 0, false, h, null);
	}


	/// <summary>Decrements an 8-bit register.</summary>
	private void DecReg(byte index)
	{
		byte val = Read8(index);
		bool h = (val & 0b0000_1111) == 0b0000_0000;
		val--;
		Write8(index, val);
		_registers.WriteFlags(val == 0, true, h, null);
	}


	/// <summary>Increments the value at HL.</summary>
	private void IncHl()
	{
		byte val = _bus.Read8(_registers.HL);
		bool h = (val & 0b0000_1111) == 0b0000_1111;
		val++;
		_bus.Write8(_registers.HL, val);
		_registers.WriteFlags(val == 0, false, h, null);
	}


	/// <summary>Decrements the value at HL.</summary>
	private void DecHl()
	{
		byte val = _bus.Read8(_registers.HL);
		bool h = (val & 0b0000_1111) == 0b0000_0000;
		val--;
		_bus.Write8(_registers.HL, val);
		_registers.WriteFlags(val == 0, true, h, null);
	}


	/// <summary>Increments a 16-bit register.</summary>
	private void IncReg16(int index)
	{
		Write16(index, (ushort)(Read16(index) + 1));
		_bus.Tick(4);
	}


	/// <summary>Decrements a 16-bit register.</summary>
	private void DecReg16(int index)
	{
		Write16(index, (ushort)(Read16(index) - 1));
		_bus.Tick(4);
	}


	/// <summary>Adds a 16-bit register to HL.</summary>
	private void AddHlReg16(int index)
	{
		AddHl(Read16(index));
		_bus.Tick(4);
	}


	/// <summary>Adds a signed value to SP.</summary>
	private void AddSpE8()
	{
		byte val = ReadPc8();
		ushort sp = _registers.SP;
		_registers.z = false;
		_registers.n = false;
		_registers.h = ((sp & 0b0000_1111) + (val & 0b0000_1111)) > 0b0000_1111;
		_registers.c = ((sp & 0b1111_1111) + val) > 0b1111_1111;
		_bus.Tick(8);
		_registers.SP = (ushort)(sp + (sbyte)val);
	}


	/// <summary>Rotates the accumulator.</summary>
	private void AccRot(byte opcode)
	{
		switch ((opcode >> 3) & 0b0000_0011)
		{
			case 0: _registers.A = Rlc(_registers.A); break;
			case 1: _registers.A = Rrc(_registers.A); break;
			case 2: _registers.A = Rl(_registers.A); break;
			case 3: _registers.A = Rr(_registers.A); break;
		}

		_registers.z = false;
	}


	/// <summary>Executes an accumulator operation.</summary>
	private void AccMisc(byte opcode)
	{
		switch ((opcode >> 3) & 0b0000_0011)
		{
			case 0: Daa(); break;
			case 1:
				_registers.A = (byte)~_registers.A;
				_registers.n = true;
				_registers.h = true;
				break;
			case 2:
				_registers.n = false;
				_registers.h = false;
				_registers.c = true;
				break;
			case 3:
				_registers.n = false;
				_registers.h = false;
				_registers.c = !_registers.c;
				break;
		}
	}

	/// <summary>Adjusts the accumulator for BCD.</summary>
	private void Daa()
	{
		int value = _registers.A;
		if (!_registers.n)
		{
			if (_registers.c || value > 0x99)
			{
				value += 0x60;
				_registers.c = true;
			}

			if (_registers.h || (value & 0b0000_1111) > 0b0000_1001)
				value += 0x06;
		}
		else
		{
			if (_registers.c)
				value -= 0x60;

			if (_registers.h)
				value -= 0x06;
		}

		_registers.A = (byte)value;
		_registers.z = _registers.A == 0;
		_registers.h = false;
	}


	/// <summary>Executes an ALU operation.</summary>
	private void AluOp(byte op, byte value)
	{
		switch (op)
		{
			case 0: Add(value); break;
			case 1: Adc(value); break;
			case 2: Sub(value); break;
			case 3: Sbc(value); break;
			case 4: And(value); break;
			case 5: Xor(value); break;
			case 6: Or(value); break;
			case 7: Cp(value); break;
		}
	}


	/// <summary>Adds a value to the accumulator.</summary>
	private void Add(byte value)
	{
		int result = _registers.A + value;

		_registers.z = (byte)result == 0;
		_registers.n = false;
		_registers.h = ((_registers.A & 0b0000_1111) + (value & 0b0000_1111)) > 0b0000_1111;
		_registers.c = result > 0b1111_1111;
		_registers.A = (byte)result;
	}


	/// <summary>Adds a value and the carry flag to the accumulator.</summary>
	private void Adc(byte value)
	{
		int carry = _registers.c ? 1 : 0;
		int result = _registers.A + value + carry;

		_registers.z = (byte)result == 0;
		_registers.n = false;
		_registers.h = ((_registers.A & 0b0000_1111) + (value & 0b0000_1111) + carry) > 0b0000_1111;
		_registers.c = result > 0b1111_1111;
		_registers.A = (byte)result;
	}


	/// <summary>Subtracts a value from the accumulator.</summary>
	private void Sub(byte value)
	{
		int result = _registers.A - value;

		_registers.z = (byte)result == 0;
		_registers.n = true;
		_registers.h = (_registers.A & 0b0000_1111) < (value & 0b0000_1111);
		_registers.c = _registers.A < value;
		_registers.A = (byte)result;
	}


	/// <summary>Subtracts a value and the carry flag from the accumulator.</summary>
	private void Sbc(byte value)
	{
		int carry = _registers.c ? 1 : 0;
		int result = _registers.A - value - carry;

		_registers.z = (byte)result == 0;
		_registers.n = true;
		_registers.h = (_registers.A & 0b0000_1111) < ((value & 0b0000_1111) + carry);
		_registers.c = _registers.A < (value + carry);
		_registers.A = (byte)result;
	}


	/// <summary>Performs a bitwise AND.</summary>
	private void And(byte value)
	{
		_registers.A &= value;
		_registers.WriteFlags(_registers.A == 0, false, true, false);
	}


	/// <summary>Performs a bitwise XOR.</summary>
	private void Xor(byte value)
	{
		_registers.A ^= value;
		_registers.WriteFlags(_registers.A == 0, false, false, false);
	}


	/// <summary>Performs a bitwise OR.</summary>
	private void Or(byte value)
	{
		_registers.A |= value;
		_registers.WriteFlags(_registers.A == 0, false, false, false);
	}


	/// <summary>Compares a value with the accumulator.</summary>
	private void Cp(byte value)
	{
		int result = _registers.A - value;

		_registers.z = (byte)result == 0;
		_registers.n = true;
		_registers.h = (_registers.A & 0b0000_1111) < (value & 0b0000_1111);
		_registers.c = _registers.A < value;
	}


	/// <summary>Adds a 16-bit value to HL.</summary>
	private void AddHl(ushort value)
	{
		int result = _registers.HL + value;

		_registers.n = false;
		_registers.h = ((_registers.HL & 0b0000_1111_1111_1111) + (value & 0b0000_1111_1111_1111)) > 0b0000_1111_1111_1111;
		_registers.c = result > 0b1111_1111_1111_1111;
		_registers.HL = (ushort)result;
	}
}