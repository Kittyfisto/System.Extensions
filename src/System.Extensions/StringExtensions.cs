using System.Diagnostics.Contracts;

namespace System
{
	/// <summary>
	///     Extensions to the <see cref="string" /> class.
	/// </summary>
	public static class StringExtensions
	{
		/// <summary>
		///     Tests if this string ends with any of the given characters.
		/// </summary>
		/// <param name="that"></param>
		/// <param name="values"></param>
		/// <returns></returns>
		[Pure]
		public static bool EndsWithAny(this string that, params char[] values)
		{
			int unused;
			return EndsWithAny(that, values, out unused);
		}

		/// <summary>
		///     Tests if this string ends with any of the given characters.
		/// </summary>
		/// <param name="that"></param>
		/// <param name="values"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">When <paramref name="that"/> or <paramref name="values"/> is null</exception>
		[Pure]
		public static bool EndsWithAny(this string that, char[] values, out int index)
		{
			if (that == null)
				throw new ArgumentNullException(nameof(that));
			if (values == null)
				throw new ArgumentNullException(nameof(values));

			if (that.Length == 0)
			{
				index = -1;
				return false;
			}

			var c = that[that.Length - 1];
			// ReSharper disable once ForCanBeConvertedToForeach
			for (index = 0; index < values.Length; ++index)
				if (c == values[index])
					return true;
			return false;
		}
	}
}