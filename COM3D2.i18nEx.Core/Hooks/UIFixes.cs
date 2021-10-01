using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using BepInEx.Harmony;
using COM3D2.i18nEx.Core.TranslationManagers;
using HarmonyLib;
using I2.Loc;
using MaidStatus;
using UnityEngine;
using UnityEngine.UI;
using wf;
using Yotogis;

namespace COM3D2.i18nEx.Core.Hooks
{
    internal static class UIFixes
    {
        public static void Initialize()
        {
            if (UIFixes.initialized)
            {
                return;
            }
            UIFixes.instance = Harmony.CreateAndPatchAll(typeof(UIFixes), "horse.coder.i18nex.ui_fixes");
            UIFixes.initialized = true;
        }

        [HarmonyPatch(typeof(CMSystem), "LoadIni")]
        [HarmonyPostfix]
        public static void PostLoadIni()
        {
            if (Configuration.General.FixSubtitleType.Value)
            {
                Configuration.ScriptTranslations.RerouteTranslationsTo.Value = TranslationsReroute.Reverse;
                Configuration.General.FixSubtitleType.Value = false;
                GameMain.Instance.CMSystem.SystemLanguage = Product.Language.English;
                GameMain.Instance.CMSystem.SubtitleType = SubtitleDisplayManager.DisplayType.Original;
                GameMain.Instance.CMSystem.SaveIni();
                Core.Logger.LogInfo("Fixed game's subtitle type!");
            }
        }

        [HarmonyPatch(typeof(Status), "maxNameLength", MethodType.Getter)]
        [HarmonyPostfix]
        public static void GetMaxNameLength(ref int __result)
        {
            __result = int.MaxValue;
        }

        [HarmonyPatch(typeof(Text), "text", MethodType.Setter)]
        [HarmonyPrefix]
        public static void OnSetText(Text __instance, string value)
        {
            UIFixes.SetLoc(__instance.gameObject, value);
        }

        [HarmonyPatch(typeof(UILabel), "ProcessAndRequest")]
        [HarmonyPrefix]
        public static void OnProcessRequest(UILabel __instance)
        {
            UIFixes.SetLoc(__instance.gameObject, __instance.text);
        }

        [CompilerGenerated]
        internal sealed class PrivateImplementationDetails
        {
            internal static uint ComputeStringHash(string s)
            {
                uint num=0;
                if (s != null)
                {
                    num = 2166136261U;
                    for (int i = 0; i < s.Length; i++)
                    {
                        num = ((uint)s[i] ^ num) * 16777619U;
                    }
                }
                return num;
            }

            //	internal static readonly PrivateImplementationDetails.__StaticArrayInitTypeSize1024 12F3E0576D447EB37B36D82BA0C1C5481B8F0D12FDC70347CE4A076B229D4C86;
            //
            //[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 1024)]
            //	private struct __StaticArrayInitTypeSize1024
            //{
            //}
        }

        private static void SetLoc(GameObject go, string text)
        {
            Localize localize = go.GetComponent<Localize>();
            if (localize != null)
            {
                if (string.IsNullOrEmpty(text))
                {
                    if (localize.Term.Equals("-") || string.IsNullOrEmpty(localize.Term))
                    {
                        return;
                    }
                    if (localize.Term.Contains("/"))
                    {
                        if (go.name.Equals("Name") && go.transform.parent.name.Equals("Title"))
                        {
                            localize.SetTerm("YotogiSkillName/" + localize.Term.Trim());
                        }
                        else
                        {
                            localize.SetTerm("General/" + localize.Term.Trim());
                        }
                    }
                }
                else if (localize.Term.Equals("SceneDaily/ボタン文字/カラオケ"))
                {
                    if (text.Equals("プライベートモード設定"))
                    {
                        localize.SetTerm("SceneDaily/ボタン文字/プライベートモード設定");
                        go.GetComponent<UILabel>().fontSize = 20;
                    }
                    else if (text.Equals("スカウト"))
                    {
                        localize.SetTerm("SceneDaily/ボタン文字/スカウト");
                        go.GetComponent<UILabel>().fontSize = 22;
                    }
                }
                else if (!localize.Term.Contains("/"))
                {
                    localize.SetTerm("General/" + text.Trim().Replace(' ', '_').Replace("\n", ""));
                }
                else if (localize.Term.StartsWith("General/") && !UIFixes.korPattern.IsMatch(text))
                {
                    localize.SetTerm("General/" + text.Trim().Replace(' ', '_').Replace("\n", ""));
                }
                if (Configuration.I2Translation.VerboseLogging.Value)
                {
                    Core.Logger.LogInfo("Trying to localize with " + localize.Term + " = " + text);
                    return;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(text))
                {
                    return;
                }
                if (UIFixes.passPattern.IsMatch(text))
                {
                    return;
                }
                string name = go.name;
                if (name != null)
                {
                    uint num = PrivateImplementationDetails.ComputeStringHash(name);
                    if (num <= 1701962553U)
                    {
                        if (num != 388258996U)
                        {
                            if (num != 1619518561U)
                            {
                                if (num != 1701962553U)
                                {
                                    goto IL_2D2;
                                }
                                if (!(name == "SubTitle"))
                                {
                                    goto IL_2D2;
                                }
                            }
                            else if (!(name == "singleTextText"))
                            {
                                goto IL_2D2;
                            }
                        }
                        else if (!(name == "Option"))
                        {
                            goto IL_2D2;
                        }
                    }
                    else if (num <= 2910044175U)
                    {
                        if (num != 2734585490U)
                        {
                            if (num != 2910044175U)
                            {
                                goto IL_2D2;
                            }
                            if (!(name == "charaName"))
                            {
                                goto IL_2D2;
                            }
                        }
                        else if (!(name == "Original"))
                        {
                            goto IL_2D2;
                        }
                    }
                    else if (num != 3189239814U)
                    {
                        if (num != 3595614522U)
                        {
                            goto IL_2D2;
                        }
                        if (!(name == "originalTextText"))
                        {
                            goto IL_2D2;
                        }
                    }
                    else if (!(name == "subtitlesTextText"))
                    {
                        goto IL_2D2;
                    }
                    return;
                }
                IL_2D2:
                localize = go.AddComponent<Localize>();
                name = go.transform.parent.name;
                if (name != null)
                {
                    if (name == "SkillName" || name == "UnitParent" || name == "Description")
                    {
                        localize.SetTerm("YotogiSkillName/" + text);
                        goto IL_41D;
                    }
                    if (name == "SelectButton(Clone)" || name == "RandomButton(Clone)")
                    {
                        string text2;
                        if (LocalizationManager.TryGetTranslation("ScenePrivate/" + text, out text2, true, 0, true, false, null, null))
                        {
                            go.GetComponent<Text>().text = text2;
                        }
                        localize.SetTerm("ScenePrivate/" + text);
                        goto IL_41D;
                    }
                    if (name == "Drop-down List")
                    {
                        goto IL_41D;
                    }
                }
                if (text.EndsWith("を入手"))
                {
                    localize.SetTerm("SceneCompetitiveShow/" + text);
                }
                else if (text.EndsWith("の状態が酔いになりました"))
                {
                    localize.SetTerm("Dialog/[HF]の状態が酔いになりました");
                }
                else if (text.Contains("再雇用シナリオ"))
                {
                    localize.SetTerm("General/解雇_Error");
                }
                else
                {
                    localize.SetTerm("General/" + text.Trim().Replace(" ", "_").Replace("\n", ""));
                }
                IL_41D:
                if (Configuration.I2Translation.VerboseLogging.Value)
                {
                    Core.Logger.LogInfo("Trying to localize with text " + text);
                }
            }
        }

        [HarmonyPatch(typeof(Text), "OnEnable")]
        [HarmonyPrefix]
        public static void ChangeUEUIFont(Text __instance)
        {
            __instance.font = UIFixes.SwapFont(__instance.font);
        }

        [HarmonyPatch(typeof(UILabel), "ProcessAndRequest")]
        [HarmonyPrefix]
        public static void ChangeFont(UILabel __instance)
        {
            __instance.trueTypeFont = UIFixes.SwapFont(__instance.trueTypeFont);
        }

        private static Font SwapFont(Font originalFont)
        {
            if (originalFont == null)
            {
                return null;
            }
            string text = Configuration.I2Translation.CustomUIFont.Value.Trim();
            if (string.IsNullOrEmpty(text) || originalFont.name == text)
            {
                return originalFont;
            }
            string key = string.Format("{0}#{1}", text, originalFont.fontSize);
            Font font;
            if (!UIFixes.customFonts.TryGetValue(key, out font))
            {
                font = (UIFixes.customFonts[key] = Font.CreateDynamicFontFromOSFont(text, originalFont.fontSize));
            }
            return font ?? originalFont;
        }


        //		[CompilerGenerated]
        //internal static void <LocalizeNTRScene>g__Localize|14_0(string item, ref UIFixes.<>c__DisplayClass14_0 A_1)
        //{
        //	GameObject childObject = UTY.GetChildObject(A_1.___toggleParent, item + "/Result", false);
        //	GameObject childObject2 = UTY.GetChildObject(A_1.___toggleParent, item + "/Title", false);
        //	childObject.AddComponent<Localize>().SetTerm("SceneNetorareCheck/" + item + "_Result");
        //	childObject2.AddComponent<Localize>().SetTerm("SceneNetorareCheck/" + item + "_Title");
        //}

        [HarmonyPatch(typeof(SceneNetorareCheck), "Start")]
        [HarmonyPostfix]
        public static void LocalizeNTRScene(GameObject ___toggleParent)
        {
            //UIFixes.<>c__DisplayClass14_0 CS$<>8__locals1;
            //CS$<>8__locals1.___toggleParent = ___toggleParent;
            //Core.Logger.LogInfo("Fixing NTR check scene.");
            //UIFixes.<LocalizeNTRScene>g__Localize|14_0("Toggle_LockUserDraftMaid", ref CS$<>8__locals1);
            //UIFixes.<LocalizeNTRScene>g__Localize|14_0("Toggle_IsComPlayer", ref CS$<>8__locals1);

            Core.Logger.LogInfo("Fixing NTR check scene.");

            void Localize(string item)
            {
                var result = UTY.GetChildObject(___toggleParent, $"{item}/Result"); //.GetComponent<UILabel>();
                var title = UTY.GetChildObject(___toggleParent, $"{item}/Title");   //.GetComponent<UILabel>();

                var resultLoc = result.AddComponent<Localize>();
                resultLoc.SetTerm($"SceneNetorareCheck/{item}_Result");

                var titleLoc = title.AddComponent<Localize>();
                titleLoc.SetTerm($"SceneNetorareCheck/{item}_Title");
            }

            Localize("Toggle_LockUserDraftMaid");
            Localize("Toggle_IsComPlayer");
        }



        [HarmonyPatch(typeof(SystemShortcut), "OnClick_Info")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> LocalizeInfoText(IEnumerable<CodeInstruction> instructions)
        {
            bool hasText = false;
            foreach (CodeInstruction codeInstruction in instructions)
            {
                if (codeInstruction.opcode == OpCodes.Callvirt)
                {
                    MethodInfo methodInfo = codeInstruction.operand as MethodInfo;
                    if (methodInfo != null && methodInfo.Name == "get_SysDlg")
                    {
                        hasText = true;
                        goto IL_2C3;
                    }
                }
                if (hasText)
                {
                    hasText = false;
                    int index = -1;
                    if (OpCodes.Ldloc_0.Value <= codeInstruction.opcode.Value && codeInstruction.opcode.Value <= OpCodes.Ldloc_3.Value)
                    {
                        index = (int)(codeInstruction.opcode.Value - OpCodes.Ldloc_0.Value);
                    }
                    else if (codeInstruction.opcode == OpCodes.Ldloc_S || codeInstruction.opcode == OpCodes.Ldloc)
                    {
                        index = (int)codeInstruction.operand;
                    }
                    if (index < 0)
                    {
                        Core.Logger.LogError("Failed to patch info text localization! Please report this!");
                        yield return codeInstruction;
                        continue;
                    }
                    yield return new CodeInstruction(OpCodes.Pop, null);
                    yield return new CodeInstruction(OpCodes.Ldloca, index);
                    yield return Transpilers.EmitDelegate<UIFixes.TranslateInfo>(delegate (ref string text)
                    {
                        string format;
                        if (!string.IsNullOrEmpty(GameMain.Instance.CMSystem.CM3D2Path) && LocalizationManager.TryGetTranslation("System/GameInfo_Description_Legacy", out format, true, 0, true, false, null, null))
                        {
                            text = string.Format(format, GameUty.GetGameVersionText(), GameUty.GetLegacyGameVersionText());
                            return;
                        }
                        string format2;
                        if (string.IsNullOrEmpty(GameMain.Instance.CMSystem.CM3D2Path) && LocalizationManager.TryGetTranslation("System/GameInfo_Description", out format2, true, 0, true, false, null, null))
                        {
                            text = string.Format(format2, GameUty.GetGameVersionText(), GameUty.GetBuildVersionText());
                        }
                    });
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(GameMain), "Instance"));
                    yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(GameMain), "SysDlg"));
                }
                IL_2C3:
                yield return codeInstruction;
                //codeInstruction = null;
            }
            //IEnumerator<CodeInstruction> enumerator = null;
            //yield break;
            yield break;
        }

        [HarmonyPatch(typeof(LocalizeTarget_NGUI_Label), "DoLocalize")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> FixDoLocalize(IEnumerable<CodeInstruction> instrs)
        {
            MethodInfo prop = AccessTools.PropertySetter(typeof(UILabel), "text");
            foreach (CodeInstruction codeInstruction in instrs)
            {
                if (codeInstruction.opcode == OpCodes.Callvirt && (MethodInfo)codeInstruction.operand == prop)
                {
                    yield return Transpilers.EmitDelegate<Action<UILabel, string>>(delegate (UILabel label, string text)
                    {
                        if (!string.IsNullOrEmpty(text))
                        {
                            label.text = text;
                        }
                    });
                }
                else
                {
                    yield return codeInstruction;
                }
            }
            //IEnumerator<CodeInstruction> enumerator = null;
            //yield break;
            yield break;
        }

        [HarmonyPatch(typeof(UIWFConditionList), "SetTexts", new Type[]
        {
            typeof(KeyValuePair<string[], Color>[]),
            typeof(int)
        })]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> FixSetTexts(IEnumerable<CodeInstruction> instrs)
        {
            MethodInfo setActive = AccessTools.Method(typeof(GameObject), "SetActive", null, null);
            bool gotBool = false;
            bool done = false;
            foreach (CodeInstruction ins in instrs)
            {
                yield return ins;
                if (!done && ins.opcode == OpCodes.Ldc_I4_1)
                {
                    gotBool = true;
                }
                if (gotBool && !(ins.opcode != OpCodes.Callvirt) && (MethodInfo)ins.operand == setActive)
                {
                    gotBool = false;
                    done = true;
                    yield return new CodeInstruction(OpCodes.Ldarg_0, null);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(UIWFConditionList), "condition_label_list_"));
                    yield return new CodeInstruction(OpCodes.Ldarg_1, null);
                    yield return new CodeInstruction(OpCodes.Ldloc_3, null);
                    yield return Transpilers.EmitDelegate<Action<List<UILabel>, KeyValuePair<string[], Color>[], int>>(delegate (List<UILabel> labels, KeyValuePair<string[], Color>[] texts, int index)
                    {
                        KeyValuePair<string[], Color> keyValuePair = texts[index];
                        UILabel uilabel = labels[index];
                        if (keyValuePair.Key.Length == 1)
                        {
                            uilabel.text = Utility.GetTermLastWord(keyValuePair.Key[0]);
                            return;
                        }
                        if (keyValuePair.Key.Length > 1)
                        {
                            uilabel.text = string.Format(Utility.GetTermLastWord(keyValuePair.Key[0]), keyValuePair.Key.Skip(1).Select(new Func<string, string>(Utility.GetTermLastWord)).Cast<object>().ToArray<object>());
                        }
                    });
                    //ins = null;
                }
            }
            //IEnumerator<CodeInstruction> enumerator = null;
            //yield break;
            yield break;
        }

        [HarmonyPatch(typeof(SkillAcquisitionCondition), "CreateConditionTextAndStaturResults")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> TranspileCreateConditionTextAndStaturResults(IEnumerable<CodeInstruction> instrs)
        {
            MethodInfo supportMultiLang = AccessTools.PropertyGetter(typeof(Product), "supportMultiLanguage");
            foreach (CodeInstruction codeInstruction in instrs)
            {
                if (codeInstruction.opcode == OpCodes.Call && (MethodInfo)codeInstruction.operand == supportMultiLang)
                {
                    yield return codeInstruction;
                    yield return new CodeInstruction(OpCodes.Pop, null);
                    yield return new CodeInstruction(OpCodes.Ldarg_0, null);
                    yield return Transpilers.EmitDelegate<Func<SkillAcquisitionCondition, bool>>(delegate (SkillAcquisitionCondition sac)
                    {
                        string text;
                        return Product.supportMultiLanguage && LocalizationManager.TryGetTranslation(sac.yotogi_class.termName, out text, true, 0, true, false, null, null);
                    });
                }
                else
                {
                    yield return codeInstruction;
                }
            }
            // IEnumerator<CodeInstruction> enumerator = null;
            // yield break;
            yield break;
        }

        [HarmonyPatch(typeof(MaidManagementMain), "OnSelectChara")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> FixNpcMaidBanishment(IEnumerable<CodeInstruction> instrs)
        {
            MethodInfo isJapan = AccessTools.PropertyGetter(typeof(Product), "isJapan");
            foreach (CodeInstruction codeInstruction in instrs)
            {
                if (codeInstruction.opcode == OpCodes.Call && (MethodInfo)codeInstruction.operand == isJapan)
                {
                    yield return new CodeInstruction(OpCodes.Ldc_I4_1, null);
                }
                else
                {
                    yield return codeInstruction;
                }
            }
            //IEnumerator<CodeInstruction> enumerator = null;
            //yield break;
            yield break;
        }

        [HarmonyPatch(typeof(ProfileCtrl), "Init")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> FixProfileCtrlPersonalityDisplay(IEnumerable<CodeInstruction> instrs)
        {
            MethodInfo termNameProp = AccessTools.PropertyGetter(typeof(Personal.Data), "termName");
            foreach (CodeInstruction codeInstruction in instrs)
            {
                if (codeInstruction.opcode == OpCodes.Callvirt && (MethodInfo)codeInstruction.operand == termNameProp)
                {
                    yield return Transpilers.EmitDelegate<Func<Personal.Data, string>>(delegate (Personal.Data data)
                    {
                        string text;
                        if (LocalizationManager.TryGetTranslation(data.termName, out text, true, 0, true, false, null, null))
                        {
                            return data.termName;
                        }
                        return data.drawName;
                    });
                }
                else
                {
                    yield return codeInstruction;
                }
            }
            //IEnumerator<CodeInstruction> enumerator = null;
            //yield break;
            yield break;
        }

        [HarmonyPatch(typeof(UILabel), "SetCurrentSelection")]
        [HarmonyPrefix]
        public static void OnSetCurrentSelection(UILabel __instance)
        {
            if (UIPopupList.current != null)
            {
                __instance.text = UIPopupList.current.value;
            }
        }

        // Note: this type is marked as 'beforefieldinit'.
        static UIFixes()
        {
        }


        private static Harmony instance;

        private static bool initialized;

        private static readonly Dictionary<string, Font> customFonts = new Dictionary<string, Font>();

        private static readonly Regex korPattern = new Regex("[가-힣]", RegexOptions.Compiled);

        private static readonly Regex passPattern = new Regex("^[-+.,\\d\\s]+$", RegexOptions.Compiled);

        private delegate void TranslateInfo(ref string text);

    }
}
