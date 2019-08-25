using System;
using System.Collections.Generic;

namespace KsWare.MediaFileLib.Shared
{
	public class GenericComparer<T> : IComparer<T>
	{
		public GenericComparer(Func<T, T, int> function)
		{
			Function = function;
		}

		public Func<T,T,int> Function { get; set; }
		public int Compare(T x, T y) => Function(x,y);
	}
}