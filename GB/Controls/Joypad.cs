using GB.Interfaces;
using GB.Utils;

namespace GB.Controls
{
	public class Joypad : IRWInterface
	{
		private byte _select = 0b0011_0000, _action = 0b0000_1111, _cross = 0b0000_1111;
		public Action? RequestInterrupt;

		public byte Read8(ushort address)
		{
			byte state = (byte)(_select | 0b1100_0000);
			byte buttons = 0b0000_1111;

			if (!_select.IsBitSet(5))
				buttons &= _action;

			if (!_select.IsBitSet(4))
				buttons &= _cross;

			return (byte)(state | buttons);
		}

		public void Write8(ushort address, byte value)
		{
			_select = (byte)(value & 0b0011_0000);
		}

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

		private void UpdateCross(byte mask, bool down) => _cross = UpdateButtons(_cross, mask, down);
		private void UpdateAction(byte mask, bool down) => _action = UpdateButtons(_action, mask, down);
		private byte UpdateButtons(byte current, byte mask, bool down)
		{
			bool prev = (current & mask) != 0;
			if (down)
			{
				current &= (byte)~mask;
				if (prev)
					RequestInterrupt?.Invoke();
			}
			else
			{
				current |= mask;
			}

			return current;
		}

		public bool AnyPressed() => _action != 0b0000_1111 || _cross != 0b0000_1111;
	
		public void ClearAny()
		{
			_action = 0b0000_1111;
			_cross = 0b0000_1111;
		}
	}
}
