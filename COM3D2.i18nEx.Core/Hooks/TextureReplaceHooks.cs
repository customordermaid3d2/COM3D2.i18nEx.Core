using System;
using System.IO;
using BepInEx.Harmony;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace COM3D2.i18nEx.Core.Hooks
{
	internal static class TextureReplaceHooks
	{
		public static void Initialize()
		{
			if (TextureReplaceHooks.initialized)
			{
				return;
			}
			TextureReplaceHooks.instance = Harmony.CreateAndPatchAll(typeof(TextureReplaceHooks), "horse.coder.i18nex.hooks.textures");
			TextureReplaceHooks.initialized = true;
		}

		[HarmonyPatch(typeof(FileSystemArchive), "IsExistentFile")]
		[HarmonyPatch(typeof(FileSystemWindows), "IsExistentFile")]
		[HarmonyPostfix]
		private static void IsExistentFileCheck(ref bool __result, string file_name)
		{
			if (file_name != null)
			{
				string extension = Path.GetExtension(file_name);
				if (extension != null && extension.Equals(".tex", StringComparison.InvariantCultureIgnoreCase))
				{
					if (!string.IsNullOrEmpty(file_name) && Core.TextureReplace.ReplacementExists(Path.GetFileNameWithoutExtension(file_name)))
					{
						__result = true;
					}
					return;
				}
			}
		}

		[HarmonyPatch(typeof(ImportCM), "LoadTexture")]
		[HarmonyPrefix]
		private static bool LoadTexture(ref TextureResource __result, AFileSystemBase f_fileSystem, string f_strFileName, bool usePoolBuffer)
		{
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(f_strFileName);
			if (string.IsNullOrEmpty(fileNameWithoutExtension))
			{
				return true;
			}
			bool skipLogging = true;
			if (Configuration.TextureReplacement.VerboseLogging.Value && TextureReplaceHooks.previousTexName != f_strFileName)
			{
				Core.Logger.LogInfo("[COM3D2_TEX] " + f_strFileName);
				TextureReplaceHooks.previousTexName = f_strFileName;
				skipLogging = false;
			}
			byte[] replacementTextureBytes = Core.TextureReplace.GetReplacementTextureBytes(fileNameWithoutExtension, "tex", skipLogging);
			if (replacementTextureBytes == null)
			{
				return true;
			}
			if (Configuration.TextureReplacement.VerboseLogging.Value)
			{
				Core.Logger.LogInfo("Replacing " + f_strFileName);
			}
			int width = 1;
			int height = 1;
			TextureFormat format = TextureFormat.ARGB32;
			TextureResource textureResource = __result;
			__result = new TextureResource(width, height, format, (textureResource != null) ? textureResource.uvRects : null, replacementTextureBytes);
			return false;
		}

		[HarmonyPatch(typeof(ImportCM), "LoadTexture")]
		[HarmonyPostfix]
		private static void OnTexLoaded(ref TextureResource __result, AFileSystemBase f_fileSystem, string f_strFileName, bool usePoolBuffer)
		{
			if (!Configuration.TextureReplacement.DumpTextures.Value || Configuration.TextureReplacement.SkipDumpingCMTextures.Value)
			{
				return;
			}
			Texture2D tex = __result.CreateTexture2D();
			Core.TextureReplace.DumpTexture(Path.GetFileNameWithoutExtension(f_strFileName), tex);
		}

		[HarmonyPatch(typeof(UIWidget), "mainTexture", MethodType.Getter)]
		[HarmonyPatch(typeof(UI2DSprite), "mainTexture", MethodType.Getter)]
		[HarmonyPostfix]
		private static void GetMainTexturePost(UIWidget __instance, ref Texture __result)
		{
			UI2DSprite ui2DSprite = __instance as UI2DSprite;
			Texture texture;
			if (ui2DSprite != null)
			{
				Sprite sprite2D = ui2DSprite.sprite2D;
				texture = ((sprite2D != null) ? sprite2D.texture : null);
			}
			else
			{
				Material material = __instance.material;
				texture = ((material != null) ? material.mainTexture : null);
			}
			if (texture == null || string.IsNullOrEmpty(texture.name) || texture.name.StartsWith("i18n_") || texture.name == "Font Texture")
			{
				return;
			}
			bool skipLogging = true;
			if (Configuration.TextureReplacement.VerboseLogging.Value && TextureReplaceHooks.previousTexName != ((texture != null) ? texture.name : null))
			{
				Core.Logger.LogInfo("[" + __instance.GetType().Name + "] " + ((texture != null) ? texture.name : null));
				TextureReplaceHooks.previousTexName = ((texture != null) ? texture.name : null);
				skipLogging = false;
			}
			byte[] replacementTextureBytes = Core.TextureReplace.GetReplacementTextureBytes(texture.name, __instance.GetType().Name, skipLogging);
			if (replacementTextureBytes == null)
			{
				if (Configuration.TextureReplacement.DumpTextures.Value)
				{
					Core.TextureReplace.DumpTexture(texture.name, texture);
				}
				return;
			}
			if (Configuration.TextureReplacement.VerboseLogging.Value)
			{
				Core.Logger.LogInfo("Replacing " + ((texture != null) ? texture.name : null));
			}
			Texture2D texture2D = texture as Texture2D;
			if (texture2D != null)
			{
				texture2D.LoadImage(TextureReplaceHooks.EmptyBytes);
				texture2D.LoadImage(replacementTextureBytes);
				texture2D.name = string.Format("i18n_{0}", texture2D);
				return;
			}
			Core.Logger.LogError(string.Concat(new string[]
			{
				"Texture ",
				texture.name,
				" is of type ",
				texture.GetType().FullName,
				" and not tex2d!"
			}));
		}

		[HarmonyPatch(typeof(UITexture), "mainTexture", MethodType.Getter)]
		[HarmonyPostfix]
		private static void GetMainTexturePostTex(UITexture __instance, ref Texture __result, ref Texture ___mTexture)
		{
			Texture texture;
			if ((texture = ___mTexture) == null)
			{
				Material material = __instance.material;
				texture = ((material != null) ? material.mainTexture : null);
			}
			Texture texture2 = texture;
			if (texture2 == null || string.IsNullOrEmpty(texture2.name) || texture2.name.StartsWith("i18n_"))
			{
				return;
			}
			bool skipLogging = true;
			if (Configuration.TextureReplacement.VerboseLogging.Value && TextureReplaceHooks.previousTexName != ((texture2 != null) ? texture2.name : null))
			{
				Core.Logger.LogInfo("[" + __instance.GetType().Name + "] " + ((texture2 != null) ? texture2.name : null));
				TextureReplaceHooks.previousTexName = ((texture2 != null) ? texture2.name : null);
				skipLogging = false;
			}
			byte[] replacementTextureBytes = Core.TextureReplace.GetReplacementTextureBytes(texture2.name, "UITexture", skipLogging);
			if (replacementTextureBytes == null)
			{
				if (Configuration.TextureReplacement.DumpTextures.Value)
				{
					Core.TextureReplace.DumpTexture(texture2.name, texture2);
				}
				return;
			}
			if (Configuration.TextureReplacement.VerboseLogging.Value)
			{
				Core.Logger.LogInfo("Replacing " + ((texture2 != null) ? texture2.name : null));
			}
			Texture2D texture2D = texture2 as Texture2D;
			if (texture2D != null)
			{
				texture2D.LoadImage(TextureReplaceHooks.EmptyBytes);
				texture2D.LoadImage(replacementTextureBytes);
				texture2D.name = string.Format("i18n_{0}", texture2D);
				return;
			}
			Core.Logger.LogError(string.Concat(new string[]
			{
				"Texture ",
				texture2.name,
				" is of type ",
				texture2.GetType().FullName,
				" and not tex2d!"
			}));
		}

		[HarmonyPatch(typeof(Image), "sprite", MethodType.Setter)]
		[HarmonyPrefix]
		private static void SetSprite(ref Sprite value)
		{
			if (value == null || value.texture == null || string.IsNullOrEmpty(value.texture.name) || value.texture.name.StartsWith("i18n_"))
			{
				return;
			}
			bool skipLogging = true;
			if (Configuration.TextureReplacement.VerboseLogging.Value)
			{
				string a = TextureReplaceHooks.previousTexName;
				Sprite sprite = value;
				string b;
				if (sprite == null)
				{
					b = null;
				}
				else
				{
					Texture2D texture = sprite.texture;
					b = ((texture != null) ? texture.name : null);
				}
				if (a != b)
				{
					ILogger logger = Core.Logger;
					string str = "[UnityEngine.UI.Image] ";
					Sprite sprite2 = value;
					string str2;
					if (sprite2 == null)
					{
						str2 = null;
					}
					else
					{
						Texture2D texture2 = sprite2.texture;
						str2 = ((texture2 != null) ? texture2.name : null);
					}
					logger.LogInfo(str + str2);
					Sprite sprite3 = value;
					string text;
					if (sprite3 == null)
					{
						text = null;
					}
					else
					{
						Texture2D texture3 = sprite3.texture;
						text = ((texture3 != null) ? texture3.name : null);
					}
					TextureReplaceHooks.previousTexName = text;
					skipLogging = false;
				}
			}
			byte[] replacementTextureBytes = Core.TextureReplace.GetReplacementTextureBytes(value.texture.name, "Image", skipLogging);
			if (replacementTextureBytes == null)
			{
				if (Configuration.TextureReplacement.DumpTextures.Value)
				{
					Core.TextureReplace.DumpTexture(value.texture.name, value.texture);
				}
				return;
			}
			if (Configuration.TextureReplacement.VerboseLogging.Value)
			{
				ILogger logger2 = Core.Logger;
				string str3 = "Replacing ";
				Sprite sprite4 = value;
				string str4;
				if (sprite4 == null)
				{
					str4 = null;
				}
				else
				{
					Texture2D texture4 = sprite4.texture;
					str4 = ((texture4 != null) ? texture4.name : null);
				}
				logger2.LogInfo(str3 + str4);
			}
			value.texture.LoadImage(TextureReplaceHooks.EmptyBytes);
			value.texture.LoadImage(replacementTextureBytes);
			value.texture.name = "i18n_" + value.texture.name;
		}

		[HarmonyPatch(typeof(MaskableGraphic), "OnEnable")]
		[HarmonyPrefix]
		private static void OnMaskableGraphicEnable(MaskableGraphic __instance)
		{
			Image image = __instance as Image;
			if (image == null || image.sprite == null)
			{
				return;
			}
			Sprite sprite = image.sprite;
			image.sprite = sprite;
		}

		// Note: this type is marked as 'beforefieldinit'.
		static TextureReplaceHooks()
		{
		}

		private const string FONT_TEX_NAME = "Font Texture";

		private static bool initialized;

		private static Harmony instance;

		private static readonly byte[] EmptyBytes = new byte[0];

		private static string previousTexName;
	}
}
