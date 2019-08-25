using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;

namespace KsWare.MediaFileLib.Shared
{
	public class MediaFileName : FileName
	{
		[SuppressMessage("ReSharper", "InconsistentNaming")] 
		private static readonly IFormatProvider enUS = new CultureInfo("en-US");
		public const string TimestampFormat = "yyyy-MM-dd HHmmss";
		public const string ExposureValuePattern = /*language=regexp*/ @"(?<ev>\sEV(?<value>\d\.\d)(?<sign>[+±-]))?";

		protected MediaFileName() { }

		public DateTime Timestamp { get; set; }
		public string Counter { get; set; }
		public string ExposureValue { get; set; }
		public string AuthorSign { get; set; }
		public string BaseName { get; set; }
		public string Suffix { get; set; }


		public static bool TryParse(string fileName, out MediaFileName mediaFileName)
		{
			var f=new MediaFileName();
			mediaFileName = ParseCore(fileName, f) ? f : null;
			return mediaFileName!=null;
		}

		private protected static bool ParseCore(string fileName, MediaFileName mediaFileName)
		{
			FileName.Parse(fileName, mediaFileName);
			var timestamp = /*language=regexp*/ @"(?<timestamp>(\d{4})-(\d{2})-(\d{2})\s(\d{6,9}))";
			var counter = /*  language=regexp*/ @"(?<counter>(-\d{1,3})?)";
			var expValue = /* language=regexp*/ @"(?<ev>\sEV\d\.\d[+±-])?";
			var author = /*   language=regexp*/ @"(?<author>.{2,4})";
			var baseName = /* language=regexp*/ @"(\s{(?<baseName>[^}]+)})?";
			var suffix = /*   language=regexp*/ @"(?<suffix>.*)";
			var match = Regex.Match(mediaFileName.Name, $@"^{timestamp}{counter}{expValue}\s{author}{baseName}{suffix}$");
			if (!match.Success) return false;

			mediaFileName.Timestamp = ParseTimestamp(match.Groups["timestamp"].Value);
			mediaFileName.Counter = match.Groups["counter"].Value;
			mediaFileName.ExposureValue = match.Groups["ev"].Value;
			mediaFileName.AuthorSign = match.Groups["author"].Value;
			mediaFileName.BaseName = match.Groups["baseName"].Value;
			mediaFileName.Suffix = match.Groups["suffix"].Value;
			return true;
		}

		public static DateTime ParseTimestamp(object value)
		{
			if (value is string dateTimeString)
			{
				var t = DateTime.ParseExact(dateTimeString, TimestampFormat, CultureInfo.InvariantCulture);
				return t;
			}
			if (value is DateTime dateTime)
			{
				return dateTime;
			}
			throw new ArgumentException(@"Unsupported data type.", nameof(value));
		}

		internal static double? ParseExposureValue(string input)
		{
			if (string.IsNullOrEmpty(input)) return null;

			var match = Regex.Match(input, MediaFileName.ExposureValuePattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
			if (!match.Success) return null;
			var value = Double.Parse(match.Groups["value"].Value, enUS);
			return match.Groups["sign"].Value == "-" ? -1 * value : value;
		}
	}
}
