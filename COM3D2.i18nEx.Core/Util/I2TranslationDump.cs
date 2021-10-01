using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BepInEx.Harmony;
using HarmonyLib;
using I2.Loc;

namespace COM3D2.i18nEx.Core.Util
{
	public static class I2TranslationDump
	{
		public static void Initialize()
		{
			if (!Configuration.I2Translation.DumpTexts.Value)
			{
				return;
			}
			if (!I2TranslationDump.initialized)
			{
				Harmony.CreateAndPatchAll(typeof(I2TranslationDump), null);
			}
			I2TranslationDump.initialized = true;
			I2TranslationDump.DumpedTerms.Clear();
			I2TranslationDump.extractPath = Path.Combine(Path.Combine(Path.Combine(Paths.TranslationsRoot, Core.CurrentSelectedLanguage), "UI_Dump"), DateTime.Now.ToString("yyyy_MM_dd__HHmmss"));
			Core.Logger.LogInfo("[I2Loc] creating UI dumps to " + I2TranslationDump.extractPath);
			Directory.CreateDirectory(I2TranslationDump.extractPath);
		}

		private static bool SplitTerm(string term, out string mainCategory, out string rest)
		{
			int num = term.IndexOf('/');
			if (num == -1)
			{
				string text;
				rest = (text = null);
				mainCategory = text;
				return false;
			}
			mainCategory = term.Substring(0, num);
			rest = term.Substring(num + 1);
			return true;
		}

		private static string EscapeCsv(this string str, char delimiter = ',')
		{
			if (str.Contains("\n") || str.Contains(delimiter.ToString()))
			{
				return "\"" + str.Replace("\"", "\"\"") + "\"";
			}
			return str;
		}

		[HarmonyPatch(typeof(LocalizationManager), "TryGetTranslation")]
		[HarmonyPostfix]
		public static void PostTryGetTranslation(ref bool __result, string Term)
		{
			if (__result || I2TranslationDump.DumpedTerms.Contains(Term))
			{
				return;
			}
			string str;
			string str2;
			if (!I2TranslationDump.SplitTerm(Term, out str, out str2))
			{
				return;
			}
			string path = Path.Combine(I2TranslationDump.extractPath, str + ".csv");
			if (!File.Exists(path))
			{
				File.WriteAllText(path, "Key,Type,Desc,Japanese,English\n", I2TranslationDump.Utf8);
			}
			File.AppendAllText(path, str2.EscapeCsv(',') + ",Text,,,\n", I2TranslationDump.Utf8);
			I2TranslationDump.DumpedTerms.Add(Term);
		}

		// Note: this type is marked as 'beforefieldinit'.
		static I2TranslationDump()
		{
		}

		private static readonly HashSet<string> DumpedTerms = new HashSet<string>();

		private static string extractPath;

		private static readonly Encoding Utf8 = new UTF8Encoding(true);

		private static bool initialized = false;
	}
}
