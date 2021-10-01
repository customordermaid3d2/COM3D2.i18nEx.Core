using System;
using System.Collections.Generic;
using System.IO;
using BepInEx.Harmony;
using COM3D2.i18nEx.Core.TranslationManagers;
using COM3D2.i18nEx.Core.Util;
using HarmonyLib;
using I2.Loc;
using Scourt.Loc;

namespace COM3D2.i18nEx.Core.Hooks
{
	internal static class ScriptTranslationHooks
	{
		public static void Initialize()
		{
			if (ScriptTranslationHooks.initialized)
			{
				return;
			}
			ScriptTranslationHooks.instance = Harmony.CreateAndPatchAll(typeof(ScriptTranslationHooks), "horse.coder.com3d2.i18nex.hooks.scripts");
			ScriptTranslationHooks.initialized = true;
		}

		[HarmonyPatch(typeof(BaseKagManager), "TagPlayVoice")]
		[HarmonyPrefix]
		private static void OnPlayVoice(BaseKagManager __instance, KagTagSupport tag_data, object ___subtitle_data)
		{
			__instance.CheckAbsolutelyNecessaryTag(tag_data, "playvoice", new string[]
			{
				"voice"
			});
			string voiceName = tag_data.GetTagProperty("voice").AsString();
			List<SubtitleData> subtitle = Core.ScriptTranslate.GetSubtitle(Path.GetFileNameWithoutExtension(__instance.kag.GetCurrentFileName()), voiceName);
			if (subtitle == null)
			{
				return;
			}
			SubtitleMovieManager globalInstance = SubtitleMovieManager.GetGlobalInstance(false);
			globalInstance.Clear();
			if (subtitle.Count == 1 && subtitle[0].startTime == 0)
			{
				subtitle[0].SetSubtitleData(___subtitle_data);
				return;
			}
			globalInstance.autoDestroy = true;
			foreach (SubtitleData subtitleData in subtitle)
			{
				if (Configuration.ScriptTranslations.RerouteTranslationsTo.Value == TranslationsReroute.Reverse)
				{
					globalInstance.AddData(subtitleData.translation + "<E>" + subtitleData.original, subtitleData.startTime, subtitleData.displayTime);
				}
				else
				{
					globalInstance.AddData(subtitleData.original + "<E>" + subtitleData.translation, subtitleData.startTime, subtitleData.displayTime);
				}
			}
			globalInstance.Play();
		}

		[HarmonyPatch(typeof(YotogiKagManager), "TagTalk")]
		[HarmonyPatch(typeof(YotogiKagManager), "TagTalkAddFt")]
		[HarmonyPatch(typeof(YotogiKagManager), "TagTalkRepeat")]
		[HarmonyPatch(typeof(YotogiKagManager), "TagTalkRepeatAdd")]
		[HarmonyPrefix]
		private static void YotogiTalk(BaseKagManager __instance, KagTagSupport tag_data)
		{
			if (tag_data.IsValid("voice"))
			{
				ScriptTranslationHooks.lastTagedVoice = tag_data.GetTagProperty("voice").AsString() + ".ogg";
			}
		}

		[HarmonyPatch(typeof(AudioSourceMgr), "Play")]
		[HarmonyPrefix]
		private static void OnPlaySound(AudioSourceMgr __instance)
		{
			if (__instance.SoundType < AudioSourceMgr.Type.Voice)
			{
				return;
			}
			ScriptTranslationHooks.lastPlayedVoice = __instance.FileName;
			string text;
			if (ScriptTranslationHooks.voiceDict.TryGetValue(ScriptTranslationHooks.lastPlayedVoice, out text))
			{
				ScriptTranslationHooks.UpdateGlobalSubtitle(text);
				return;
			}
			SubtitleMovieManager globalInstance = SubtitleMovieManager.GetGlobalInstance(false);
			if (globalInstance == null)
			{
				return;
			}
			globalInstance.Clear();
		}

		[HarmonyPatch(typeof(YotogiKagManager), "TagHitRet")]
		[HarmonyPrefix]
		private static void YotogiHitRet(BaseKagManager __instance)
		{
			if (ScriptTranslationHooks.lastTagedVoice == null || ScriptTranslationHooks.voiceDict.ContainsKey(ScriptTranslationHooks.lastTagedVoice))
			{
				return;
			}
			string text = __instance.kag.GetText();
			if (!text.IsNullOrWhiteSpace())
			{
				ScriptTranslationHooks.voiceDict.Add(ScriptTranslationHooks.lastTagedVoice, text);
				if (ScriptTranslationHooks.lastPlayedVoice != null && ScriptTranslationHooks.lastPlayedVoice == ScriptTranslationHooks.lastTagedVoice)
				{
					ScriptTranslationHooks.UpdateGlobalSubtitle(text);
				}
			}
			ScriptTranslationHooks.lastTagedVoice = null;
		}

		private static void UpdateGlobalSubtitle(string text)
		{
			if (SubtitleHelper.currentLevel != 63)
			{
				return;
			}
			if (!GameMain.Instance.CMSystem.YotogiSubtitleVisible)
			{
				return;
			}
			SubtitleHelper.yotogiGlobal = true;
			SubtitleMovieManager globalInstance = SubtitleMovieManager.GetGlobalInstance(false);
			globalInstance.Clear();
			globalInstance.autoDestroy = true;
			globalInstance.AddData(text, 0, 60000);
			globalInstance.Play();
		}

		[HarmonyPatch(typeof(BaseKagManager), "TagVRChoicesSet")]
		[HarmonyPatch(typeof(BaseKagManager), "TagVRDialog")]
		[HarmonyPatch(typeof(ADVKagManager), "TagChoicesRandomSet")]
		[HarmonyPatch(typeof(ADVKagManager), "TagChoicesSet")]
		[HarmonyPatch(typeof(ADVKagManager), "TagTalk")]
		[HarmonyPrefix]
		private static void LogScriptName(BaseKagManager __instance)
		{
			ScriptTranslationHooks.curScriptFileName = __instance.kag.GetCurrentFileName();
		}

		[HarmonyPatch(typeof(BaseKagManager), "TagVRChoicesSet")]
		[HarmonyPatch(typeof(BaseKagManager), "TagVRDialog")]
		[HarmonyPatch(typeof(ADVKagManager), "TagChoicesRandomSet")]
		[HarmonyPatch(typeof(ADVKagManager), "TagChoicesSet")]
		[HarmonyPatch(typeof(ADVKagManager), "TagTalk")]
		[HarmonyPostfix]
		private static void ClearScriptName()
		{
			ScriptTranslationHooks.curScriptFileName = null;
		}

		[HarmonyPatch(typeof(ScriptManager), "ReplaceCharaName", new Type[]
		{
			typeof(string)
		})]
		[HarmonyPrefix]
		private static void ReplaceCharaName(ref string text)
		{
			if (string.IsNullOrEmpty(ScriptTranslationHooks.curScriptFileName))
			{
				return;
			}
			ScriptTranslationHooks.TranslateLine(ScriptTranslationHooks.curScriptFileName, ref text, false);
		}

		[HarmonyPatch(typeof(Scourt.Loc.LocalizationManager), "GetTranslationText", typeof(string))]
		//[HarmonyPatch(typeof(MessageClass), "GetTranslationText")]
		[HarmonyPostfix]
		//private static void OnGetTranslationText(ref KeyValuePair<string, string> __result)
			private static void OnGetTranslationText(ref LocalizationString __result)
		{
			/*
			if (!string.IsNullOrEmpty(__result.Key) && string.IsNullOrEmpty(__result.Value))
			{
				string translation;
				if (!LocalizationManager.TryGetTranslation("SubMaid/" + __result.Key + "/名前", out translation, true, 0, true, false, null, null))
				{
					bool flag;
					translation = Core.ScriptTranslate.GetTranslation(null, __result.Key, out flag);
				}
				__result = new KeyValuePair<string, string>(__result.Key, translation);
			}
			*/

			try
			{
				if (!__result.IsEmpty(Product.baseScenarioLanguage))
				{
					if (!I2.Loc.LocalizationManager.TryGetTranslation($"SubMaid/{Product.baseScenarioLanguage}/名前", out var tl))
                    {
						bool flag;
						tl = Core.ScriptTranslate.GetTranslation(null, Product.baseScenarioLanguage.ToString(), out flag);
                    }
					__result[Product.baseScenarioLanguage] = tl;
				}
			}
			catch (System.Exception e)
			{
				Core.Logger.LogWarning("OnGetTranslationText : " + Product.baseScenarioLanguage);
				Core.Logger.LogWarning("OnGetTranslationText : " + e.ToString());
			}
		}

		[HarmonyPatch(typeof(KagScript), "GetText")]
		[HarmonyPostfix]
		private static void KagScriptGetText(KagScript __instance, ref string __result)
		{
			if (string.IsNullOrEmpty(__result))
			{
				return;
			}
			ScriptTranslationHooks.TranslateLine(__instance.GetCurrentFileName(), ref __result, false);
		}

		private static bool TranslateLine(string fileName, ref string text, bool stop = false)
		{
			KeyValuePair<string, string> keyValuePair = text.SplitTranslation();
			ScriptTranslationHooks.ProcessTranslation(fileName, ref keyValuePair);
			TranslationsReroute value = Configuration.ScriptTranslations.RerouteTranslationsTo.Value;
			if (!string.IsNullOrEmpty(keyValuePair.Value))
			{
				if (value == TranslationsReroute.Reverse)
				{
					text = keyValuePair.Value + "<E>" + keyValuePair.Key;
				}
				else if (value == TranslationsReroute.RouteToJapanese)
				{
					text = keyValuePair.Value + "<E>" + keyValuePair.Value;
				}
				else
				{
					text = keyValuePair.Key + "<E>" + keyValuePair.Value;
				}
				return true;
			}
			if (value == TranslationsReroute.Reverse || value == TranslationsReroute.RouteToEnglish)
			{
				text = keyValuePair.Key + "<E>" + keyValuePair.Key;
				return true;
			}
			return false;
		}

		private static void ProcessTranslation(string fileName, ref KeyValuePair<string, string> translationPair)
		{
			if (string.IsNullOrEmpty(translationPair.Key))
			{
				if (Configuration.ScriptTranslations.VerboseLogging.Value)
				{
					Core.Logger.LogInfo(string.Concat(new string[]
					{
						"[Script] [",
						fileName,
						"] \"",
						translationPair.Key,
						"\" => \"",
						translationPair.Value,
						"\""
					}));
				}
				return;
			}
			if (fileName == null)
			{
				if (Configuration.ScriptTranslations.VerboseLogging.Value)
				{
					Core.Logger.LogWarning("Found script with no name! Skipping...");
				}
				return;
			}
			fileName = Path.GetFileNameWithoutExtension(fileName);
			bool flag;
			string translation = Core.ScriptTranslate.GetTranslation(fileName, translationPair.Key, out flag);
			if (!string.IsNullOrEmpty(translation))
			{
				translationPair = new KeyValuePair<string, string>(translationPair.Key, translation);
				if (Configuration.ScriptTranslations.VerboseLogging.Value)
				{
					Core.Logger.LogInfo(string.Concat(new string[]
					{
						"[Script] [",
						fileName,
						"] \"",
						translationPair.Key,
						"\" => \"",
						translation,
						"\""
					}));
				}
			}
			else if (Configuration.ScriptTranslations.DumpScriptTranslations.Value && Core.ScriptTranslate.WriteTranslation(fileName, translationPair.Key, translationPair.Value))
			{
				Core.Logger.LogInfo(string.Concat(new string[]
				{
					"[DUMP] [",
					fileName,
					"] \"",
					translationPair.Key,
					"\" => \"",
					translation,
					"\""
				}));
			}
			if (flag)
			{
				SubtitleDisplayManager.DisplayType subtitleType = GameMain.Instance.CMSystem.SubtitleType;
				if (subtitleType == SubtitleDisplayManager.DisplayType.Original)
				{
					translationPair = new KeyValuePair<string, string>(translation, translationPair.Key);
					return;
				}
				if (subtitleType == SubtitleDisplayManager.DisplayType.OriginalAndSubtitle)
				{
					translationPair = new KeyValuePair<string, string>(translation + "(" + translationPair.Key + ")", translationPair.Key);
					return;
				}
			}
			else
			{
				translationPair = new KeyValuePair<string, string>(translationPair.Key, translation);
			}
		}

		// Note: this type is marked as 'beforefieldinit'.
		static ScriptTranslationHooks()
		{
		}

		private static Harmony instance;

		private static string curScriptFileName;

		private static bool initialized;

		private static string lastTagedVoice;

		private static string lastPlayedVoice;

		internal static Dictionary<string, string> voiceDict = new Dictionary<string, string>();
	}
}
