using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using COM3D2.i18nEx.Core.ScriptZip;
using COM3D2.i18nEx.Core.Util;
using UnityEngine;

namespace COM3D2.i18nEx.Core.TranslationManagers
{
	internal class ScriptTranslationFile
	{
		public ScriptTranslationFile(string fileName, string path)
		{
			this.FileName = fileName;
			string[] array = path.Split(new string[]
			{
				"\t"
			}, StringSplitOptions.RemoveEmptyEntries);
			this.FullPath = array[0];
			if (array.Length > 1)
			{
				this.FileOffset = uint.Parse(array[1]);
			}
		}

		public string FileName;/*
		{
			[CompilerGenerated]
			get
			{
				return this.<FileName>k__BackingField;
			}
		}
		*/
		public string FullPath;/*
		{
			[CompilerGenerated]
			get
			{
				return this.<FullPath>k__BackingField;
			}
		}
		*/

		public Dictionary<string, string> Translations= new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

		public Dictionary<string, List<SubtitleData>> Subtitles = new Dictionary<string, List<SubtitleData>>(StringComparer.InvariantCultureIgnoreCase);

		private void ParseVoiceSubtitle(string line)
		{
			try
			{
				SubtitleData subtitleData = JsonUtility.FromJson<SubtitleData>(line.Substring("@VoiceSubtitle".Length));
				List<SubtitleData> list;
				if (!this.Subtitles.TryGetValue(subtitleData.voice, out list))
				{
					list = (this.Subtitles[subtitleData.voice] = new List<SubtitleData>());
				}
				list.Add(subtitleData);
			}
			catch (Exception arg)
			{
				Core.Logger.LogWarning(string.Format("Failed to load subtitle line from {0}. Reason: {1}\n Line: {2}", this.FileName, arg, line));
			}
		}

		public void LoadTranslations()
		{
			this.Translations.Clear();
			if (!File.Exists(this.FullPath))
			{
				return;
			}
			string[] array;
			if (this.FileOffset == 4294967295U)
			{
				array = File.ReadAllLines(this.FullPath);
			}
			else
			{
				byte[] array2 = PKZip.ReadScriptData(this.FullPath, this.FileOffset);
				if (array2 == null)
				{
					return;
				}
				this.RawString = Encoding.UTF8.GetString(array2);
				array = this.RawString.Replace("\r", "").Split(new char[]
				{
					'\n'
				}, StringSplitOptions.RemoveEmptyEntries);
			}
			foreach (string text in array)
			{
				string text2 = text.Trim();
				if (text2.Length != 0 && !text2.StartsWith(";"))
				{
					if (text2.StartsWith("@VoiceSubtitle"))
					{
						this.ParseVoiceSubtitle(text2);
					}
					else
					{
						string[] array4 = text.Split(new char[]
						{
							'\t'
						}, StringSplitOptions.RemoveEmptyEntries);
						string key = array4[0].Unescape();
						string value = (array4.Length > 1) ? array4[1].Unescape() : null;
						this.Translations[key] = value;
					}
				}
			}
		}

		private const string VOICE_SUBTITLE_TAG = "@VoiceSubtitle";

		public uint FileOffset = uint.MaxValue;

		public string RawString;


	}
}
