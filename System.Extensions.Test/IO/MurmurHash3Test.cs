using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using FluentAssertions;
using NUnit.Framework;

namespace System.Extensions.Test.IO
{
	[TestFixture]
	public sealed class MurmurHash3Test
	{
		/// <summary>
		///     Can be used to generate test data which is always different in each test case,
		///     however since the console output is recorded, a failing test can be easily
		///     reproduced by simply replacing the seed computation with the used seed of the
		///     failing test case.
		/// </summary>
		private Random _rng;

		[SetUp]
		public void Setup()
		{
			int rngSeed = (int)(DateTime.UtcNow.Ticks % int.MaxValue);
			Console.WriteLine("int rngSeed = {0};", rngSeed);
			_rng = new Random(rngSeed);
		}

		public static IEnumerable<int> DataLengths => new[]
		{
			1,
			2,
			3,
			4,
			5,
			6,
			7,
			8,
			9,
			10,
			11,
			12,
			13,
			14,
			15,
			16,
			17,
			32,
			33,
			50,
			100,
			128,
			256,
			257,
			512,
			1024,
			1025,
			1048576
		};

		public static IEnumerable<uint> MurmurSeeds => new uint[]
		{
			1,
			2,
			42,
			1337,
			9001
		};

		[Test]
		[Description("Verifies that the ctor actually uses a seed of 0 as the comment tells")]
		public void TestSeed1([ValueSource(nameof(DataLengths))] int dataLength)
		{
			var data = new byte[dataLength];
			_rng.NextBytes(data);

			var h = new MurmurHash3();
			var actual = new MurmurHash3(seed: 0);

			var hash = h.ComputeHash(data);
			var actualHash = actual.ComputeHash(data);

			hash.Should().Equal(actualHash);
		}

		[Test]
		[Description("Verifies that computing the hash twice yields the same result")]
		public void TestComputeTwice([ValueSource(nameof(DataLengths))] int dataLength,
									 [ValueSource(nameof(MurmurSeeds))] uint seed)
		{
			var data = new byte[dataLength];
			_rng.NextBytes(data);

			var h = new MurmurHash3(seed);
			var hash1 = h.ComputeHash(data);
			var hash2 = h.ComputeHash(data);

			hash2.Should().Equal(hash1);
		}

		[Test]
		public void RecordComputationSpeed()
		{
			var dataLength = 1024 * 512;
			var data = new byte[dataLength];
			_rng.NextBytes(data);

			Console.WriteLine("Hashing {0}K", dataLength/1024);
			Hash(() => new MurmurHash3().ComputeHash(data), "MurmurHash3");
			Hash(() => MD5.Create().ComputeHash(data), "MD5");
			Hash(() => SHA256.Create().ComputeHash(data), "SHA256");
		}

		private static void Hash(Func<byte[]> compute, string name)
		{
			// We compute the value once so most code is jitted...
			compute();

			const int tries = 100;
			var sw = new Stopwatch();
			byte[] hash = null;
			for (int i = 0; i < tries; ++i)
			{
				sw.Start();
				hash = compute();
				sw.Stop();
			}

			Console.WriteLine("{0}: 0x{1}, {2:F1}ms",
				name,
				hash.ToHexString(),
				sw.Elapsed.TotalMilliseconds/tries);
		}
	}
}