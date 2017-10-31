using System.Security.Cryptography;

namespace System.IO
{
	/// <summary>
	///     This class provides an implementation of MurmurHash (<a href="https://en.wikipedia.org/wiki/MurmurHash" />) which
	///     is a very fast hashing algorithm **WHICH IS NOT INTENDED TO BE USED SCENARIOS WHERE CRYPTOGRAPHIC HASHES ARE
	///     REQUIRED**.
	/// </summary>
	/// <remarks>
	///     This implementation is a modification of the source found at
	///     <a href="http://blog.teamleadnet.com/2012/08/murmurhash3-ultra-fast-hash-algorithm.html" />.
	/// </remarks>
	/// <remarks>
	///     In all honesty, this implementation performs slower than <see cref="MD5" /> in my tests, so if
	///     performance is important to you, you might be better of using that.
	/// </remarks>
	public sealed class MurmurHash3
	{
		private readonly uint _seed;

		/// <summary>
		///     Initializes this object with a seed of 0.
		/// </summary>
		public MurmurHash3()
			: this(seed: 0)
		{
		}

		/// <summary>
		///     Initializes this object with the given seed.
		/// </summary>
		/// <remarks>
		///     Using the same data, but a different seed causes different hash values to be produced!
		/// </remarks>
		public MurmurHash3(uint seed)
		{
			_seed = seed;
		}

		/// <summary>
		///     Computes the hash value for the specified byte array.
		/// </summary>
		/// <remarks>
		///     This method is thread-safe and may be called by as many threads as desired.
		/// </remarks>
		/// <param name="data"></param>
		/// <returns></returns>
		public byte[] ComputeHash(byte[] data)
		{
			var cruncher = new NumberCruncher(_seed);
			return cruncher.ComputeHash(data);
		}

		private struct NumberCruncher
		{
			private static readonly ulong READ_SIZE = 16;

			private static readonly ulong C1 = 0x87c37b91114253d5L;
			private static readonly ulong C2 = 0x4cf5ad432745937fL;

			private ulong _h1;
			private ulong _h2;

			private ulong _length;
			private readonly uint _seed;

			public NumberCruncher(uint seed)
			{
				_seed = seed;
				_h1 = 0;
				_h2 = 0;
				_length = 0;
			}

			private void MixBody(ulong k1, ulong k2)
			{
				_h1 ^= MixKey1(k1);

				_h1 = _h1.RotateLeft(bits: 27);
				_h1 += _h2;
				_h1 = _h1 * 5 + 0x52dce729;

				_h2 ^= MixKey2(k2);

				_h2 = _h2.RotateLeft(bits: 31);
				_h2 += _h1;
				_h2 = _h2 * 5 + 0x38495ab5;
			}

			private static ulong MixKey1(ulong k1)
			{
				k1 *= C1;
				k1 = k1.RotateLeft(bits: 31);
				k1 *= C2;
				return k1;
			}

			private static ulong MixKey2(ulong k2)
			{
				k2 *= C2;
				k2 = k2.RotateLeft(bits: 33);
				k2 *= C1;
				return k2;
			}

			private static ulong MixFinal(ulong k)
			{
				// avalanche bits

				k ^= k >> 33;
				k *= 0xff51afd7ed558ccdL;
				k ^= k >> 33;
				k *= 0xc4ceb9fe1a85ec53L;
				k ^= k >> 33;
				return k;
			}

			public byte[] ComputeHash(byte[] data)
			{
				ProcessBytes(data);

				_h1 ^= _length;
				_h2 ^= _length;

				_h1 += _h2;
				_h2 += _h1;

				_h1 = MixFinal(_h1);
				_h2 = MixFinal(_h2);

				_h1 += _h2;
				_h2 += _h1;

				var hash = new byte[READ_SIZE];

				Array.Copy(BitConverter.GetBytes(_h1), sourceIndex: 0, destinationArray: hash, destinationIndex: 0, length: 8);
				Array.Copy(BitConverter.GetBytes(_h2), sourceIndex: 0, destinationArray: hash, destinationIndex: 8, length: 8);

				return hash;
			}

			private void ProcessBytes(byte[] data)
			{
				_h1 = _seed;
				_length = 0L;

				var pos = 0;
				var remaining = (ulong) data.Length;

				// read 128 bits, 16 bytes, 2 longs in eacy cycle
				while (remaining >= READ_SIZE)
				{
					var k1 = data.GetUInt64(pos);
					pos += 8;

					var k2 = data.GetUInt64(pos);
					pos += 8;

					_length += READ_SIZE;
					remaining -= READ_SIZE;

					MixBody(k1, k2);
				}

				// if the input MOD 16 != 0
				if (remaining > 0)
					ProcessBytesRemaining(data, remaining, pos);
			}

			private void ProcessBytesRemaining(byte[] bb, ulong remaining, int pos)
			{
				ulong k1 = 0;
				ulong k2 = 0;
				_length += remaining;

				// little endian (x86) processing
				switch (remaining)
				{
					case 15:
						k2 ^= (ulong) bb[pos + 14] << 48;
						goto case 14; //< fall through
					case 14:
						k2 ^= (ulong) bb[pos + 13] << 40;
						goto case 13; //< fall through
					case 13:
						k2 ^= (ulong) bb[pos + 12] << 32;
						goto case 12; //< fall through
					case 12:
						k2 ^= (ulong) bb[pos + 11] << 24;
						goto case 11; //< fall through
					case 11:
						k2 ^= (ulong) bb[pos + 10] << 16;
						goto case 10; //< fall through
					case 10:
						k2 ^= (ulong) bb[pos + 9] << 8;
						goto case 9; //< fall through
					case 9:
						k2 ^= bb[pos + 8];
						goto case 8; //< fall through
					case 8:
						k1 ^= bb.GetUInt64(pos);
						break;
					case 7:
						k1 ^= (ulong) bb[pos + 6] << 48;
						goto case 6; //< fall through
					case 6:
						k1 ^= (ulong) bb[pos + 5] << 40;
						goto case 5; //< fall through
					case 5:
						k1 ^= (ulong) bb[pos + 4] << 32;
						goto case 4; //< fall through
					case 4:
						k1 ^= (ulong) bb[pos + 3] << 24;
						goto case 3; //< fall through
					case 3:
						k1 ^= (ulong) bb[pos + 2] << 16;
						goto case 2; //< fall through
					case 2:
						k1 ^= (ulong) bb[pos + 1] << 8;
						goto case 1; //< fall through
					case 1:
						k1 ^= bb[pos];
						break;

					default:
						throw new Exception("Something went wrong with remaining bytes calculation.");
				}

				_h1 ^= MixKey1(k1);
				_h2 ^= MixKey2(k2);
			}
		}
	}
}