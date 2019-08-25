using System.IO;

namespace KsWare.MediaFileLib.Shared
{
	public class FileName
	{
		public string FullName { get; set; }

		public string Name { get; set; }

		public string Extension { get; set; }

		public string DirectoryName { get; set; }

		protected static void Parse(string fileName, FileName f)
		{
			f.FullName = Path.GetFullPath(fileName);
			f.Name = Path.GetFileNameWithoutExtension(fileName);
			f.Extension = Path.GetExtension(fileName);
			f.DirectoryName = Path.GetDirectoryName(fileName);
		}

		public static bool TryParse(string fileName, out FileName f)
		{//TODO
			f=new FileName();
			Parse(fileName, f);
			return true;
		}
	}
}