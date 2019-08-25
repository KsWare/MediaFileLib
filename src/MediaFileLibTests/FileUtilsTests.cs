using System;
using System.IO;
using KsWare.MediaFileLib.Shared;
using NUnit.Framework;
using static KsWare.MediaFileLibTests.TestTools;

namespace KsWare.MediaFileLibTests
{
	[TestFixture]
	public class FileUtilsTests
	{
		[TearDown]
		public void Cleanup()
		{
			foreach (var file in Directory.GetFiles(Environment.CurrentDirectory))
				File.Delete(file);
		}

		private void CreateTestFile(string fileName)
		{
			var f = Path.Combine(Environment.CurrentDirectory, fileName);
			using (File.CreateText(f)) ;
		}

		[TestCase("DSC_0123.png", "DSC_0123", "DSC_0123.jpg")]
		[TestCase("DSC_0123.png", "DSC_0123", "timestamp {DSC_0123}.jpg")]
		[TestCase("DSC_0123~suffix.jpg", "DSC_0123", "DSC_0123.jpg")]
		[TestCase("DSC_0123~suffix.jpg", "DSC_0123", "timestamp {DSC_0123}.jpg")]
		[TestCase("DSC_0123~suffix.png", "DSC_0123", "timestamp {DSC_0123}.jpg")]
		[TestCase("timestamp {DSC_0123}~suffix.jpg", "DSC_0123", "DSC_0123.jpg")]
		[TestCase("timestamp {DSC_0123}~suffix.png", "DSC_0123", "DSC_0123.jpg")]
		[TestCase("timestamp {DSC_0123}~suffix.jpg", "DSC_0123", "timestamp {DSC_0123}.jpg")]
		[TestCase("timestamp {DSC_0123}~suffix.png", "DSC_0123", "timestamp {DSC_0123}.jpg")]
		public void TryGetBaseFile_Success(string fileName, string baseName, string expectedBaseFile)
		{
			fileName = Path.Combine(Environment.CurrentDirectory, fileName);
			expectedBaseFile = Path.Combine(Environment.CurrentDirectory, expectedBaseFile);
			CreateTestFile(expectedBaseFile);
			var success = FileUtils.TryGetBaseFile(fileName, baseName, out var baseFile);
			Assert.That(success,Is.EqualTo(true));
			Assert.That(baseFile,Is.EqualTo(expectedBaseFile));
		}

		[Test]
		public void GetDateTest()
		{
			var fn = GetTestDataPath("Sample.jpg");
			//			var fn = GetTestDataPath(@"E:\Fotos\2019-07-13 Import DC-FZ1000 II\P1001227.JPG");
//			var fn = GetTestDataPath(@"E:\Fotos\2019-07-06 Import DC-FZ1000 II\P1001048 - Kopie.JPG");
//			var fn = GetTestDataPath(@"E:\Fotos\2019-07-06 Import DC-FZ1000 II\P1001047 - Kopie.MP4");
			var creationTime = File.GetCreationTime(fn);
			var lastAccessTime = File.GetLastAccessTime(fn);
			var lastWriteTime = File.GetLastWriteTime(fn);

			var dateAcquired = FileUtils.GetDate(fn, p => p.System.DateAcquired);
			var dateCreated = FileUtils.GetDate(fn, p => p.System.DateCreated);
			var dateAccessed = FileUtils.GetDate(fn, p => p.System.DateAccessed);
			var dateArchived = FileUtils.GetDate(fn, p => p.System.DateArchived);
			var dateComplete = FileUtils.GetDate(fn, p => p.System.DateCompleted);
			var dateImported = FileUtils.GetDate(fn, p => p.System.DateImported);
			var dateModified = FileUtils.GetDate(fn, p => p.System.DateModified);
			var dueDate = FileUtils.GetDate(fn, p => p.System.DueDate);
			var endDate = FileUtils.GetDate(fn, p => p.System.EndDate);
			var itemDate = FileUtils.GetDate(fn, p => p.System.ItemDate);
			var startDate = FileUtils.GetDate(fn, p => p.System.StartDate);

			// dateAcquired null
			// dateCreated  same as creationTime (some seconds difference)
			// dateAccessed same as lastAccessTime (some seconds difference)
			// dateArchived null
			// dateComplete null
			// dateImported same lastWriteTime
			// dateModified same as lastWriteTime
			// dueDate      null
			// endDate      null
			// itemDate     seems to be the real date of photo taken
			// startDate    null
		}


		[TestCase("B90", 1, ExpectedResult = "B91")]
		[TestCase("B90", -1, ExpectedResult = "B89")]
		[TestCase("B100", -1, ExpectedResult = "B099")]
		[TestCase("B99", 1, ExpectedResult = null)] // Overflow
		[TestCase("B00", -1, ExpectedResult = null)] // Negative
		[TestCase("B0B90", 1, ExpectedResult = "B0B91")]
		[TestCase("00B90", 1, ExpectedResult = "00B91")]
		[TestCase("ABC", 1, ExpectedResult = null)]
		public string AddOffset(string originalName, int value)
		{
			return FileUtils.AddOffset(originalName, value);
		}

		[TestCase("YXZ000001.jpg",true,ExpectedResult = null)]
		[TestCase("YXZ000001~A.jpg", true, ExpectedResult = "YXZ000001.jpg")]
		[TestCase("YXZ000001_~A.jpg", true, ExpectedResult = "YXZ000001.jpg")] //Aurora Batch
		public string GetBaseFile(string fileName, bool fileExists)
		{
			fileName = Path.Combine(Environment.CurrentDirectory, fileName);
			if (fileExists) CreateTestFile(Path.Combine(Environment.CurrentDirectory, "YXZ000001.jpg"));

			FileUtils.SplitName(fileName, out var baseName, out _);
			FileUtils.TryGetBaseFile(fileName, baseName, out var baseFile);
			return Path.GetFileName(baseFile);
		}


		[TestCase(@"E:\Fotos\Gruppentest\S4003114.JPG")]
		[TestCase(@"E:\Fotos\Gruppentest\S4003115.JPG")]
		[TestCase(@"E:\Fotos\Gruppentest\S4003116.JPG")]
		[TestCase(@"E:\Fotos\Gruppentest\S4003117.JPG")]
		[TestCase(@"E:\Fotos\Gruppentest\S4003118.JPG")]
		public void Group1(string file)
		{
			if(!Directory.Exists(Path.GetDirectoryName(file))) Assert.Inconclusive($"Path not found! {file}");
			var group = FileUtils.GroupExposureSequence(file);
			Assert.That(group.Count, Is.EqualTo(5));
			Assert.That(group[0].BaseName, Is.EqualTo("S4003114"));
			Assert.That(group[4].BaseName, Is.EqualTo("S4003118"));
		}

		[TestCase(@"E:\Fotos\Gruppentest\S4003119.JPG")]
		[TestCase(@"E:\Fotos\Gruppentest\S4003120.JPG")]
		[TestCase(@"E:\Fotos\Gruppentest\S4003121.JPG")]
		[TestCase(@"E:\Fotos\Gruppentest\S4003122.JPG")]
		[TestCase(@"E:\Fotos\Gruppentest\S4003123.JPG")]
		public void Group2(string file)
		{
			if (!Directory.Exists(Path.GetDirectoryName(file))) Assert.Inconclusive($"Path not found! {file}");
			var group = FileUtils.GroupExposureSequence(file);
			Assert.That(group.Count,Is.EqualTo(5));
			Assert.That(group[0].BaseName, Is.EqualTo("S4003119"));
			Assert.That(group[4].BaseName, Is.EqualTo("S4003123"));
		}

		[TestCase(@"E:\Fotos\Gruppentest\S4003124.JPG")]
		[TestCase(@"E:\Fotos\Gruppentest\S4003125.JPG")]
		[TestCase(@"E:\Fotos\Gruppentest\S4003126.JPG")]
		[TestCase(@"E:\Fotos\Gruppentest\S4003127.JPG")]
		[TestCase(@"E:\Fotos\Gruppentest\S4003128.JPG")]
		public void Group3(string file)
		{
			if (!Directory.Exists(Path.GetDirectoryName(file))) Assert.Inconclusive($"Path not found! {file}");
			var group = FileUtils.GroupExposureSequence(file);
			Assert.That(group.Count, Is.EqualTo(5));
			Assert.That(group[0].BaseName, Is.EqualTo("S4003124"));
			Assert.That(group[4].BaseName, Is.EqualTo("S4003128"));
		}

		[Test]
		public void DateSubstract()
		{
			var t1 = new DateTime(2000, 1, 1, 12, 00, 00);
			var t2 = new DateTime(2000, 1, 1, 12, 00, 01);
			Assert.That(t2.Subtract(t1).TotalSeconds, Is.EqualTo(1.0));
			Assert.That(t1.Subtract(t2).TotalSeconds, Is.EqualTo(-1.0));
		}
	}
}
