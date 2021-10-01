using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using COM3D2.i18nEx.Core.Util;
using ExIni;

namespace COM3D2.i18nEx.Core.Loaders
{
	internal class BasicTranslationLoader : ITranslationLoader
	{
		public string CurrentLanguage
		{
			//[CompilerGenerated]
			get
			{
				return this.CurrentLanguagek__BackingField;
			}
			//[CompilerGenerated]
			private set
			{
				this.CurrentLanguagek__BackingField = value;
			}
		}

		public void SelectLanguage(string name, string path, IniFile config)
		{
			this.CurrentLanguage = name;
			this.langPath = path;
			Core.Logger.LogInfo("Loading language \"" + this.CurrentLanguage + "\"");
		}

		public void UnloadCurrentTranslation()
		{
			Core.Logger.LogInfo("Unloading language \"" + this.CurrentLanguage + "\"");
			this.CurrentLanguage = null;
			this.langPath = null;
		}

		public IEnumerable<string> GetScriptTranslationFileNames()
		{
			string path = Path.Combine(this.langPath, "Script");
			if (!Directory.Exists(path))
			{
				return null;
			}
			return Directory.GetFiles(path, "*.txt", SearchOption.AllDirectories);
		}

		public IEnumerable<string> GetScriptTranslationZipNames()
		{
			string path = Path.Combine(this.langPath, "Script");
			if (!Directory.Exists(path))
			{
				return null;
			}
			return Directory.GetFiles(path, "*.zip", SearchOption.AllDirectories);
		}

		public IEnumerable<string> GetTextureTranslationFileNames()
		{
			string path = Path.Combine(this.langPath, "Textures");
			if (!Directory.Exists(path))
			{
				return null;
			}
			return Directory.GetFiles(path, "*.png", SearchOption.AllDirectories);
		}

		public SortedDictionary<string, IEnumerable<string>> GetUITranslationFileNames()
		{
			string text = Path.Combine(this.langPath, "UI");
			if (!Directory.Exists(text))
			{
				return null;
			}
			SortedDictionary<string, IEnumerable<string>> sortedDictionary = new SortedDictionary<string, IEnumerable<string>>(StringComparer.InvariantCultureIgnoreCase);
			string[] directories = Directory.GetDirectories(text, "*", SearchOption.TopDirectoryOnly);
			for (int i = 0; i < directories.Length; i++)
			{
				string directory = directories[i];
				string key = directory.Splice(text.Length, -1).Trim(new char[]
				{
					'\\',
					'/'
				});
				sortedDictionary.Add(key, from s in Directory.GetFiles(directory, "*.csv", SearchOption.AllDirectories)
				select s.Splice(directory.Length + 1, -1));
			}
			return sortedDictionary;
		}

		public Stream OpenScriptTranslation(string path)
		{
			if (File.Exists(path))
			{
				return File.OpenRead(path);
			}
			return null;
		}

		public Stream OpenTextureTranslation(string path)
		{
			if (File.Exists(path))
			{
				return File.OpenRead(path);
			}
			return null;
		}

		public Stream OpenUiTranslation(string path)
		{
			path = Utility.CombinePaths(new string[]
			{
				this.langPath,
				"UI",
				path
			});
			if (File.Exists(path))
			{
				return File.OpenRead(path);
			}
			return null;
		}

		public BasicTranslationLoader()
		{
		}

		private string langPath;

		//[CompilerGenerated]
		private string CurrentLanguagek__BackingField;
		/*

		[CompilerGenerated]
		private sealed class <>c__DisplayClass10_0
		{
			public <>c__DisplayClass10_0()
			{
			}

			internal string <GetUITranslationFileNames>b__0(string s)
			{
				return s.Splice(this.directory.Length + 1, -1);
			}

			public string directory;
		}
		*/
	}
}
