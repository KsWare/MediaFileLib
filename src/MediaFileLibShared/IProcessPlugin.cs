using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace KsWare.MediaFileLib.Shared
{
	public interface IProcessPlugin
	{
		ProcessPriority Priority { get; }
		bool IsMatch(FileInfo fileInfo, out Match match);
		MediaFileInfo CreateMediaFileInfo(FileInfo fileInfo, Match match, string authorSign);
		bool Process(MediaFileInfo file);
	}
}
