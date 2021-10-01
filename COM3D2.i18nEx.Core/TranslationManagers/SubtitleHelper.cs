using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;

namespace COM3D2.i18nEx.Core.TranslationManagers
{
	internal static class SubtitleHelper
	{
		static SubtitleHelper()
		{
			if (SubtitleHelper.subDataType == null)
			{
				Core.Logger.LogError("BaseKagManager.SubtitleData class has not been found! Can't display subtitles! Check that you use the latest game version and latest i18nEx!");
				return;
			}
			SubtitleHelper.textField = AccessTools.Field(SubtitleHelper.subDataType, "text");
			SubtitleHelper.displayTimeField = AccessTools.Field(SubtitleHelper.subDataType, "displayTime");
			SubtitleHelper.addDisplayTimeField = AccessTools.Field(SubtitleHelper.subDataType, "addDisplayTime");
			SubtitleHelper.casinoTypeField = AccessTools.Field(SubtitleHelper.subDataType, "casinoType");
		}

		public static void SetSubtitleData(this SubtitleData subData, object kagSubtitleData)
		{
			if (SubtitleHelper.subDataType == null)
			{
				return;
			}
			if (Configuration.ScriptTranslations.RerouteTranslationsTo.Value == TranslationsReroute.Reverse)
			{
				FieldInfo fieldInfo = SubtitleHelper.textField;
				if (fieldInfo != null)
				{
					fieldInfo.SetValue(kagSubtitleData, subData.translation + "<E>" + subData.original);
				}
			}
			else
			{
				FieldInfo fieldInfo2 = SubtitleHelper.textField;
				if (fieldInfo2 != null)
				{
					fieldInfo2.SetValue(kagSubtitleData, subData.original + "<E>" + subData.translation);
				}
			}
			FieldInfo fieldInfo3 = SubtitleHelper.displayTimeField;
			if (fieldInfo3 != null)
			{
				fieldInfo3.SetValue(kagSubtitleData, subData.displayTime);
			}
			FieldInfo fieldInfo4 = SubtitleHelper.addDisplayTimeField;
			if (fieldInfo4 != null)
			{
				fieldInfo4.SetValue(kagSubtitleData, subData.addDisplayTime);
			}
			FieldInfo fieldInfo5 = SubtitleHelper.casinoTypeField;
			if (fieldInfo5 == null)
			{
				return;
			}
			fieldInfo5.SetValue(kagSubtitleData, subData.isCasino);
		}

		private static readonly FieldInfo textField;

		private static readonly FieldInfo displayTimeField;

		private static readonly FieldInfo addDisplayTimeField;

		private static readonly FieldInfo casinoTypeField;

		private static readonly Type subDataType = typeof(BaseKagManager).GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic).FirstOrDefault((Type t) => t.Name == "SubtitleData");

		public static bool yotogiGlobal;

		public static int currentLevel;
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

			internal bool <.cctor>b__7_0(Type t)
			{
				return t.Name == "SubtitleData";
			}

			public static readonly SubtitleHelper.<>c <>9 = new SubtitleHelper.<>c();
		}
		*/
	}
}
