using GB.Communication;
using GB.Controls;

namespace GB.CPU
{
	public class CPU
	{
		public Registers Registers = new();
		public BUS Bus { get; private set; }

		private readonly InterruptController _interruptContoller;
		private readonly Instructions.Instructions _instructions;


		/// <summary>Delay before interrupt.</summary>
		public int ImeDelay;

		/// <summary>CPU halt and sto.</summary>
		public bool Halted, Stopped;

		public CPU(BUS bus)
		{
			Bus = bus;
			_instructions = new Instructions.Instructions(this);
			_interruptContoller = Bus.InterruptController;
		}

		/// <summary>Runs one CPU step.</summary>
		/// <returns>Number of used CPU cycles.</returns>
		public int Step()
		{
			ulong cycles = Bus.CycleCount;

			// Handle stop
			if (Stopped)
			{
				Bus.Tick(4);

				// Wake up by joypad
				Joypad jp = Bus.IO.Joypad;
				if (jp.AnyPressed())
				{
					Stopped = false;
					jp.ClearAny();
				}
				return 4;
			}

			// Pending interrupts
			byte pending = _interruptContoller.Pending();
			if (Halted && pending != 0)
				Halted = false;

			// Handle interrupts
			if (_interruptContoller.HandleInterrupts(Registers, Bus))
				return (int)(Bus.CycleCount - cycles);

			// Handle halt
			if (Halted)
			{
				Bus.Tick(4);
				return 4;
			}

			// Read instruction
			byte opcode = Bus.Read8(Registers.PC);
			Registers.PC++;

			// Execute instruction
			_instructions.Execute(opcode);

			// Update IME
			UpdateIme();

			return (int)(Bus.CycleCount - cycles);
		}

		/// <summary>Updates the interrupt state.</summary>
		private void UpdateIme() {
			if (ImeDelay > 0)
			{
				ImeDelay--;
				if (ImeDelay == 0)
					Registers.IME = true;
			}
		}
	}
}
