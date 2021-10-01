using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using BepInEx.Harmony;
using COM3D2.i18nEx.Core.Util;
using HarmonyLib;
using I2.Loc;
using UnityEngine;

namespace COM3D2.i18nEx.Core.Hooks
{
	internal static class TranslationHooks
	{
		public static void Initialize()
		{
			if (TranslationHooks.initialized)
			{
				return;
			}
			ScriptTranslationHooks.Initialize();
			TextureReplaceHooks.Initialize();
			UIFixes.Initialize();
			TranslationHooks.instance = Harmony.CreateAndPatchAll(typeof(TranslationHooks), "horse.coder.i18nex.hooks.base");
			TranslationHooks.initialized = true;
		}

		[HarmonyPatch(typeof(Product), "supportMultiLanguage", MethodType.Getter)]
		[HarmonyPostfix]
		private static void SupportMultiLanguage(ref bool __result)
		{
			__result = true;
		}

		[HarmonyPatch(typeof(Product), "isJapan", MethodType.Getter)]
		[HarmonyPostfix]
		private static void IsJapan(ref bool __result)
		{
			__result = false;
		}

		[HarmonyPatch(typeof(SceneNetorareCheck), "Start")]
		[HarmonyTranspiler]
		private static IEnumerable<CodeInstruction> FixNTRCheckScene(IEnumerable<CodeInstruction> instructions)
		{
			foreach (CodeInstruction codeInstruction in instructions)
			{
				if (codeInstruction.opcode == OpCodes.Call)
				{
					MethodInfo methodInfo = codeInstruction.operand as MethodInfo;
					if (methodInfo != null && methodInfo.Name == "get_isJapan")
					{
						yield return new CodeInstruction(OpCodes.Ldc_I4_1, null);
						continue;
					}
				}
				yield return codeInstruction;
			}
			//IEnumerator<CodeInstruction> enumerator = null;
			//yield break;
			yield break;
		}

		[HarmonyPatch(typeof(SubtitleDisplayManager), "messageBgAlpha", MethodType.Setter)]
		[HarmonyPrefix]
		private static bool OnGetConfigMessageAlpha(SubtitleDisplayManager __instance, ref float value)
		{
			Transform parent = __instance.transform.parent;
			if (Configuration.I2Translation.OverrideSubtitleOpacity.Value && parent && parent.name == "YotogiPlayPanel")
			{
				if ((double)Math.Abs(value - __instance.messageBgAlpha) < 0.001)
				{
					return false;
				}
				value = Mathf.Clamp(Configuration.I2Translation.SubtitleOpacity.Value, 0f, 1f);
			}
			return true;
		}

		[HarmonyPatch(typeof(LocalizationManager), "GetTranslation")]
		[HarmonyPostfix]
		private static void OnGetTranslation(ref string __result, string Term, bool FixForRTL, int maxLineLengthForRTL, bool ignoreRTLnumbers, bool applyParameters, GameObject localParametersRoot, string overrideLanguage)
		{
			if (overrideLanguage != "Japanese" && (__result.IsNullOrWhiteSpace() || (__result.IndexOf('/') >= 0 && Term.Contains(__result))))
			{
				__result = LocalizationManager.GetTranslation(Term, FixForRTL, maxLineLengthForRTL, ignoreRTLnumbers, applyParameters, localParametersRoot, "Japanese");
				return;
			}
			if (__result.IsNullOrWhiteSpace() && !Term.Equals("NotoSansCJKjp-DemiLight") && !Term.IsNullOrWhiteSpace())
			{
				if (Configuration.I2Translation.VerboseLogging.Value)
				{
					Core.Logger.LogInfo(string.Concat(new string[]
					{
						"[I2Loc] Translating term \"",
						Term,
						"\" => \"",
						__result,
						"\""
					}));
				}
				string text = "";
				if (overrideLanguage != "English" && !Term.Contains("/") && !TranslationHooks.passPattern.IsMatch(Term))
				{
					text = LocalizationManager.GetTranslation("General/" + Term, FixForRTL, maxLineLengthForRTL, ignoreRTLnumbers, applyParameters, localParametersRoot, "English");
				}
				if (!text.IsNullOrWhiteSpace())
				{
					__result = text;
				}
			}
		}

		[HarmonyPatch(typeof(ConfigMgr), "Update")]
		[HarmonyPrefix]
		private static bool OnConfigMgrUpdate()
		{
			return false;
		}

		// Note: this type is marked as 'beforefieldinit'.
		static TranslationHooks()
		{
		}

		private static bool initialized;

		private static Harmony instance;

		private static Regex passPattern = new Regex("^[-+,.a-zA-Z_0-9]+$", RegexOptions.Compiled);

	}
}
