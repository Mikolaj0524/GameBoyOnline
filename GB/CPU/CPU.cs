using GB.Communication;

namespace GB.CPU
{
	public class CPU
	{
		public Registers Registers = new();
		public BUS Bus { get; private set; }

		private readonly InterruptController _interruptContoller;
		private readonly Instructions _instructions;

		public bool Halted, Stopped;

		public CPU(BUS bus)
		{
			Bus = bus;
			_instructions = new Instructions(this);
			_interruptContoller = Bus.InterruptController;
		}


		public int Step()
		{
			ulong cycles = Bus.CycleCount;


			return (int)cycles;
		}
	}
}
