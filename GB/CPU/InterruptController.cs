using GB.Communication;

namespace GB.CPU
{
	public class InterruptController
	{
		public byte IE, IF;

		private static readonly Dictionary<Interrupt, (byte mask, ushort vec)> _interrupts = new()
		{
			[Interrupt.VBlank] = (0b0000_0001, 0x0040),
			[Interrupt.LCDStat] = (0b0000_0010, 0x0048),
			[Interrupt.Timer] = (0b0000_0100, 0x0050),
			[Interrupt.Serial] = (0b0000_1000, 0x0058),
			[Interrupt.Joypad] = (0b0001_0000, 0x0060)
		};

		public void SetInterrupt(Interrupt i) => IF |= _interrupts[i].mask;

		public byte Pending() => (byte)(IF & IE);

		public bool HandleInterrupts(Registers registers, BUS bus)
		{
			byte pending = Pending();
			if (pending == 0 || !registers.IME)
				return false;

			registers.IME = false;
			bus.Tick(8);

			ushort interruptVec = 0;
			foreach (var interrupt in _interrupts)
			{
				if ((pending & interrupt.Value.mask) != 0)
				{
					IF &= (byte)~interrupt.Value.mask;
					interruptVec = interrupt.Value.vec;
					break;
				}
			}

			registers.SP--;
			bus.Write8(registers.SP, (byte)(registers.PC >> 8));
			registers.SP--;
			bus.Write8(registers.SP, (byte)registers.PC);

			registers.PC = interruptVec;
			bus.Tick(4);

			return true;
		}
	}
}
