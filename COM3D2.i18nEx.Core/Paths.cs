using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace COM3D2.i18nEx.Core
{
	internal static class Paths
	{
		public static string TranslationsRoot;


		public static string ConfigurationFilePath;

		public static void Initialize(string gameRoot)
		{
			Core.Logger.LogInfo("Initializing paths...");
			Paths.TranslationsRoot = Path.Combine(gameRoot, "i18nEx");
			Paths.ConfigurationFilePath = Path.Combine(Paths.TranslationsRoot, "configuration.ini");
			if (!Directory.Exists(Paths.TranslationsRoot))
			{
				Core.Logger.LogInfo("No root path found. Creating one in " + Paths.TranslationsRoot);
				Directory.CreateDirectory(Paths.TranslationsRoot);
			}
		}


	}
}
