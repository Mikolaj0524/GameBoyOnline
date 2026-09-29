namespace GB.CPU.Instructions;

public partial class Instructions
{
	private void ExecuteCB()
	{
		byte opcode = ReadPc8();
		byte reg = (byte)(opcode & 0b0000_0111);
		byte bit = (byte)((opcode >> 3) & 0b0000_0111);
		byte val = Read8(reg);

		if (opcode <= 0x3F)
		{
			val = bit switch
			{
				0 => Rlc(val),
				1 => Rrc(val),
				2 => Rl(val),
				3 => Rr(val),
				4 => Sla(val),
				5 => Sra(val),
				6 => Swap(val),
				7 => Srl(val),
				_ => val
			};
			Write8(reg, val);
		}
		else if (opcode <= 0x7F)
		{
			Bit(bit, val);
		}
		else if (opcode <= 0xBF)
		{
			val &= (byte)~(1 << bit);
			Write8(reg, val);
		}
		else
		{
			val |= (byte)(1 << bit);
			Write8(reg, val);
		}
	}

	private void Bit(int bit, byte value)
	{
		bool z = (value & (1 << bit)) == 0;
		_registers.WriteFlags(z, false, true, null);
	}

	private byte Rlc(byte val)
	{
		bool c = (val & 0b1000_0000) != 0;
		val = (byte)((val << 1) | (c ? 1 : 0));

		_registers.WriteFlags(val == 0, false, false, c);
		return val;
	}

	private byte Rrc(byte val)
	{
		bool c = (val & 0b0000_0001) != 0;
		val = (byte)((val >> 1) | (c ? 0b1000_0000 : 0));

		_registers.WriteFlags(val == 0, false, false, c);
		return val;
	}

	private byte Rl(byte val)
	{
		bool c = (val & 0b1000_0000) != 0;
		val = (byte)((val << 1) | (_registers.c ? 1 : 0));

		_registers.WriteFlags(val == 0, false, false, c);
		return val;
	}

	private byte Rr(byte val)
	{
		bool c = (val & 0b0000_0001) != 0;
		val = (byte)((val >> 1) | (_registers.c ? 0b1000_0000 : 0));

		_registers.WriteFlags(val == 0, false, false, c);
		return val;
	}

	private byte Sla(byte val)
	{
		bool c = (val & 0b1000_0000) != 0;
		val <<= 1;

		_registers.WriteFlags(val == 0, false, false, c);
		return val;
	}

	private byte Sra(byte val)
	{
		bool c = (val & 0b0000_0001) != 0;
		val = (byte)((val >> 1) | (val & 0b1000_0000));

		_registers.WriteFlags(val == 0, false, false, c);
		return val;
	}

	private byte Srl(byte val)
	{
		bool c = (val & 0b0000_0001) != 0;
		val >>= 1;

		_registers.WriteFlags(val == 0, false, false, c);
		return val;
	}

	private byte Swap(byte val)
	{
		val = (byte)((val << 4) | (val >> 4));

		_registers.WriteFlags(val == 0, false, false, false);
		return val;
	}
}
