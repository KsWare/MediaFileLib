using KsWare.MediaFileLib.Shared;
using NUnit.Framework;

namespace KsWare.MediaFileLibTests
{
	[TestFixture]
	public class MediaFileNameTests
	{

		[TestCase(@"C:\2019-06-29 165723 KS71.jpg", "2019-06-29 165723","", "", "KS71","","")]
		[TestCase(@"C:\2019-06-29 165723-001 KS71.jpg", "2019-06-29 165723", "-001", "", "KS71", "", "")]
		[TestCase(@"C:\2019-06-29 165723 KS71 {basename}.jpg", "2019-06-29 165723", "", "", "KS71", "basename", "")]
		[TestCase(@"C:\2019-06-29 165723 KS71 {basename}~suffix.jpg", "2019-06-29 165723", "", "", "KS71", "basename", "~suffix")]
		[TestCase(@"C:\2019-06-29 165723 KS71~suffix.jpg", "2019-06-29 165723", "", "", "KS71", "", "~suffix")]
		[TestCase(@"C:\2019-06-29 165723 EV0.3- KS71.jpg", "2019-06-29 165723", "", " EV0.3-", "KS71", "", "")]
		[TestCase(@"C:\2019-06-29 165723-001 EV0.3- KS71 {basename}~suffix.jpg", "2019-06-29 165723", "-001", " EV0.3-", "KS71", "basename", "~suffix")]
		public void TryParse_Success(string input, string timestamp, string counter, string exposureValue, string sign, string baseName, string suffix)
		{

			var success = MediaFileName.TryParse(input, out var sut);
			Assert.That(success, Is.EqualTo(true));
			Assert.That(sut.Timestamp, Is.EqualTo(MediaFileName.ParseTimestamp(timestamp)));
			Assert.That(sut.Counter, Is.EqualTo(counter));
			Assert.That(sut.ExposureValue, Is.EqualTo(exposureValue));
			Assert.That(sut.AuthorSign, Is.EqualTo(sign));
			Assert.That(sut.BaseName, Is.EqualTo(baseName));
			Assert.That(sut.Suffix, Is.EqualTo(suffix));
		}

		[TestCase(@"C:\basename.jpg")]
		[TestCase(@"C:\basename~suffix.jpg")]
		public void TryParse_Fail(string input)
		{
			var success = MediaFileName.TryParse(input, out var sut);
			Assert.That(success, Is.EqualTo(false));
		}
	}
}