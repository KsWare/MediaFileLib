using System;
using System.Collections.Generic;
using System.IO;

namespace KsWare.MediaFileLib.Shared {

	internal class FileNameComparer : IEqualityComparer<string> {

		private readonly string _directory;

		public FileNameComparer(string directory) {
			_directory = directory;
		}

		public bool Equals(string x, string y) {
			if (x == null && y == null) return true;
			if (x == null || y == null) return false;
			if (!x.Contains("\\")) x = Path.Combine(_directory, x);
			if (!y.Contains("\\")) y = Path.Combine(_directory, y);
			return x.Equals(y, StringComparison.OrdinalIgnoreCase);
		}

		public int GetHashCode(string obj) {
			if (!obj.Contains("\\")) obj = Path.Combine(_directory, obj);
			return obj.ToLowerInvariant().GetHashCode();
		}
	}

}