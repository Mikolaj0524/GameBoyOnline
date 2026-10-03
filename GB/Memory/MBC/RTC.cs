using GB.Utils;

namespace GB.Memory.MBC
{
	public class RTC
	{
		/// <summary>Total elapsed RTC seconds.</summary>
		private long _total = 0;


		/// <summary>Last RTC update time.</summary>
		private DateTime _stamp = DateTime.UtcNow;


		/// <summary>RTC halt and carry flags.</summary>
		private bool _halt = false, _carry = false;


		/// <summary>Latched RTC register values.</summary>
		private byte _sec, _min, _hour, _dayLow, _dayHigh;


		/// <summary>Updates RTC.</summary>
		private void Update()
		{
			DateTime now = DateTime.UtcNow;

			if (!_halt)
			{
				long elapsed = (long)(now - _stamp).TotalSeconds;
				if (elapsed > 0)
				{
					// Add elapsed to RTC
					_total += elapsed;
					_stamp = _stamp.AddSeconds(elapsed);
				}
			}
			else
			{
				// Keep timestamp
				_stamp = now;
			}

			// Check time limit
			const long maxSeconds = 512L * 86400L;
			if (_total >= maxSeconds)
			{
				_carry = true;
				_total %= maxSeconds;
			}
		}


		/// <summary>Saves current RTC values.</summary>
		public void Latch()
		{
			Update();

			long days = _total / 86400;
			_sec = (byte)(_total % 60);
			_min = (byte)((_total / 60) % 60);
			_hour = (byte)((_total / 3600) % 24);
			_dayLow = (byte)(days & 0xFF);
			_dayHigh = (byte)(days.GetBit(8) | (_halt ? 0x40 : 0) | (_carry ? 0x80 : 0));
		}


		/// <summary>Reads latched RTC register (0x08-0x0C).</summary>
		/// <returns>Register value.</returns>
		public byte Read(byte reg) => reg switch
		{
			0x08 => _sec,
			0x09 => _min,
			0x0A => _hour,
			0x0B => _dayLow,
			0x0C => _dayHigh,
			_ => 0xFF
		};


		/// <summary>Writes RTC register (0x08-0x0C).</summary>
		public void Write(byte reg, byte value)
		{
			Update();

			long s = _total % 60;
			long m = (_total / 60) % 60;
			long h = (_total / 3600) % 24;
			long d = _total / 86400;

			switch (reg)
			{
				case 0x08: s = value & 0x3F; break;
				case 0x09: m = value & 0x3F; break;
				case 0x0A: h = value & 0x1F; break;
				case 0x0B: d = (d & 0x100) | value; break;
				case 0x0C:
					d = (d & 0xFF) | ((long)value.GetBit(0) << 8);
					_halt = value.IsBitSet(6);
					_carry = value.IsBitSet(7);
					break;
				default: return;
			}

			// Save time
			_total = (d * 86400) + (h * 3600) + (m * 60) + s;

			// Reset timestamp
			_stamp = DateTime.UtcNow;
		}
	}
}
