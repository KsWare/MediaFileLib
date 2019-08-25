using System.IO;
using KsWare.MediaFileLib.Shared;
using NUnit.Framework;

namespace KsWare.MediaTimestampRenamerTests
{
	[TestFixture]
	public class FileNameTests
	{
		[TestCase("C:\\Name.ext", "C:\\", "Name", ".ext")]
		[TestCase("C:\\Folder\\Name.ext", "C:\\Folder", "Name", ".ext")]
		public void TryParse_Success(string input, string directory, string name, string extension)
		{
			var success = FileName.TryParse(input, out var sut);
			Assert.That(success, Is.EqualTo(true));
			Assert.That(sut.DirectoryName, Is.EqualTo(directory));
			Assert.That(sut.Name, Is.EqualTo(name));
			Assert.That(sut.Extension, Is.EqualTo(extension));
			Assert.That(sut.FullName, Is.EqualTo(Path.Combine(sut.DirectoryName,sut.Name+sut.Extension)));
		}


		// TODO TryParse_Fail
		//		[TestCase("C:\\Folder\\")]
		//		[TestCase("C:\\Folder")]
		//		[TestCase("foo.bar")]
		//		[TestCase("foo")]
		//		public void TryParse_Fail(string input)
		//		{
		//			var success = FileName.TryParse(input, out var sut);
		//			Assert.That(success, Is.EqualTo(false));
		//		}
	}
}