using System;
using System.IO;
using JetBrains.Annotations;

namespace KsWare.MediaFileLib.Shared {

	public class MediaFileInfo : MediaFileName {

		private Lazy<double?> _exposureValue;
		private Lazy<DateTime?> _dateTaken;
		private Lazy<double?> _focalLength;
		private Lazy<double?> _digitalZoom;
		private Lazy<double?> _exposureTimeNumerator;
		private Lazy<double?> _exposureTimeDenominator;
		private Lazy<double?> _exposureTime;
		private Lazy<double?> _brightness;

		public MediaFileInfo([NotNull] string fileName) {
			ParseCore(new FileInfo(fileName), this);
			InitializeLazyProperties();
		}

		/// <summary>
		/// Creates a new instance of <see cref="MediaFileInfo"/>.
		/// </summary>
		/// <param name="fileInfo">The <see cref="FileInfo"/>.</param>
		/// <param name="authorSign">The authors sign. (<b>NOTE: </b>Does dot overwrite the sign from <paramref name="fileInfo"/>.)</param>
		public MediaFileInfo([NotNull] FileInfo fileInfo, string authorSign) {
			ParseCore(fileInfo, this);
			if (string.IsNullOrEmpty(AuthorSign)) AuthorSign = authorSign;
			InitializeLazyProperties();
		}

		private void InitializeLazyProperties() {
			_dateTaken = new Lazy<DateTime?>(() => FileUtils.GetDateTaken(OriginalFile.FullName));
			_focalLength = new Lazy<double?>(() =>
				FileUtils.GetDouble(OriginalFile.FullName, p => p.System.Photo.FocalLength));
			_digitalZoom = new Lazy<double?>(() =>
				FileUtils.GetDouble(OriginalFile.FullName, p => p.System.Photo.DigitalZoom));
			_exposureTimeNumerator = new Lazy<double?>(() =>
				FileUtils.GetDouble(OriginalFile.FullName, p => p.System.Photo.ExposureTimeNumerator));
			_exposureTimeDenominator = new Lazy<double?>(() =>
				FileUtils.GetDouble(OriginalFile.FullName, p => p.System.Photo.ExposureTimeDenominator));
			_exposureTime = new Lazy<double?>(() =>
				_exposureTimeNumerator.Value.HasValue && _exposureTimeDenominator.Value.HasValue
					? _exposureTimeNumerator.Value.Value / _exposureTimeDenominator.Value.Value
					: (double?) null);
			_exposureValue = new Lazy<double?>(() =>
				FileUtils.GetDouble(OriginalFile.FullName, p => p.System.Photo.ExposureBias));
			_brightness = new Lazy<double?>(() =>
				FileUtils.GetDouble(OriginalFile.FullName, p => p.System.Photo.Brightness));
		}

		public new double? ExposureValue => _exposureValue.Value;

		public new double? Brightness => _brightness.Value;

		public bool IsRenamed { get; set; }

		public bool Exists => File.Exists(ToString());

		public FileInfo OriginalFile { get; set; }

		public FileInfo OldFile { get; set; }


		public string TimestampString => Timestamp.ToString(MediaFileName.TimestampFormat);
		public DateTime? DateTaken => _dateTaken.Value;
		public double? FocalLength => _focalLength.Value;
		public double? DigitalZoom => _digitalZoom.Value;
		public double? ExposureTime => _exposureTime.Value;
		public TimeSpan? DiffTime { get; set; }

		public GroupType GroupType { get; set; }
		public string GroupIndex { get; set; }

		public override string ToString() {
			var baseName = string.IsNullOrEmpty(BaseName) ? "" : $" {{{BaseName}}}";
			var groupIndex = string.IsNullOrWhiteSpace(GroupIndex) ? null : GroupIndex;
			if (groupIndex==null) groupIndex = GroupType == GroupType.ExposureValue ? FileUtils.ExposureValueToString(ExposureValue) : null;
			if (groupIndex != null) groupIndex = " " + groupIndex;

			var name = Timestamp.ToString(MediaFileName.TimestampFormat) + Counter + groupIndex + " " + AuthorSign +
			           baseName;
			if (!string.IsNullOrEmpty(Suffix)) name += Suffix;
			else if (!name.EndsWith(" ") && !string.IsNullOrEmpty(Suffix) && !Suffix.StartsWith(" "))
				name += " " + Suffix;
			else name += Suffix;

			return Path.Combine(DirectoryName, name + Extension);
		}

		public string CreateUniqueFileName() {
			while (true) {
				if (!Exists) break;
				if (string.IsNullOrEmpty(Counter)) Counter = "-2";
				else if (Counter.Length == 4) {
					var c = int.Parse(Counter.Substring(1)) + 1;
					Counter = $"-{c:D3}";
				}
				else {
					var c = int.Parse(Counter.Substring(1)) + 1;
					Counter = $"-{c}";
				}

				if (Counter.Length > 4) {
					//TODO
				}
			}

			return ToString();
		}

		public bool Rename(string pluginName) {
			if (IsRenamed) return false;
			OldFile = OriginalFile;
			OriginalFile = new FileInfo(CreateUniqueFileName());
			if (!FileUtils.TryRenameWithLog(OldFile.FullName, OriginalFile.FullName, pluginName)) return false;

			IsRenamed = true;
			return true;
		}

		private static bool ParseCore(FileInfo file, MediaFileInfo f) {
			f.OriginalFile = file;
			if (MediaFileName.ParseCore(file.FullName, f)) {
				f.IsRenamed = true;
			}
			else {
				FileUtils.SplitName(f.OriginalFile.Name, out var baseName, out var suffix);
				f.BaseName = baseName;
				f.Suffix = suffix;
				f.Timestamp = FileUtils.GetDateTaken(f.OriginalFile.FullName) ?? f.OriginalFile.LastWriteTime;
				f.IsRenamed = false;
			}

			return true;
		}

//		public static MediaFileInfo Parse(string fileName)
//		{
//			var f = new MediaFileInfo();
//			if(!ParseCore(fileName, f)) throw new ArgumentException(); //TODO ArgumentException message
//			return f;
//		}
//
//		public static bool TryParse(string fileName, out MediaFileInfo mediaFileInfo)
//		{
//			var f = new MediaFileInfo();
//			mediaFileInfo = ParseCore(fileName, f) ? f : null;
//			return mediaFileInfo != null;
//		}
	}

}