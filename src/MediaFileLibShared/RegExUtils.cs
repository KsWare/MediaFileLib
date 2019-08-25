using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace KsWare.MediaFileLib.Shared
{
	// using static KsWare.MediaFileLib.Shared.RegExUtils;

	public static class RegExUtils
	{
		public static bool IsMatch(string input, [RegexPattern] string pattern, out Match match)
		{
			match = Regex.Match(input, pattern, RegexOptions.IgnorePatternWhitespace);
			return match.Success;
		}
	}
}
