using GB.Communication;

namespace GB.CPU.Instructions
{
	public partial class Instructions(CPU cpu)
	{
		private readonly CPU _cpu = cpu;
		private readonly Registers _registers = cpu.Registers;
		private readonly BUS _bus = cpu.Bus;

		public void Execute(byte opcode) {
			byte dst = (byte)((opcode >> 3) & 0b0000_0111);
			byte src = (byte)(opcode & 0b0000_0111);
			byte group = (byte)((opcode >> 3) & 0b0000_0111);
			byte group16 = (byte)((opcode >> 4) & 0b0000_0011);

			switch (opcode) {
				case 0x00: /* NOP */ return;
				case 0x76: Halt(); return;
				case 0xF3: Di(); return;
				case 0xFB: Ei(); return;

				case >= 0x40 and <= 0x7F: LdRegReg(dst, src); return;
				case >= 0x80 and <= 0xBF: AluReg(group, src); return;
				case 0x06 or 0x0E or 0x16 or 0x1E or 0x26 or 0x2E or 0x3E: LdRegImm(dst); return;
				case 0x36: LdMemHlImm(); return;
				case 0x02 or 0x0A or 0x12 or 0x1A or 0x22 or 0x2A or 0x32 or 0x3A: LdInd(group16, read: (opcode & 0x08) != 0); return;
				case 0xE0 or 0xF0 or 0xE2 or 0xF2 or 0xEA or 0xFA: Ldh(opcode); return;
				case 0x01 or 0x11 or 0x21 or 0x31: Ld16Imm(group16); return;
				case 0xF9: LdSpHl(); return;
				case 0x08: LdA16Sp(); return;
				case 0xF8: LdHlSpE8(); return;
				case 0xC5 or 0xD5 or 0xE5 or 0xF5: PushGroup(group16); return;
				case 0xC1 or 0xD1 or 0xE1 or 0xF1: PopGroup(group16); return;
				case 0xC6 or 0xCE or 0xD6 or 0xDE or 0xE6 or 0xEE or 0xF6 or 0xFE: AluImm(group); return;
				case 0x04 or 0x0C or 0x14 or 0x1C or 0x24 or 0x2C or 0x3C: IncReg(dst); return;
				case 0x34: IncHl(); return;
				case 0x05 or 0x0D or 0x15 or 0x1D or 0x25 or 0x2D or 0x3D: DecReg(dst); return;
				case 0x35: DecHl(); return;
				case 0x03 or 0x13 or 0x23 or 0x33: IncReg16(group16); return;
				case 0x0B or 0x1B or 0x2B or 0x3B: DecReg16(group16); return;
				case 0x09 or 0x19 or 0x29 or 0x39: AddHlReg16(group16); return;
				case 0xE8: AddSpE8(); return;
				case 0x07 or 0x0F or 0x17 or 0x1F: AccRot(opcode); return;
				case 0x27 or 0x2F or 0x37 or 0x3F: AccMisc(opcode); return;
				case 0x18: Jr(true); return;
				case 0x20 or 0x28 or 0x30 or 0x38: Jr(Cond(group & 0x03)); return;
				case 0xC3: Jp(true); return;
				case 0xC2 or 0xCA or 0xD2 or 0xDA: Jp(Cond(group & 0x03)); return;
				case 0xE9: JpHl(); return;
				case 0xCD: Call(true); return;
				case 0xC4 or 0xCC or 0xD4 or 0xDC: Call(Cond(group & 0x03)); return;
				case 0xC9: RetUncond(); return;
				case 0xC0 or 0xC8 or 0xD0 or 0xD8: RetCond(Cond(group & 0x03)); return;
				case 0xD9: Reti(); return;
				case 0xC7 or 0xCF or 0xD7 or 0xDF or 0xE7 or 0xEF or 0xF7 or 0xFF: Rst((ushort)(opcode & 0x38)); return;
				
				case 0xCB: ExecuteCB(); return;
				case 0x10: Stop(); return;

				default: throw new NotImplementedException($"[CPU] Not found instruction with 0x{opcode:X2} opcode! PC: {_registers.PC - 1}");
			}
		}


		/// <summary>Reads an 8-bit register.</summary>
		/// <returns>Register value.</returns>
		private byte Read8(byte index) => index switch
		{
			0 => _registers.B,
			1 => _registers.C,
			2 => _registers.D,
			3 => _registers.E,
			4 => _registers.H,
			5 => _registers.L,
			6 => _bus.Read8(_registers.HL),
			7 => _registers.A,
			_ => 0
		};


		/// <summary>Writes to an 8-bit register.</summary>
		private void Write8(byte index, byte value)
		{
			switch (index)
			{
				case 0: _registers.B = value; break;
				case 1: _registers.C = value; break;
				case 2: _registers.D = value; break;
				case 3: _registers.E = value; break;
				case 4: _registers.H = value; break;
				case 5: _registers.L = value; break;
				case 6: _bus.Write8(_registers.HL, value); break;
				case 7: _registers.A = value; break;
			}
		}


		/// <summary>Reads a 16-bit register.</summary>
		/// /// <returns>Register value.</returns>
		private ushort Read16(int index) => index switch
		{
			0 => _registers.BC,
			1 => _registers.DE,
			2 => _registers.HL,
			3 => _registers.SP,
			_ => 0
		};


		/// <summary>Writes to a 16-bit register.</summary>
		private void Write16(int index, ushort val)
		{
			switch (index)
			{
				case 0: _registers.BC = val; break;
				case 1: _registers.DE = val; break;
				case 2: _registers.HL = val; break;
				case 3: _registers.SP = val; break;
			}
		}


		/// <summary>Reads a 16-bit register group.</summary>
		/// <returns>Register value.</returns>
		private ushort ReadGroup16(int index) => index switch
		{
			0 => _registers.BC,
			1 => _registers.DE,
			2 => _registers.HL,
			3 => _registers.AF,
			_ => 0
		};


		/// <summary>Writes to a 16-bit register group.</summary>
		private void WriteGroup16(int index, ushort val)
		{
			switch (index)
			{
				case 0: _registers.BC = val; break;
				case 1: _registers.DE = val; break;
				case 2: _registers.HL = val; break;
				case 3: _registers.AF = (ushort)(val & 0b1111_1111_1111_0000); break;
			}
		}


		/// <summary>Reads the next byte from the program counter.</summary>
		/// <returns>Read byte.</returns>
		private byte ReadPc8()
		{
			byte val = _bus.Read8(_registers.PC);
			_registers.PC++;
			return val;
		}


		/// <summary>Reads the next 16-bit value from the program counter.</summary>
		/// <returns>Read value.</returns>
		private ushort ReadPc16()
		{
			ushort val = _bus.Read16(_registers.PC);
			_registers.PC += 2;
			return val;
		}


		/// <summary>Pushes a value onto the stack.</summary>
		/// <param name="value">Value to push.</param>
		private void Push(ushort value)
		{
			_registers.SP--;
			_bus.Write8(_registers.SP, (byte)(value >> 8));
			_registers.SP--;
			_bus.Write8(_registers.SP, (byte)value);
		}


		/// <summary>Pops a value from the stack.</summary>
		/// <returns>Popped value.</returns>
		private ushort Pop()
		{
			byte low = _bus.Read8(_registers.SP);
			_registers.SP++;
			byte high = _bus.Read8(_registers.SP);
			_registers.SP++;
			return (ushort)(low | (high << 8));
		}
	}
}
