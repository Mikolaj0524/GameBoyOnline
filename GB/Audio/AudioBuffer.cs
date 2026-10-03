namespace GB.Audio
{
	public sealed class AudioBuffer
	{
		/// <summary>Audio sample data.</summary>
		private readonly float[] _data;


		/// <summary>Buffer lock.</summary>
		private readonly object _lock = new();


		/// <summary>Buffer positions and sample count.</summary>
		private int _readPos, _writePos, _count, _capacity;


		/// <summary>Last output samples.</summary>
		private float _lastLeft, _lastRight;

		public AudioBuffer(int capacityFrames)
		{
			_capacity = capacityFrames;
			_data = new float[capacityFrames * 2];
		}


		/// <summary>Gets the number of available frames.</summary>
		public int AvailableFrames
		{
			get
			{
				lock (_lock)
					return _count;
			}
		}


		/// <summary>Writes a stereo sample.</summary>
		public void Write(float left, float right)
		{
			lock (_lock)
			{
				// Remove frame when buffer is full.
				if (_count == _capacity)
				{
					_readPos = (_readPos + 1) % _capacity;
					_count--;
				}

				// Write position.
				int i = _writePos * 2;
				_data[i] = left;
				_data[i + 1] = right;

				// Next frame
				_writePos = (_writePos + 1) % _capacity;
				_count++;
			}
		}


		/// <summary>Reads audio samples from the buffer.</summary>
		/// <returns>Number of float values requested.</returns>
		public int Read(float[] buffer, int offset, int count)
		{
			lock (_lock)
			{
				// Number of stereo frames
				int frames = count / 2;
				int n = Math.Min(frames, _count);

				for (int f = 0; f < n; f++)
				{
					// Read position
					int i = _readPos * 2;

					// Save last samples
					_lastLeft = _data[i];
					_lastRight = _data[i + 1];

					// Output position
					int bufferIndex = offset + f * 2;
					buffer[bufferIndex] = _lastLeft;
					buffer[bufferIndex + 1] = _lastRight;

					// Next frame
					_readPos = (_readPos + 1) % _capacity;
				}

				_count -= n;

				// Fill missing samples.
				if (n < frames)
				{
					for (int f = n; f < frames; f++)
					{
						_lastLeft *= 0.98f;
						_lastRight *= 0.98f;

						int bufferIndex = offset + f * 2;
						buffer[bufferIndex] = _lastLeft;
						buffer[bufferIndex + 1] = _lastRight;
					}
				}

				// Clear unused values.
				for (int i = offset + frames * 2; i < offset + count; i++)
					buffer[i] = 0f;

				return count;
			}
		}
	}
}