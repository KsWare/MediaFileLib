using System.IO;
using System.Reflection;

namespace KsWare.MediaFileLibTests
{
	internal static class TestTools
	{
		public static string GetTestDataPath(string name)
		{
			var folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			return Path.Combine(folder, "TestData", name);
		}
	}
}
