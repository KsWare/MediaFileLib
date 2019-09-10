using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using static KsWare.MediaFileLib.Shared.RegExUtils;


namespace KsWare.MediaFileLib.Shared
{
	public static class FileUtils
	{
		// 20190426_132443_HDR			DCIM\Camera
		// DSC_0001_1					DCIM\100ANDRO
		// FB_IMG_1552644499266			DCIM\Facebook
		// IMG_20190503_183708_2.png	DCIM\OpenCamera
		//

		private static readonly string[] ExifFileExtensions = {".jpg", ".jpeg", ".tif", ".tiff"};
		private static readonly string[] MovieFileExtensions = { ".mov", ".mp4" };
		private static readonly string[] ImageFileExtension =
		{
			".jpg", ".jpeg", // exif
			".tif", ".tiff", //exif
			".png",
			".bmp",
//			".emf",
//			".gif",
//			".ico",
//			".wmf",
		};

		public static DirectoryScanOptions DefaultDirectoryScanOptions = new DirectoryScanOptions();

		public static bool IsExifExtension(string fileName)
		{
			var ext = fileName.StartsWith(".") ? fileName : Path.GetExtension(fileName);

			return ExifFileExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase);
		}

		public static bool IsMovie(string fileName)
		{
			var ext = fileName.StartsWith(".") ? fileName : Path.GetExtension(fileName);
			return MovieFileExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase);
		}
		public static bool IsImage(string fileName)
		{
			var ext = fileName.StartsWith(".") ? fileName : Path.GetExtension(fileName);
			return ImageFileExtension.Contains(ext,StringComparer.OrdinalIgnoreCase);
		}

		public static void ScanDirectoryRecursive(object directory, ref List<string> files,
			DirectoryScanOptions options = null)
		{
			if (options == null) options = DefaultDirectoryScanOptions;
			var d = directory is DirectoryInfo info ? info : new DirectoryInfo((string)directory);


			files.AddRange(Directory.GetFiles(d.FullName));
			foreach (var sub in d.EnumerateDirectories())
			{
				// ignore junctions
				if (options.IncludeReparsePoint || (sub.Attributes & FileAttributes.ReparsePoint) != 0) continue;

				// ignore hidden or system directories
				if (options.IncludeSystem || (sub.Attributes & FileAttributes.System) != 0) continue;
				if (options.IncludeHidden || (sub.Attributes & FileAttributes.Hidden) != 0) continue;

				// ignore directories starting with "."
				if (sub.Name.StartsWith(".")) continue;

				ScanDirectoryRecursive(sub, ref files);
			}
		}

		public static void OpenInExplorer(string path)
		{
			string cmd = "explorer.exe";
			string arg = "/select, " + path;
			Process.Start(cmd, arg);
		}

		public static DateTime? GetDateTakenOrAlternative([NotNull] string fileName)
		{
			if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentNullException(nameof(fileName));
			if (IsImage(fileName))
			{
				{
					var dateTaken = GetDateTaken(fileName);
					if (dateTaken.HasValue) return dateTaken.Value;
				}
				{
					var dateTaken = GetDateTakenFromBase(fileName);
					if (dateTaken.HasValue) return dateTaken.Value;
				}
				{
					var timestamp = GetTimestampFromFileName(fileName);
					if (timestamp.HasValue) return timestamp.Value;
				}

				// return File.GetLastWriteTime(fileName);
				return null;
			}
			else if (IsMovie(fileName))
			{
				var d = GetDate(fileName, p => p.System.ItemDate); // also DateAcquired
				if (d.HasValue) return d.Value;


				// return File.GetLastWriteTime(fileName);
				return null;
			}


			return null;
		}

		public static bool TryFindFile(string directory, string baseName, string suffix, string ext, out string fileName)
		{
			// "base file" is the file w/ the baseName and w/o a suffix.
			if (string.IsNullOrEmpty(directory)) throw new ArgumentNullException(nameof(directory));
			if (string.IsNullOrEmpty(baseName)) throw new ArgumentNullException(nameof(baseName));
			if (string.IsNullOrEmpty(ext)) throw new ArgumentNullException(nameof(ext));
			var fileNameComparer = new FileNameComparer(directory);

			// DSC_0123.ext
			var baseFiles = Directory.GetFiles(directory, $"{baseName}{suffix}{ext}")
				.ToArray();

			if (baseFiles.Length == 1)
			{
				fileName = baseFiles[0];
				return true;
			}

			// ..{DSC_0123}...ext
			baseFiles = Directory.GetFiles(directory, $"*{{{baseName}}}{suffix}{ext}")
				.ToArray();
			if (baseFiles.Length == 1)
			{
				fileName = baseFiles[0];
				return true;
			}

			if (baseFiles.Length > 1)
			{
				Debug.WriteLine($"File not unique, using first. Base: {baseName} "); //TODO
				fileName = baseFiles[0];
				return true;
			}

			Debug.WriteLine($"File not found. Base: {baseName}");
			fileName = null;
			return false;
		}

		public static bool TryGetBaseFile(string fileName, out string baseFile) =>
			TryGetBaseFile(fileName, null, out baseFile);
		public static bool TryGetBaseFile(string fileName, [CanBeNull] string baseName, out string baseFile)
		{
			// "base file" is the file w/ the baseName and w/o a suffix.
			if(string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(nameof(fileName));

			baseFile = null;
			if (baseName == null)
			{
				if(MediaFileName.TryParse(fileName, out var mediaFileName))
				{
					if (string.IsNullOrEmpty(mediaFileName.Suffix))
					{
						return false;
					}
					else
					{
						baseName = mediaFileName.BaseName.Trim('{', '}');
					}
				}
				else
				{
					SplitName(fileName, out baseName, out _);
				}
				
			}

			var name = Path.GetFileName(fileName);
			var ext = Path.GetExtension(fileName);
			var directory = Path.GetDirectoryName(fileName);

			// DSC_0123.ext
			var baseFiles = Directory.GetFiles(directory, baseName + ext)
				.Where(f => !Path.GetFileName(f).Equals(name, StringComparison.InvariantCultureIgnoreCase)) // exclude self
				.ToArray();

			if (baseFiles.Length == 1)
			{
				baseFile = baseFiles[0];
				return true;
			}

			if (baseFiles.Length > 1)
			{
				Debug.WriteLine($"{fileName} Base file not unique, using first.)"); //TODO
				baseFile = baseFiles[0];
				return true;
			}

			// ..{DSC_0123}...ext
			baseFiles = Directory.GetFiles(directory, $"*{{{baseName}}}{ext}")
				.Where(f => !Path.GetFileName(f).Equals(name, StringComparison.InvariantCultureIgnoreCase)) // exclude self
				.ToArray();
			if (baseFiles.Length == 1)
			{
				baseFile = baseFiles[0];
				return true;
			}

			if (baseFiles.Length > 1)
			{
				Debug.WriteLine($"{fileName} Base file not unique, using first.)");//TODO
				baseFile = baseFiles[0];
				return true;
			}

			// DSC_0123.*
			baseFiles = Directory.GetFiles(directory, baseName + ".*")
				.Where(f => !Path.GetFileName(f).Equals(name, StringComparison.InvariantCultureIgnoreCase)) // exclude self
				.ToArray();

			if (baseFiles.Length == 1)
			{
				baseFile = baseFiles[0];
				return true;
			}

			if (baseFiles.Length > 1)
			{
				Debug.WriteLine($"{fileName} Base file not unique, using first.)"); //TODO
				baseFile = baseFiles[0];
				return true;
			}

			// ..{DSC_0123}...*
			baseFiles = Directory.GetFiles(directory, $"*{{{baseName}}}.*")
				.Where(f => !Path.GetFileName(f).Equals(name, StringComparison.InvariantCultureIgnoreCase)) // exclude self
				.ToArray();
			if (baseFiles.Length == 1)
			{
				baseFile = baseFiles[0];
				return true;
			}

			if (baseFiles.Length > 1)
			{
				Debug.WriteLine($"{fileName} Base file not unique, using first.)");
				baseFile = baseFiles[0];
				return true;
			}

			Debug.WriteLine($"{fileName} Base file not found.)");
			baseFile = null;
			return false;
		}

		public static DateTime? GetDateTakenFromBase(string fileName)
		{
			if (!SplitName(fileName, out var baseName, out _)) return null;
			if (!TryGetBaseFile(fileName, baseName, out var baseFile)) return null;
			return GetDateTaken(baseFile);
		}

//		public static DateTime? GetDateTakenUseBitmapFrame([NotNull] string fileName)
//		{
//			using (var data = new MemoryStream(File.ReadAllBytes(fileName)))
//			{
//				try
//				{
//					var bitmap = BitmapFrame.Create(data);
//					var timestamp = GetDateTaken(bitmap);
//					return timestamp;
//				}
//				catch (Exception ex)
//				{
//					Debug.WriteLine($"{Path.GetFileName(fileName),-32} Bitmap not supported. {ex.Message}");
//					return null;
//				}
//			}
//		}

		public static DateTime? GetDateTaken([NotNull] string fileName) => GetDate(fileName, p => p.System.ItemDate);

		public static DateTime? GetDateAcquired([NotNull] string fileName)
		{
			using (var file = ShellFile.FromFilePath(fileName))
			{
				var d = file.Properties.System.ItemDate;
				return d.Value;
			}
		}

		public static DateTime? GetDate(string fileName, Func<ShellProperties, ShellProperty<DateTime?>> selector)
		{
			using (var file = ShellFile.FromFilePath(fileName))
			{
				var d = selector(file.Properties);
				return d.Value;
			}
		}

		private static DateTime? GetTimestampFromFileName(string fileName)
		{
			var patterns = new[]
			{
				// 20180921_181653  2018-09-09_17_36_10
				@"(?<yyyy>20\d{2})[^0-9]?(?<MM>\d{2})[^0-9]?(?<dd>\d{2})[^0-9]?(?<HH>\d{2})[^0-9]?(?<mm>\d{2})[^0-9]?(?<ss>\d{2})[^0-9]?(?<f>\d{0,3})",
			};

			foreach (var pattern in patterns)
			{
				var match = Regex.Match(Path.GetFileNameWithoutExtension(fileName), pattern);
				if (!match.Success) continue;

				if (!int.TryParse(match.Groups["yyyy"].Value, NumberStyles.Integer, null, out var year)) year = 0;
				if (!int.TryParse(match.Groups["MM"].Value, NumberStyles.Integer, null, out var month)) month = 0;
				if (!int.TryParse(match.Groups["dd"].Value, NumberStyles.Integer, null, out var day)) day = 0;
				if (!int.TryParse(match.Groups["HH"].Value, NumberStyles.Integer, null, out var hour)) hour = 0;
				if (!int.TryParse(match.Groups["mm"].Value, NumberStyles.Integer, null, out var minute)) minute = 0;
				if (!int.TryParse(match.Groups["ss"].Value, NumberStyles.Integer, null, out var second)) second = 0;
				if (!int.TryParse(match.Groups["f"].Value, NumberStyles.Integer, null, out var millisecond)) millisecond = 0;

				return new DateTime(year, month, day, hour, minute, second, millisecond);
			}

			return null;
		}

//		private static DateTime? GetDateTaken(BitmapSource bitmapSource)
//		{
//			// Ermitteln, ob Metadaten vorhanden sind
//			if (bitmapSource.Metadata == null) return null;
//			// Ermitteln, ob die Metadaten vom Typ BitmapMetadata sind
//			// (die einzige zurzeit von ImageMetadata abgeleitete Klasse)
//			var bitmapMetadata = bitmapSource.Metadata as BitmapMetadata;
//			if (bitmapMetadata == null) return null;
//			if (bitmapMetadata.DateTaken == null) return null;
//			// Versuch, das Datum mit der aktuellen Kultur zu konvertieren
//			DateTime result;
//			if (!DateTime.TryParse(bitmapMetadata.DateTaken, out result))
//			{
//				throw new FormatException("Der String '" + bitmapMetadata.DateTaken + "' kann nicht in einen DateTime-Wert " + "konvertiert werden");
//				return null;
//			}
//			return result;
//		}

		public static object GetValue(string fileName, Func<ShellProperties, IShellProperty> selector)
		{
			using (var file = ShellFile.FromFilePath(fileName))
			{
				try
				{
					var d = selector(file.Properties);
					return d.ValueAsObject;
				}
				catch (Exception ex)
				{
					Debug.WriteLine($"ERROR GetValue => {ex.Message}");
					return null;
				}
			}
		}

		public static double? GetDouble(string fileName, Func<ShellProperties, IShellProperty> selector)
		{
			using (var file = ShellFile.FromFilePath(fileName))
			{
				try
				{
					var d = selector(file.Properties);
					if (d.ValueAsObject == null) return null;
					return (double)Convert.ChangeType(d.ValueAsObject,typeof(double));
				}
				catch (Exception ex)
				{
					Debug.WriteLine($"ERROR GetValue => {ex.Message}");
					return null;
				}
			}
		}

		//https://stackoverflow.com/questions/5337683/how-to-set-extended-file-properties
		public static float GetExposureBias(string file)
		{
			var v = GetValue(file, p => p.System.Photo.ExposureBias);
			return v == null ? 0f : (float)((double) v);
		}

		public static bool TryGetExposureBias(string file, out float exposureBias)
		{
			exposureBias = 0f;
			var v = GetValue(file, p => p.System.Photo.ExposureBias);
			if (v == null) return false;
			exposureBias = (float)((double)v);
			return true;
		}


		public static string AddOffset(string originalName, int value)
		{
			var match = Regex.Match(originalName, @"^(?<prefix>.*?)(?<counter>\d+)$");
			if (!match.Success) return null;
			var counter = int.Parse(match.Groups["counter"].Value);
			var counterLength = match.Groups["counter"].Value.Length;
			counter += value;
			if (counter < 0) return null;
			var c = counter.ToString("D" + counterLength);
			if (c.Length > counterLength) return null; //overflow
			return $"{match.Groups["prefix"].Value}{c}";
		}


		public static bool SplitName(string fileName, out string baseName, out string suffix)
		{
			if(IsNormalizedName(fileName)) throw new ArgumentException("Name is already normaliezed!",nameof(fileName));

			fileName = Path.GetFileNameWithoutExtension(fileName);
			var suffixMatch = /*lang=regex*/ @"^(?<basename>.*?)(_?(?<suffix>~.+))$";
			if (!IsMatch(fileName, suffixMatch, out var match))
			{
				baseName = Path.GetFileNameWithoutExtension(fileName);
				suffix = null;
				return false;
			}
			baseName = match.Groups["basename"].Value;
			suffix = match.Groups["suffix"].Value;
			return true;
		}

		private static bool IsNormalizedName(string fileName) => MediaFileName.TryParse(fileName, out _);

		public static bool TryFindExposureDefaultFile(string refFile, out string exposureDefaultFile)
		{
			var sequenceFiles = GroupExposureSequence(refFile).ToArray();
			if (sequenceFiles.Length == 1) // no sequence
			{
				exposureDefaultFile = null; 
			}
			else
			{
				exposureDefaultFile = sequenceFiles
				.OrderBy(f => f.ExposureValue, new GenericComparer<double?>(CompareNearestDoubleAbsolute))
				.FirstOrDefault()?.OriginalFile.FullName;
			}
			return exposureDefaultFile != null;
		}

		public static bool TryFindExposureDefaultFileOld(string refFile, out string exposureDefaultFile)
		{
			var mediaFile = new MediaFileInfo(refFile);

			var evv = new[] {"EV0.0±", "EV0.3-", "EV0.3+", "EV0.7-", "EV0.7+", "EV1.0-", "EV1.0+", "EV1.3-", "EV1.3+", "EV1.7-", "EV1.7+", "EV2.0-", "EV2.0+" };
			

			// file with same time und EV:0
			{
				var t0 = mediaFile.TimestampString;
				var files = Directory.GetFiles(mediaFile.DirectoryName, $"{t0} EV0.0±*{mediaFile.Extension}");
				if (files.Length == 1)
				{
					exposureDefaultFile = files[0];
					return true;
				}
				if (files.Length > 1) Debugger.Break();
				
			}

			// file with time -1s und EV:0
			{
				var t0 = mediaFile.Timestamp.AddSeconds(-1).ToString(MediaFileName.TimestampFormat);
				var files = Directory.GetFiles(mediaFile.DirectoryName, $"{t0} EV0.0±*{mediaFile.Extension}");
				if (files.Length == 1)
				{
					exposureDefaultFile = files[0];
					return true;
				}
				if (files.Length > 1) Debugger.Break();
			}

			// match by decremented original name
			for (int i = 1; i < 8; i++)
			{
				var baseName = AddOffset(mediaFile.BaseName, -i);
				var file1 = Path.Combine(mediaFile.DirectoryName, baseName + mediaFile.Extension);
				if (File.Exists(file1))
				{
					var ev1 = GetExposureBias(file1);
					if (ev1.Equals(0f))
					{
						exposureDefaultFile = file1;
						return true;
					}
				}

				var files2 = Directory.GetFiles(mediaFile.DirectoryName, $"*{{{baseName}}}{mediaFile.Extension}");
				foreach (var file2 in files2)
				{
					var ev2 = GetExposureBias(file2);
					if (ev2.Equals(0f))
					{
						exposureDefaultFile = file2;
						return true;
					}
				}
			}

			Debug.WriteLine($"Base file not found! Current: {mediaFile.OriginalFile.FullName}");
			exposureDefaultFile = null;
			return false;
		}

		public static int CompareNearestDoubleAbsolute(double? m1, double? m2)
		{
			// Sort order: Null  0  -1  +1  +2  -3  ...
			
			if (!m1.HasValue && !m2.HasValue) return 0;
			if (!m1.HasValue) return -1;
			if (!m2.HasValue) return 1;

			var comp = Math.Abs(m1.Value).CompareTo(Math.Abs(m2.Value));
			if (comp != 0) return comp;
			comp = Math.Sign(m1.Value).CompareTo(Math.Sign(m2.Value));
			return comp;
		}

		public static List<MediaFileInfo> GroupExposureSequence(string file)
		{
			var refFile=new MediaFileInfo(file);
			var exposureValues = new List<double>();
			exposureValues.Add(Math.Round(refFile.ExposureValue.Value,1));


			var filesBefore = GetNeighbors(refFile, -9);
			var maxGap = 1.0;
			for (int i = 0; i < filesBefore.Count; i++)
			{
				var f = filesBefore[i];
				var ev = Math.Round(f.ExposureValue.Value, 1);
				maxGap += f.ExposureTime.Value + 0.01;
				var ca = exposureValues.Contains(ev);
				var cb = refFile.FocalLength != f.FocalLength || refFile.DigitalZoom != f.DigitalZoom;
				var cc = Math.Abs(refFile.DateTaken.Value.Subtract(f.DateTaken.Value).TotalSeconds) >= maxGap; 
				if (ca || cb || cc ) RemoveLeftover(filesBefore,i);
				else { exposureValues.Add(ev); }
			}

			var filesAfter=GetNeighbors(refFile, 9);
			maxGap = 1.0 + refFile.ExposureTime.Value;
			for (int i = 0; i < filesAfter.Count; i++)
			{
				var f = filesAfter[i];
				var ev = Math.Round(f.ExposureValue.Value, 1);
				maxGap += f.ExposureTime.Value + 0.01;
				var ca = exposureValues.Contains(ev);
				var cb = refFile.FocalLength != f.FocalLength || refFile.DigitalZoom != f.DigitalZoom;
				var cc = Math.Abs(refFile.DateTaken.Value.Subtract(f.DateTaken.Value).TotalSeconds) >= maxGap;
				if (ca || cb || cc) RemoveLeftover(filesAfter, i);
				else { exposureValues.Add(ev); }
			}

			var list=new List<MediaFileInfo>();
			filesBefore.Reverse();
			list.AddRange(filesBefore);
			list.Add(refFile);
			list.AddRange(filesAfter);
			return list;

			void RemoveLeftover(List<MediaFileInfo> l, int from)
			{
				for (int j = from; l.Count > from; j++) l.RemoveAt(from);
			}
		}

		public static List<MediaFileInfo> GetNeighbors(MediaFileInfo file, int v)
		{
			if (string.IsNullOrEmpty(file.BaseName)) throw new ArgumentException();
			if (!string.IsNullOrEmpty(file.Suffix)) throw new ArgumentException();

			var neighbors=new List<MediaFileInfo>();
			for (int i = 1; i < Math.Abs(v); i++)
			{
				var baseName = AddOffset(file.BaseName, Math.Sign(v) * i);
				if(baseName==null) break;
				if (!TryFindFile(file.DirectoryName, baseName, "", file.Extension, out var baseFile)) break;
				neighbors.Add(new MediaFileInfo(baseFile));
			}

			return neighbors;
		}

		public static List<MediaFileInfo> GetNeighbors(MediaFileInfo file, int start, int end)
		{
			if (!string.IsNullOrEmpty(file.Suffix)) throw new ArgumentException();

			var neighbors = new List<MediaFileInfo>();
			for (int i = start; i <= end; i++)
			{
				if(i==0) continue;
				var baseName = AddOffset(file.BaseName, i);
				if (TryFindFile(file.DirectoryName, baseName, "", file.Extension, out var baseFile))
				{
					neighbors.Add(new MediaFileInfo(baseFile));
				}
			}

			return neighbors;
		}

		public static string GetOriginalName(string fileName)
		{
			if (MediaFileName.TryParse(fileName, out var f)) return f.BaseName;
			SplitName(fileName, out var baseName, out _);
			return baseName;
		}

		public static bool TryRenameWithLog(string fileName, string newFileName)
		{
			const bool failed = false;
			var directory = Path.GetDirectoryName(fileName);
			var oldName = Path.GetFileName(fileName);
			var newName = Path.GetFileName(newFileName);

			var file = new FileInfo(Path.Combine(directory, oldName));
			if (!file.Exists) return failed;
			if (newFileName.Contains(":") || newFileName.Contains(@"\\"))
			{
				// full path
				if (!Path.GetDirectoryName(fileName).Equals(directory,StringComparison.OrdinalIgnoreCase))
				{
					DirectoryLog(fileName, $"Rename failed. Directory can not be changed! NewName: \"{newFileName}\"");
					return failed;
				}
			}
			else if(newFileName.Contains(@"\"))
			{
				// relative path
				DirectoryLog(fileName, $"Rename failed. Directory can not be changed! NewName: \"{newFileName}\"");
				return failed;
			}
			else
			{
				// no path
				newFileName = Path.Combine(directory, newFileName);
			}

			var newFile = new FileInfo(newFileName);
			if (newFile.Exists)
			{
				// throw new InvalidOperationException("File already exists!");
				DirectoryLog(newFile.FullName, "Rename failed. File already exist!");
				return failed;
			}

			try
			{
				File.Move(fileName, newFileName);
			}
			catch (Exception ex)
			{
				DirectoryLog(fileName, $"Rename failed. Exception: {ex.Message}");
				return failed;
			}

			#region rename.log
			try
			{
				using (var log = File.AppendText(Path.Combine(directory, "~rename.log")))
					log.WriteLine($"\"{oldName}\" => \"{newName}\"");
			}
			catch (Exception ex)
			{
				using (var log = File.AppendText(Path.Combine(directory, $"~rename-{DateTime.Now:yyyyMMddHHmmssfff}.log")))
					log.WriteLine($"\"{oldName}\" => \"{newName}\"");
			}
			#endregion

			#region ~rename-revert.cmd
			try
			{
				using (var log = new StreamWriter(File.Open(Path.Combine(directory, "~rename-revert.cmd"),FileMode.Append, FileAccess.Write, FileShare.None), GetOemEncoding()))
					log.WriteLine($"move \"{newName}\" \"{oldName}\"");
			}
			catch (Exception ex)
			{
				using (var log = new StreamWriter(File.Open(Path.Combine(directory, "~rename-revert-{DateTime.Now:yyyyMMddHHmmssfff}.cmd"), FileMode.Append, FileAccess.Write, FileShare.None), GetOemEncoding()))
					log.WriteLine($"move \"{newName}\" \"{oldName}\"");
			}
			#endregion

			return true;
		}

		public static bool TryMoveWithLog(string fileName, string newFileName)
		{
			const bool failed = false;
			var directory = Path.GetDirectoryName(fileName);
			var oldName = Path.GetFileName(fileName);
			var newName = Path.GetFileName(newFileName);

			var file = new FileInfo(fileName);
			if (!file.Exists) return failed;
			
			if (newFileName.Contains(":") || newFileName.Contains(@"\\"))
			{ // full path
			}
			else if (newFileName.Contains(@"\"))
			{ // relative path
				newFileName = Path.Combine(directory, newFileName);
			}
			else
			{ // no path
				newFileName = Path.Combine(directory, newFileName);
			}

			var newFile = new FileInfo(newFileName);
			if (newFile.Exists)
			{
				// throw new InvalidOperationException("File already exists!");
				DirectoryLog(newFile.FullName, "Move failed. File already exist!");
				return failed;
			}

			try
			{
				File.Move(fileName, newFileName);
			}
			catch (Exception ex)
			{
				DirectoryLog(fileName, $"Rename failed. Exception: {ex.Message}");
				return failed;
			}

			#region rename.log

			var msg = $"move \"{oldName}\" => \"{newName}\"";
			try
			{
				using (var log = File.AppendText(Path.Combine(directory, "~rename.log")))
					log.WriteLine(msg);
			}
			catch (Exception ex)
			{
				using (var log = File.AppendText(Path.Combine(directory, $"~rename-{DateTime.Now:yyyyMMddHHmmssfff}.log")))
					log.WriteLine($"\"{oldName}\" => \"{newName}\"");
			}
			#endregion

			#region ~rename-revert.cmd

			msg = $"move \"{newName}\" \"{oldName}\"";
			try
			{
				using (var log = new StreamWriter(File.Open(Path.Combine(directory, "~rename-revert.cmd"), FileMode.Append, FileAccess.Write, FileShare.None), GetOemEncoding()))
					log.WriteLine(msg);
			}
			catch (Exception ex)
			{
				using (var log = new StreamWriter(File.Open(Path.Combine(directory, "~rename-revert-{DateTime.Now:yyyyMMddHHmmssfff}.cmd"), FileMode.Append, FileAccess.Write, FileShare.None), GetOemEncoding()))
					log.WriteLine($"move \"{newName}\" \"{oldName}\"");
			}
			#endregion

			return true;
		}

		public static Encoding GetOemEncoding()
		{
			int lcid = GetSystemDefaultLCID();
			var ci = CultureInfo.GetCultureInfo(lcid);
			var page = ci.TextInfo.OEMCodePage;
			return Encoding.GetEncoding(page);
		}

		[System.Runtime.InteropServices.DllImport("kernel32.dll")]
		public static extern int GetSystemDefaultLCID();

		public static void DirectoryLog(string fileName, string message)
		{
			var directory = Path.GetDirectoryName(fileName);
			var name = Path.GetFileName(fileName);
			var m = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {name} {message}";
			try
			{
				using (var log = File.AppendText(Path.Combine(directory, "~error.log")))
					log.WriteLine(m);
			}
			catch (Exception ex)
			{
				using (var log = File.AppendText(Path.Combine(directory, $"~error-{DateTime.Now:yyyyMMddHHmmssfff}.log")))
					log.WriteLine(m);
			}
		}
	}

	public class DirectoryScanOptions
	{
		public bool IncludeHidden { get; set; }
		public bool IncludeSystem { get; set; }

		public bool IncludeReparsePoint { get; set; }

//		public bool Recursive { get; set; }
	}
}
