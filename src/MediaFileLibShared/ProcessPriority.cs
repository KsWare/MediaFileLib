using System;
using System.Collections.Generic;
using System.Text;

namespace KsWare.MediaFileLib.Shared {

	public enum ProcessPriority {
		None,
		FullNameMatch,
		NameMatch,
		ExtensionMatch,
		BeforeDefault,
		Default //TODO better name for "Default"
		,
	}

}
