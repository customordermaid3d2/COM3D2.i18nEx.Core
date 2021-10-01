using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using COM3D2.i18nEx.Core.Util;
using I2.Loc;
using UnityEngine;

namespace COM3D2.i18nEx.Core.TranslationManagers
{
	internal class I2TranslationManager : TranslationManagerBase
	{
		public override void LoadLanguage()
		{
			UnityEngine.Object.DontDestroyOnLoad(this.go);
			Core.Logger.LogInfo("Loading UI translations");
			this.LoadTranslations();
		}

		private void LoadTranslations()
		{
			SortedDictionary<string, IEnumerable<string>> uitranslationFileNames = Core.TranslationLoader.GetUITranslationFileNames();
			if (uitranslationFileNames == null)
			{
				Core.Logger.LogInfo("No UI translations found! Skipping...");
				return;
			}
			LanguageSource languageSource = this.go.GetComponent<LanguageSource>() ?? this.go.AddComponent<LanguageSource>();
			languageSource.name = "i18nEx";
			languageSource.ClearAllData();
			foreach (KeyValuePair<string, IEnumerable<string>> keyValuePair in uitranslationFileNames.OrderByDescending((KeyValuePair<string, IEnumerable<string>> k) => k.Key, StringComparer.InvariantCultureIgnoreCase))
			{
				string key = keyValuePair.Key;
				IEnumerable<string> value = keyValuePair.Value;
				if (Configuration.I2Translation.VerboseLogging.Value)
				{
					Core.Logger.LogInfo("Loading unit " + key);
				}
				foreach (string text in value)
				{
					string text2 = text.Replace("\\", "/").Splice(0, -5);
					if (Configuration.I2Translation.VerboseLogging.Value)
					{
						Core.Logger.LogInfo("Loading category " + text2);
					}
					string csvstring;
					using (StreamReader streamReader = new StreamReader(Core.TranslationLoader.OpenUiTranslation(string.Format("{0}{1}{2}", key, Path.DirectorySeparatorChar, text))))
					{
						csvstring = streamReader.ReadToEnd().ToLF();
					}
					languageSource.Import_CSV(text2, csvstring, eSpreadsheetUpdateMode.Merge, ',', null);
				}
			}
			Core.Logger.LogInfo("Loaded the following languages: " + string.Join(",", (from d in languageSource.mLanguages
			select d.Name).ToArray<string>()));
			LocalizationManager.LocalizeAll(true);
		}

		private void Update()
		{
			if (Configuration.I2Translation.ReloadTranslationsKey.Value.IsPressed)
			{
				this.ReloadActiveTranslations();
			}
		}

		public override void ReloadActiveTranslations()
		{
			Core.Logger.LogInfo("Reloading current I2 translations");
			this.LoadTranslations();
		}

		public I2TranslationManager()
		{
		}

		private readonly GameObject go = new GameObject();
		/*
		[CompilerGenerated]
		[Serializable]
		private sealed class <>c
		{
			// Note: this type is marked as 'beforefieldinit'.
			static <>c()
			{
			}

			public <>c()
			{
			}

			internal string <LoadTranslations>b__2_1(KeyValuePair<string, IEnumerable<string>> k)
			{
				return k.Key;
			}

			internal string <LoadTranslations>b__2_0(LanguageData d)
			{
				return d.Name;
			}

			public static readonly I2TranslationManager.<>c <>9 = new I2TranslationManager.<>c();

			public static Func<KeyValuePair<string, IEnumerable<string>>, string> <>9__2_1;

			public static Func<LanguageData, string> <>9__2_0;
		}
		*/
	}
}
