using System.Diagnostics.Contracts;
using System.Text;

namespace System
{
	/// <summary>
	///     Extensions to byte arrays.
	/// </summary>
	public static class ByteArrayExtensions
	{
		/// <summary>
		///     Prints a hexadecimal string in the form of "0123456789abcdef".
		/// </summary>
		/// <param name="that"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">In case <paramref name="that" /> is null.</exception>
		[Pure]
		public static string ToHexString(this byte[] that)
		{
			if (that == null)
				throw new ArgumentNullException(nameof(that));

			var hex = new StringBuilder(that.Length * 2);
			foreach (var b in that)
				hex.AppendFormat("{0:x2}", b);
			return hex.ToString();
		}
	}
}