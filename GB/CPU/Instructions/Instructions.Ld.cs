namespace GB.CPU.Instructions;

public partial class Instructions
{
	/// <summary>Loads one register into another.</summary>
	private void LdRegReg(byte dest, byte src)
	{
		byte val = Read8(src);
		Write8(dest, val);
	}


	/// <summary>Loads an immediate value into a register.</summary>
	private void LdRegImm(byte index)
	{
		Write8(index, ReadPc8());
	}


	/// <summary>Loads an immediate value into memory at HL.</summary>
	private void LdMemHlImm()
	{
		_bus.Write8(_registers.HL, ReadPc8());
	}


	/// <summary>Loads a value using a 16-bit register.</summary>
	private void LdInd(byte group16, bool read)
	{
		ushort address = group16 switch
		{
			0 => _registers.BC,
			1 => _registers.DE,
			2 => _registers.HL++,
			3 => _registers.HL--,
			_ => 0
		};

		if (read)
		{
			_registers.A = _bus.Read8(address);
			return;
		}

		_bus.Write8(address, _registers.A);
	}


	/// <summary>Loads A from or to an IO address.</summary>
	private void Ldh(byte opcode)
	{
		ushort address = 0;
		switch (opcode)
		{
			case 0xE0 or 0xF0: address = (ushort)(0xFF00 + ReadPc8()); break;
			case 0xE2 or 0xF2: address = (ushort)(0xFF00 + _registers.C); break;
			case 0xEA or 0xFA: address = ReadPc16(); break;
		}

		if ((opcode & 0b0001_0000) != 0)
		{
			_registers.A = _bus.Read8(address);
			return;
		}

		_bus.Write8(address, _registers.A);
	}


	/// <summary>Loads an immediate value into a 16-bit register.</summary>
	private void Ld16Imm(int index) {
		Write16(index, ReadPc16());
	}


	/// <summary>Loads HL into SP.</summary>
	private void LdSpHl()
	{
		_registers.SP = _registers.HL;
		_bus.Tick(4);
	}


	/// <summary>Stores SP at a memory address.</summary>
	private void LdA16Sp()
	{
		ushort address = ReadPc16();
		_bus.Write16(address, _registers.SP);
	}


	/// <summary>Loads SP plus a signed value into HL.</summary>
	private void LdHlSpE8()
	{
		byte rawVal = ReadPc8();
		ushort sp = _registers.SP;
		_registers.z = false;
		_registers.n = false;
		_registers.h = ((sp & 0b0000_1111) + (rawVal & 0b0000_1111)) > 0b0000_1111;
		_registers.c = ((sp & 0b1111_1111) + rawVal) > 0b1111_1111;
		_bus.Tick(4);
		_registers.HL = (ushort)(sp + (sbyte)rawVal);
	}


	/// <summary>Pushes a 16-bit register onto the stack.</summary>
	private void PushGroup(int index)
	{
		_bus.Tick(4);
		Push(ReadGroup16(index));
	}


	/// <summary>Pops a value from the stack into a 16-bit register.</summary>
	private void PopGroup(int index) {
		WriteGroup16(index, Pop());
	}
}
