using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;
using JetBrains.Annotations;

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

		public static void RenameDependencyExtension([NotNull] this MediaFileInfo mediaFile, [NotNull] string extension,
			[CanBeNull] string extensionFolder, bool moveToExtensionFolder, out bool extensionFound)
		{
			Contract.Requires(mediaFile != null);
			Contract.Requires(!string.IsNullOrEmpty(extension));

			var fileName = Path.Combine(mediaFile.OldFile.DirectoryName, $"{mediaFile.OldFile.NameWithoutExtension()}{extension}");
			if (File.Exists(fileName))
			{
				extensionFound = true;
				if (!string.IsNullOrEmpty(extensionFolder) && moveToExtensionFolder)
				{
					var newName = Path.Combine(mediaFile.OldFile.DirectoryName, extensionFolder, $"{mediaFile.OriginalFile.NameWithoutExtension()}{extension}");
					FileUtils.TryMoveWithLog(fileName, newName);
					return;
				}
				else
				{
					var newName = $"{mediaFile.OriginalFile.NameWithoutExtension()}{extension}";
					FileUtils.TryRenameWithLog(fileName, newName);
					return;
				}
			}

			if (string.IsNullOrEmpty(extensionFolder))
			{
				extensionFound = false;
				return;
			}
			fileName = Path.Combine(mediaFile.OldFile.DirectoryName, extensionFolder, $"{mediaFile.OldFile.NameWithoutExtension()}{extension}");
			if (File.Exists(fileName))
			{
				extensionFound = true;
				var newName = $"{mediaFile.OriginalFile.NameWithoutExtension()}{extension}";
				FileUtils.TryRenameWithLog(fileName, newName);
			}
			else
			{
				extensionFound = false;
				return;
			}
		}

		public static void RenameDependencyRawExtension(this MediaFileInfo mediaFile, IEnumerable<string> rawExtensions, string rawFolder = "RAW", bool moveToRawFolder = false)
		{
			foreach (var ext in rawExtensions)
			{
				RenameDependencyExtension(mediaFile, ext, rawFolder, moveToRawFolder, out var found);
				if(found) return;
			}
		}
	}
}
