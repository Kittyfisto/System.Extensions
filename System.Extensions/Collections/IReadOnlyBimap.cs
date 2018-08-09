using System.Collections.Generic;

namespace System.Collections
{
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="T1"></typeparam>
	/// <typeparam name="T2"></typeparam>
	public interface IReadOnlyBimap<T1, T2>
	{
		/// <summary>
		/// 
		/// </summary>
		IReadOnlyDictionary<T1, T2> Forward { get; }

		/// <summary>
		/// 
		/// </summary>
		IReadOnlyDictionary<T2, T1> Backward { get; }
	}
}