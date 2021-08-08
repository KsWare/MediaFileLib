using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace KsWare.MediaFileLib.Shared {

	public static class MediaFileInfoExtension {

		public static bool RenameDependency(this MediaFileInfo mediaFile, string folder, string suffix) {
			var fileName = Path.Combine(mediaFile.OldFile.DirectoryName, folder,
				$"{mediaFile.OldFile.NameWithoutExtension()}{suffix}{mediaFile.OldFile.Extension}");
			if (!File.Exists(fileName)) return false;
			var newFileName = Path.Combine(mediaFile.OldFile.DirectoryName, folder,
				$"{mediaFile.OriginalFile.NameWithoutExtension()}{suffix}{mediaFile.OriginalFile.Extension}");
			if (File.Exists(newFileName)) {
				return false;
			}

			File.Move(fileName, newFileName);
			return true;
		}

		public static void RenameDependencyExtension([NotNull] this MediaFileInfo mediaFile, [NotNull] string extension,
			[CanBeNull] string extensionFolder, bool moveToExtensionFolder, string pluginName, out bool extensionFound) {
			Contract.Requires(mediaFile != null);
			Contract.Requires(!string.IsNullOrEmpty(extension));

			{
				var fileName = Path.Combine(mediaFile.OldFile.DirectoryName,
					$"{mediaFile.OldFile.NameWithoutExtension()}{extension}");
				if (File.Exists(fileName)) {
					extensionFound = true;
					if (!string.IsNullOrEmpty(extensionFolder) && moveToExtensionFolder) {
						var newName = Path.Combine(mediaFile.OldFile.DirectoryName, extensionFolder,
							$"{mediaFile.OriginalFile.NameWithoutExtension()}{extension}");
						FileUtils.TryMoveWithLog(fileName, newName);
						return;
					}
					else {
						var newName = $"{mediaFile.OriginalFile.NameWithoutExtension()}{extension}";
						FileUtils.TryRenameWithLog(fileName, newName, pluginName);
						return;
					}
				}
			}

			if (!string.IsNullOrEmpty(extensionFolder)){
				var fileName = Path.Combine(mediaFile.OldFile.DirectoryName, extensionFolder,
					$"{mediaFile.OldFile.NameWithoutExtension()}{extension}");
				if (File.Exists(fileName)) {
					extensionFound = true;
					var newName = $"{mediaFile.OriginalFile.NameWithoutExtension()}{extension}";
					FileUtils.TryRenameWithLog(fileName, newName, pluginName);
					return;
				}
			}

			{
				// Xiaomi Redmi Camera
				// IMG_20210706_144341_1.jpg + Raw\IMG_20210706_144341.dwg
				// IMG_20210706_144341_3.jpg + Raw\IMG_20210706_144341_2.dwg
				var fn = mediaFile.OldFile.NameWithoutExtension();
				var m = Regex.Match(fn, @"^(?<base>.*?\d{8}_\d{6})_(?<suffix>\d)$");
				if (m.Success) {
					var suffix = m.Groups["suffix"].Success ? $"_{int.Parse(m.Groups["suffix"].Value) - 1}" : null;
					if (suffix == "_0") suffix = "";
					if (suffix != null) {
						var fileName = Path.Combine(mediaFile.OldFile.DirectoryName, "Raw",
							$"{m.Groups["base"]}{suffix}{extension}");
						if (File.Exists(fileName)) {
							extensionFound = true;
							var newName = Path.Combine(mediaFile.OriginalFile.DirectoryName, "Raw",
								$"{mediaFile.OriginalFile.NameWithoutExtension()}{extension}");
							FileUtils.TryRenameWithLog(fileName, newName, pluginName);
							return;
						}
					}
				}
			}

			{
				extensionFound = false;
				return;
			}
		}

		public static void RenameDependencyRawExtension(this MediaFileInfo mediaFile, IEnumerable<string> rawExtensions,
			string rawFolder, bool moveToRawFolder, string pluginName) {
			foreach (var ext in rawExtensions) {
				RenameDependencyExtension(mediaFile, ext, rawFolder, moveToRawFolder, pluginName, out var found);
				if (found) return;
			}
		}
	}

}
