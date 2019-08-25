using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace KsWare.MediaFileLib.Shared
{
	public static class FileInfoExtension
	{
		public static string NameWithoutExtension(this FileInfo fileInfo) =>
			Path.GetFileNameWithoutExtension(fileInfo.Name);
	}
}
