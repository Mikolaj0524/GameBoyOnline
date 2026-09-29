using GB.Communication;

namespace GB.CPU
{
	public class CPU
	{
		public Registers Registers = new();
		public BUS Bus { get; private set; }

		private readonly InterruptController _interruptContoller;
		private readonly Instructions.Instructions _instructions;

		public int ImeDelay;
		public bool Halted, Stopped;

		public CPU(BUS bus)
		{
			Bus = bus;
			_instructions = new Instructions.Instructions(this);
			_interruptContoller = Bus.InterruptController;
		}

		public int Step()
		{
			ulong cycles = Bus.CycleCount;

			byte opcode = Bus.Read8(Registers.PC);
			Registers.PC++;

			_instructions.Execute(opcode);

			return (int)cycles;
		}
	}
}
