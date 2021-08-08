using System.IO;
using System.Text.RegularExpressions;

namespace KsWare.MediaFileLib.Shared {

	public interface IProcessPlugin {
		string Name { get; }
		ProcessPriority Priority { get; }
		bool IsMatch(FileInfo fileInfo, out Match match);
		MediaFileInfo CreateMediaFileInfo(FileInfo fileInfo, Match match, string authorSign);
		bool Process(MediaFileInfo file);
	}

}
