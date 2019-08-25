using System;
using KsWare.MediaFileLib.Shared;
using NUnit.Framework;

namespace KsWare.MediaFileLibTests
{
	[TestFixture()]
	public class NearestTimeComparerTests
	{
		[TestCase(0, 1, ExpectedResult = -1)]
		[TestCase(1, 0, ExpectedResult = +1)]
		[TestCase(1, 1, ExpectedResult = 0)]
		[TestCase(0, 0, ExpectedResult = 0)]
		[TestCase(-1, -1, ExpectedResult = 0)]
		[TestCase(1, -1, ExpectedResult = 1)]
		[TestCase(-1, 1, ExpectedResult = -1)]
		[TestCase(1, -2, ExpectedResult = -1)]
		[TestCase(-2, 1, ExpectedResult = +1)]
		public int Compare(double a, double b)
		{
			var compTime = new DateTime(2000, 1, 1, 12, 00, 00);
			var sut = new NearestTimeComparer(compTime);
			return sut.Compare(compTime.AddSeconds(a), compTime.AddSeconds(b));
		}
	}
}