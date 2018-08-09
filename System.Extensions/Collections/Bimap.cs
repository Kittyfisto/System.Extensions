using System.Collections.Generic;

namespace System.Collections
{
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="T1"></typeparam>
	/// <typeparam name="T2"></typeparam>
	public sealed class Bimap<T1, T2>
		: IBimap<T1, T2>
	{
		private readonly Dictionary<T1, T2> _forward;
		private readonly Dictionary<T2, T1> _backward;

		/// <summary>
		/// 
		/// </summary>
		public Bimap()
		{
			_forward = new Dictionary<T1, T2>();
			_backward = new Dictionary<T2, T1>();
		}

		#region Implementation of IReadOnlyMap<T1,T2>

		/// <inheritdoc />
		public IReadOnlyDictionary<T1, T2> Forward => _forward;

		/// <inheritdoc />
		public IReadOnlyDictionary<T2, T1> Backward => _backward;

		#endregion

		#region Implementation of IMap<T1,T2>

		/// <inheritdoc />
		public void Add(T1 t1, T2 t2)
		{
			_forward.Add(t1, t2);
			_backward.Add(t2, t1);
		}

		/// <inheritdoc />
		public void Clear()
		{
			_forward.Clear();
			_backward.Clear();
		}

		#endregion
	}
}