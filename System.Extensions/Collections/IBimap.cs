namespace System.Collections
{
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="T1"></typeparam>
	/// <typeparam name="T2"></typeparam>
	public interface IBimap<T1, T2>
		: IReadOnlyBimap<T1, T2>
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="t1"></param>
		/// <param name="t2"></param>
		void Add(T1 t1, T2 t2);

		/// <summary>
		/// 
		/// </summary>
		void Clear();
	}
}