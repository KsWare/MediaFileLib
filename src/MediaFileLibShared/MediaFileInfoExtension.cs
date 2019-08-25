using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace KsWare.MediaFileLib.Shared
{
	public static class MediaFileInfoExtension
	{
		public static bool RenameDependency(this MediaFileInfo mediaFile, string folder, string suffix)
		{
			var fileName = Path.Combine(mediaFile.OldFile.DirectoryName, folder,
				$"{mediaFile.OldFile.NameWithoutExtension()}{suffix}{mediaFile.OldFile.Extension}");
			if (!File.Exists(fileName)) return false;
			var newFileName = Path.Combine(mediaFile.OldFile.DirectoryName, folder, $"{mediaFile.OriginalFile.NameWithoutExtension()}{suffix}{mediaFile.OriginalFile.Extension}");
			if (File.Exists(newFileName))
			{
				return false;
			}
			File.Move(fileName, newFileName);
			return true;
		}

		public static bool RenameDependencyExtension(this MediaFileInfo mediaFile, string extension)
		{
			var fileName= Path.Combine(mediaFile.OldFile.DirectoryName, $"{mediaFile.OldFile.NameWithoutExtension()}{extension}");
			if (!File.Exists(fileName)) return false;
			var newName = $"{mediaFile.OriginalFile.NameWithoutExtension()}{extension}";
			return FileUtils.RenameWithLog(fileName, newName);
		}

		public static bool RenameDependencyRawExtension(this MediaFileInfo mediaFile, IEnumerable<string> rawExtensions)
		{
			foreach (var ext in rawExtensions)
			{
				if(RenameDependencyExtension(mediaFile, ext)) return true;
			}
			return false;
		}
	}
}
