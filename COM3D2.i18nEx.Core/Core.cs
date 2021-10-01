using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using COM3D2.i18nEx.Core.Hooks;
using COM3D2.i18nEx.Core.Loaders;
using COM3D2.i18nEx.Core.TranslationManagers;
using COM3D2.i18nEx.Core.Util;
using ExIni;
using I2.Loc;
using UnityEngine;

namespace COM3D2.i18nEx.Core
{
	public class Core : MonoBehaviour
	{
		public static ILogger Logger;


		public bool Initialized;


		internal static ITranslationLoader TranslationLoader;

		private int GameVersion
		{
			get
			{
				return (int)typeof(Misc).GetField("GAME_VERSION").GetValue(null);
			}
		}

		internal static string CurrentSelectedLanguage;

		public void Initialize(ILogger logger, string gameRoot)
		{
			if (this.GameVersion < 1320)
			{
				logger.LogWarning(string.Format("This version of i18nEx core supports only game versions {0} or newer. Detected game version: {1}", 1320, this.GameVersion));
				UnityEngine.Object.Destroy(this);
				return;
			}
			if (this.Initialized)
			{
				return;
			}
			Core.Logger = logger;
			Core.Logger.LogInfo("Initializing i18nEx...");
			Paths.Initialize(gameRoot);
			this.InitializeTranslationManagers();
			TranslationHooks.Initialize();
			Core.Logger.LogInfo("i18nEx initialized!");
			this.Initialized = true;
		}

		private T RegisterTranslationManager<T>() where T : TranslationManagerBase
		{
			T t = base.gameObject.AddComponent<T>();
			this.managers.Add(t);
			return t;
		}

		private void InitializeTranslationManagers()
		{
			Core.ScriptTranslate = this.RegisterTranslationManager<ScriptTranslationManager>();
			Core.TextureReplace = this.RegisterTranslationManager<TextureReplaceManager>();
			Core.I2Translation = this.RegisterTranslationManager<I2TranslationManager>();
			this.LoadLanguage(Configuration.General.ActiveLanguage.Value);
			Configuration.General.ActiveLanguage.ValueChanged += this.LoadLanguage;
		}

		private void LoadLanguage(string langName)
		{
			string text = Path.Combine(Paths.TranslationsRoot, langName);
			if (!Directory.Exists(text))
			{
				Core.Logger.LogWarning("No translations for language \"" + langName + "\" was found!");
				return;
			}
			ITranslationLoader translationLoader = Core.TranslationLoader;
			if (translationLoader != null)
			{
				translationLoader.UnloadCurrentTranslation();
			}
			IniFile iniFile = this.LoadLanguageConfig(text);
			ITranslationLoader translationLoader2;
			if (iniFile != null)
			{
				translationLoader2 = this.GetLoader(iniFile["Info"]["Loader"].Value);
			}
			else
			{
				ITranslationLoader translationLoader3 = new BasicTranslationLoader();
				translationLoader2 = translationLoader3;
			}
			Core.TranslationLoader = translationLoader2;
			Core.Logger.LogInfo(string.Format("Selecting language for {0}", Core.TranslationLoader));
			Core.TranslationLoader.SelectLanguage(langName, text, iniFile);
			foreach (TranslationManagerBase translationManagerBase in this.managers)
			{
				translationManagerBase.LoadLanguage();
			}
			Core.CurrentSelectedLanguage = langName;
			I2TranslationDump.Initialize();
		}

		private ITranslationLoader GetLoader(string loaderName)
		{
			if (string.IsNullOrEmpty(loaderName))
			{
				return new BasicTranslationLoader();
			}
			string text = Path.Combine(Paths.TranslationsRoot, "loaders");
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			loaderName = loaderName.Trim();
			if (loaderName == "BasicLoader")
			{
				return new BasicTranslationLoader();
			}
			string path = Path.Combine(text, loaderName + ".dll");
			if (!File.Exists(path))
			{
				return new BasicTranslationLoader();
			}
			ITranslationLoader result;
			try
			{
				Type type = Assembly.LoadFile(path).GetTypes().FirstOrDefault((Type t) => t.GetInterface("ITranslationLoader") != null);
				Core.Logger.LogInfo(string.Format("Invoking loader {0}", type));
				if (type != null)
				{
					result = (Activator.CreateInstance(type) as ITranslationLoader);
				}
				else
				{
					Core.Logger.LogWarning("Loader \"" + loaderName + ".dll\" doesn't contain any translation loader implementations!");
					result = new BasicTranslationLoader();
				}
			}
			catch (Exception ex)
			{
				Core.Logger.LogWarning("Failed to load translation loader \"" + loaderName + ".dll\". Reason: " + ex.Message);
				result = new BasicTranslationLoader();
			}
			return result;
		}

		private IniFile LoadLanguageConfig(string tlPath)
		{
			string text = Path.Combine(tlPath, "config.ini");
			if (!File.Exists(text))
			{
				return null;
			}
			IniFile result;
			try
			{
				result = IniFile.FromFile(text);
			}
			catch (Exception ex)
			{
				Core.Logger.LogWarning("Failed to read config.ini. Reason: " + ex.Message);
				result = null;
			}
			return result;
		}

		private void Awake()
		{
			UnityEngine.Object.DontDestroyOnLoad(this);
		}

		private void Update()
		{
			KeyCommandHandler.UpdateState();
			if (Configuration.General.ReloadConfigKey.Value.IsPressed)
			{
				Configuration.Reload();
			}
			if (Configuration.General.ReloadTranslationsKey.Value.IsPressed)
			{
				foreach (TranslationManagerBase translationManagerBase in this.managers)
				{
					translationManagerBase.ReloadActiveTranslations();
				}
			}
			if (Input.GetKey(KeyCode.Keypad0))
			{
				foreach (LanguageSource arg in LocalizationManager.Sources)
				{
					Core.Logger.LogInfo(string.Format("Got source {0}", arg));
				}
			}
		}

		public Core()
		{
		}

		private const int MIN_SUPPORTED_VERSION = 1320;

		internal static ScriptTranslationManager ScriptTranslate;

		internal static TextureReplaceManager TextureReplace;

		internal static I2TranslationManager I2Translation;

		private readonly List<TranslationManagerBase> managers = new List<TranslationManagerBase>();


	}
}
