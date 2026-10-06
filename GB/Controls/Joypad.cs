using GB.CPU;
using GB.Interfaces;
using GB.Utils;

namespace GB.Controls
{
	public class Joypad(InterruptController interruptController) : IRWInterface
	{
		/// <summary>Button selection and button states.</summary>
		private byte _select = 0b0011_0000, _action = 0b0000_1111, _cross = 0b0000_1111;

		private readonly InterruptController _interruptController = interruptController;

		/// <summary>Reads the joypad state.</summary>
		/// <returns>Joypad state.</returns>
		public byte Read8(ushort address)
		{
			byte state = (byte)(_select | 0b1100_0000);
			byte buttons = 0b0000_1111;

			// Read action buttons.
			if (!_select.IsBitSet(5))
				buttons &= _action;

			// Read direction buttons.
			if (!_select.IsBitSet(4))
				buttons &= _cross;

			return (byte)(state | buttons);
		}


		/// <summary>Writes to the joypad register.</summary>
		public void Write8(ushort address, byte value)
		{
			_select = (byte)(value & 0b0011_0000);
		}


		/// <summary>Updates a button state.</summary>
		public void Press(Button button, bool down)
		{
			switch (button)
			{
				case Button.Right: UpdateCross(0b0000_0001, down); break;
				case Button.Left: UpdateCross(0b0000_0010, down); break;
				case Button.Up: UpdateCross(0b0000_0100, down); break;
				case Button.Down: UpdateCross(0b0000_1000, down); break;
				case Button.A: UpdateAction(0b0000_0001, down); break;
				case Button.B: UpdateAction(0b0000_0010, down); break;
				case Button.Select: UpdateAction(0b0000_0100, down); break;
				case Button.Start: UpdateAction(0b0000_1000, down); break;
			}
		}


		/// <summary>Updates a direction button.</summary>
		private void UpdateCross(byte mask, bool down) => _cross = UpdateButtons(_cross, mask, down);


		/// <summary>Updates an action button.</summary>
		private void UpdateAction(byte mask, bool down) => _action = UpdateButtons(_action, mask, down);


		/// <summary>Updates a button state.</summary>
		/// <returns>Updated button state.</returns>
		private byte UpdateButtons(byte current, byte mask, bool down)
		{
			bool prev = (current & mask) != 0;
			if (down)
			{
				// Mark the button as pressed.
				current &= (byte)~mask;

				// Request an interrupt when the button is pressed.
				if (prev)
					_interruptController.SetInterrupt(Interrupt.Joypad);
			}
			else
			{
				// Mark the button as released.
				current |= mask;
			}

			return current;
		}


		/// <summary>Checks if any button is pressed.</summary>
		/// <returns>True if a button is pressed.</returns>
		public bool AnyPressed() => _action != 0b0000_1111 || _cross != 0b0000_1111;

		/// <summary>Clears all pressed buttons.</summary>
		public void ClearAny()
		{
			_action = 0b0000_1111;
			_cross = 0b0000_1111;
		}
	}
}
