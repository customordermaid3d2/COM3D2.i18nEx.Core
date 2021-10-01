using System;
using System.Collections.Generic;
using System.IO;
using COM3D2.i18nEx.Core.TranslationManagers;
using COM3D2.i18nEx.Core.Util;
using ExIni;
using UnityEngine;

namespace COM3D2.i18nEx.Core
{
	internal static class Configuration
	{
		static Configuration()
		{
			IniSection iniSection = Configuration.configFile["ScriptTranslations"];
			if (iniSection.HasKey("InsertJapaneseTextIntoEnglishText"))
			{
				bool flag;
				if (bool.TryParse(iniSection["InsertJapaneseTextIntoEnglishText"].Value, out flag))
				{
					Configuration.ScriptTranslations.RerouteTranslationsTo.Value = (flag ? TranslationsReroute.RouteToEnglish : TranslationsReroute.None);
				}
				iniSection.DeleteKey("InsertJapaneseTextIntoEnglishText");
				Configuration.configFile.Save(Paths.ConfigurationFilePath);
			}
		}

		public static void Reload()
		{
			Configuration.configFile.Merge(IniFile.FromFile(Paths.ConfigurationFilePath));
			foreach (IReloadable reloadable in Configuration.reloadableWrappers)
			{
				reloadable.Reload();
			}
		}

		private static ConfigWrapper<T> Wrap<T>(string section, string key, string description = "", T @default = default(T), Func<T, string> toStringConvert = null, Func<string, T> fromStringConvert = null)
		{
			ConfigWrapper<T> configWrapper = new ConfigWrapper<T>(Configuration.configFile, Paths.ConfigurationFilePath, section, key, description, @default, toStringConvert, fromStringConvert);
			Configuration.reloadableWrappers.Add(configWrapper);
			return configWrapper;
		}

		private static readonly IniFile configFile = File.Exists(Paths.ConfigurationFilePath) ? IniFile.FromFile(Paths.ConfigurationFilePath) : new IniFile();

		private static readonly List<IReloadable> reloadableWrappers = new List<IReloadable>();

		public static readonly Configuration.GeneralConfig General = new Configuration.GeneralConfig();

		public static readonly Configuration.ScriptTranslationsConfig ScriptTranslations = new Configuration.ScriptTranslationsConfig();

		public static readonly Configuration.TextureReplacementConfig TextureReplacement = new Configuration.TextureReplacementConfig();

		public static readonly Configuration.I2TranslationConfig I2Translation = new Configuration.I2TranslationConfig();

		internal class GeneralConfig
		{
			public GeneralConfig()
			{
			}

			public ConfigWrapper<bool> FixSubtitleType = Configuration.Wrap<bool>("General", "FixGameSubtitleType", "DO NOT TOUCH: If enabled, i18nEx will reset game subtitle type to Japanese on the next game run", true, null, null);

			public ConfigWrapper<string> ActiveLanguage = Configuration.Wrap<string>("General", "ActiveLanguage", "Currently selected language", "English", null, null);

			public ConfigWrapper<KeyCommand> ReloadConfigKey = Configuration.Wrap<KeyCommand>("General", "ReloadConfigKey", "The key to reload current configuration file", new KeyCommand(new KeyCode[]
			{
				KeyCode.LeftControl,
				KeyCode.F12
			}), KeyCommand.KeyCommandToString, KeyCommand.KeyCommandFromString);

			public ConfigWrapper<KeyCommand> ReloadTranslationsKey = Configuration.Wrap<KeyCommand>("General", "ReloadConfigKey", "The key to reload current configuration file", new KeyCommand(new KeyCode[]
			{
				KeyCode.LeftAlt,
				KeyCode.F12
			}), KeyCommand.KeyCommandToString, KeyCommand.KeyCommandFromString);
		}

		internal class ScriptTranslationsConfig
		{
			public ScriptTranslationsConfig()
			{
			}

			public ConfigWrapper<double> ClipboardCaptureTime = Configuration.Wrap<double>("ScriptTranslations", "ClipboardCaptureTime", "If `SendScriptToClipboard` is enabled, specifies the time to wait before sending all input to clipboard.", 0.25, null, null);

			public ConfigWrapper<bool> DumpScriptTranslations = Configuration.Wrap<bool>("ScriptTranslations", "DumpUntranslatedLines", "If enabled, dumps untranslated script lines (along with built-in translations, if present).", false, null, null);

			public ConfigWrapper<int> MaxTranslationFilesCached = Configuration.Wrap<int>("ScriptTranslations", "CacheSize", "Specifies how many text translation files should be kept in memory at once\nHaving bigger cache can improve performance at the cost of memory usage", 1, null, null);

			public ConfigWrapper<KeyCommand> ReloadTranslationsKey = Configuration.Wrap<KeyCommand>("ScriptTranslations", "ReloadTranslationsKey", "The key (or key combination) to reload all translations.", new KeyCommand(new KeyCode[]
			{
				KeyCode.LeftAlt,
				KeyCode.Keypad1
			}), KeyCommand.KeyCommandToString, KeyCommand.KeyCommandFromString);

			public ConfigWrapper<TranslationsReroute> RerouteTranslationsTo = Configuration.Wrap<TranslationsReroute>("ScriptTranslations", "RerouteTranslationsTo", "Allows you to route both English and Japanese translations into a single textbox instead of viewing both\nSupports the following values:\nNone -- Disabled. English text is written into English textbox; Japanese into Japanese\nRouteToEnglish -- Puts Japanese text into English textbox if there is no translation text available\nRouteToJapanese -- Puts translations into Japanese textbox if there is a translation available", TranslationsReroute.Reverse, EnumConverter<TranslationsReroute>.EnumToString, EnumConverter<TranslationsReroute>.EnumFromString);

			public ConfigWrapper<bool> SendScriptToClipboard = Configuration.Wrap<bool>("ScriptTranslations", "SendToClipboard", "If enabled, sends untranslated story text to clipboard.", false, null, null);

			public ConfigWrapper<bool> VerboseLogging = Configuration.Wrap<bool>("ScriptTranslations", "VerboseLogging", "If enabled, logs precise translation info\nUseful if you're writing new translations.", false, null, null);
		}

		internal class TextureReplacementConfig
		{
			public TextureReplacementConfig()
			{
			}

			public ConfigWrapper<bool> DumpTextures = Configuration.Wrap<bool>("TextureReplacement", "DumpOriginalTextures", "If enabled, dumps textures that have no replacements.", false, null, null);

			public ConfigWrapper<int> MaxTexturesCached = Configuration.Wrap<int>("TextureReplacement", "CacheSize", "Specifies how many texture replacements should be kept in memory at once\nHaving bigger cache can improve performance at the cost of memory usage", 10, null, null);

			public ConfigWrapper<KeyCommand> ReloadTranslationsKey = Configuration.Wrap<KeyCommand>("TextureReplacement", "ReloadTranslationsKey", "The key (or key combination) to reload all translations.", new KeyCommand(new KeyCode[]
			{
				KeyCode.LeftAlt,
				KeyCode.Keypad2
			}), KeyCommand.KeyCommandToString, KeyCommand.KeyCommandFromString);

			public ConfigWrapper<bool> SkipDumpingCMTextures = Configuration.Wrap<bool>("TextureReplacement", "SkipDumpingCMTextures", "If `DumpOriginalTextures` is enabled, setting this to `True` will disable dumping game's own .tex files\nUse this if you don't want to dump all in-game textures.", true, null, null);

			public ConfigWrapper<bool> VerboseLogging = Configuration.Wrap<bool>("TextureReplacement", "VerboseLogging", "If enabled, logs precise texture replacement info\nUseful if you're writing new translations.", false, null, null);
		}

		internal class I2TranslationConfig
		{
			public I2TranslationConfig()
			{
			}

			public ConfigWrapper<string> CustomUIFont = Configuration.Wrap<string>("I2Translation", "CustomUIFont", "If specified, replaces the UI font with this one.\nIMPORTANT: The font **must** be installed on your machine and it **must** be a TrueType font.", "", null, null);

			public ConfigWrapper<KeyCommand> PrintFontNamesKey = Configuration.Wrap<KeyCommand>("I2Translation", "PrintFontNamesKey", "The key (or key combination) do display all supported UI fonts in the console.", new KeyCommand(new KeyCode[]
			{
				KeyCode.LeftAlt,
				KeyCode.F11
			}), KeyCommand.KeyCommandToString, KeyCommand.KeyCommandFromString);

			public ConfigWrapper<KeyCommand> ReloadTranslationsKey = Configuration.Wrap<KeyCommand>("I2Translation", "ReloadTranslationsKey", "The key (or key combination) to reload all translations.", new KeyCommand(new KeyCode[]
			{
				KeyCode.LeftAlt,
				KeyCode.Keypad3
			}), KeyCommand.KeyCommandToString, KeyCommand.KeyCommandFromString);

			public ConfigWrapper<bool> VerboseLogging = Configuration.Wrap<bool>("I2Translation", "VerboseLogging", "If enabled, logs precise I2Loc loading and translation info\nUseful if you're debugging.", false, null, null);

			public ConfigWrapper<bool> DumpTexts = Configuration.Wrap<bool>("I2Translation", "DumpUntranslatedUITexts", "If enabled, dumps untranslated UI texts", false, null, null);

			public ConfigWrapper<bool> OverrideSubtitleOpacity = Configuration.Wrap<bool>("I2Translation", "OverrideSubtitleOpacity", "If enabled, allows to change subtitle box opacity without affecting other elements.", false, null, null);

			public ConfigWrapper<float> SubtitleOpacity = Configuration.Wrap<float>("I2Translation", "SubtitleOpacity", "If OverrideSubtitleOpacity is true, specifies opacity of the subtitle box. Must be a decimal between 0 (transparent) and 1 (opaque).", 1f, null, null);
		}
	}
}
