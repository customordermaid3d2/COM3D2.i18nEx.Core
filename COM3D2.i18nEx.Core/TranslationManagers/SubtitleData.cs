using System;

namespace COM3D2.i18nEx.Core.TranslationManagers
{
	[Serializable]
	internal class SubtitleData
	{
		public SubtitleData()
		{
		}

		public int addDisplayTime;

		public int displayTime = -1;

		public bool isCasino;

		public string original = string.Empty;

		public int startTime;

		public string translation = string.Empty;

		public string voice = string.Empty;
	}
}
