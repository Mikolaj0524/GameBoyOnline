namespace GB.Audio
{
	public sealed class AudioBuffer
	{
		private readonly float[] _data;
		private readonly object _lock = new();

		private int _readPos, _writePos, _count, _capacity;
		private float _lastLeft, _lastRight;

		public AudioBuffer(int capacityFrames)
		{
			_capacity = capacityFrames;
			_data = new float[capacityFrames * 2];
		}

		public int AvailableFrames
		{
			get
			{
				lock (_lock)
					return _count;
			}
		}

		public void Write(float left, float right)
		{
			lock (_lock)
			{
				if (_count == _capacity)
				{
					_readPos = (_readPos + 1) % _capacity;
					_count--;
				}

				int i = _writePos * 2;
				_data[i] = left;
				_data[i + 1] = right;

				_writePos = (_writePos + 1) % _capacity;
				_count++;
			}
		}

		public int Read(float[] buffer, int offset, int count)
		{
			lock (_lock)
			{
				int frames = count / 2;
				int n = Math.Min(frames, _count);

				for (int f = 0; f < n; f++)
				{
					int i = _readPos * 2;

					_lastLeft = _data[i];
					_lastRight = _data[i + 1];

					int bufferIndex = offset + f * 2;
					buffer[bufferIndex] = _lastLeft;
					buffer[bufferIndex + 1] = _lastRight;

					_readPos = (_readPos + 1) % _capacity;
				}

				_count -= n;

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

				for (int i = offset + frames * 2; i < offset + count; i++)
					buffer[i] = 0f;

				return count;
			}
		}

		public int Read(byte[] byteBuffer)
		{
			int count = byteBuffer.Length / 4;
			float[] temp = new float[count];

			int samples = Read(temp, 0, temp.Length);
			Buffer.BlockCopy(temp, 0, byteBuffer, 0, samples * 4);
			return samples;
		}
	}
}