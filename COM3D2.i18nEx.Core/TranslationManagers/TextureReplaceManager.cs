using System;
using System.Collections.Generic;
using System.IO;
using COM3D2.i18nEx.Core.Util;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace COM3D2.i18nEx.Core.TranslationManagers
{
	internal class TextureReplaceManager : TranslationManagerBase
	{
		public override void LoadLanguage()
		{
			Core.Logger.LogInfo("Loading texture replacements");
			this.missingTextures.Clear();
			this.textureReplacements.Clear();
			this.texReplacementLookup.Clear();
			this.texReplacementCache.Clear();
			IEnumerable<string> textureTranslationFileNames = Core.TranslationLoader.GetTextureTranslationFileNames();
			if (textureTranslationFileNames == null)
			{
				Core.Logger.LogInfo("No textures found! Skipping...");
				return;
			}
			foreach (string text in textureTranslationFileNames)
			{
				string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(text);
				if (this.textureReplacements.ContainsKey(fileNameWithoutExtension))
				{
					Core.Logger.LogWarning("Found duplicate replacements for texture \"" + fileNameWithoutExtension + "\". Please name all your textures uniquely. If there are name collisions, name them by hash.");
				}
				else
				{
					this.textureReplacements[fileNameWithoutExtension] = text;
				}
			}
		}

		private void Update()
		{
			if (Configuration.TextureReplacement.ReloadTranslationsKey.Value.IsPressed)
			{
				this.ReloadActiveTranslations();
			}
			if (Configuration.I2Translation.PrintFontNamesKey.Value.IsPressed)
			{
				Core.Logger.LogInfo("Supported fonts:\n" + string.Join("\n", Font.GetOSInstalledFontNames()));
			}
		}

		public bool ReplacementExists(string texName)
		{
			return this.textureReplacements.ContainsKey(texName);
		}

		public override void ReloadActiveTranslations()
		{
			this.LoadLanguage();
		}

		public byte[] GetReplacementTextureBytes(string texName, string tag = null, bool skipLogging = false)
		{
			TextureReplacement replacement = this.GetReplacement(texName, tag, skipLogging);
			if (replacement == null)
			{
				return null;
			}
			return replacement.Data;
		}

		public void DumpTexture(string texName, Texture tex)
		{
			if (this.dumpedItems.Contains(texName))
			{
				return;
			}
			Texture2D texture2D = tex as Texture2D;
			if (texture2D == null)
			{
				return;
			}
			string text = Utility.CombinePaths(new string[]
			{
				Paths.TranslationsRoot,
				Configuration.General.ActiveLanguage.Value,
				"Textures",
				"Dumped"
			});
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			Core.Logger.LogInfo("[DUMP] " + texName + ".png");
			File.WriteAllBytes(Path.Combine(text, texName + ".png"), Utility.TexToPng(texture2D));
			this.dumpedItems.Add(texName);
		}

		private TextureReplacement GetReplacement(string texName, string tag = null, bool skipLogging = false)
		{
			string text = (texName + ":" + tag).KnuthHash().ToString("X16");
			foreach (string text2 in new string[]
			{
				texName,
				text,
				string.Format("{0}@{1}", texName, SceneManager.GetActiveScene().buildIndex),
				string.Format("{0}@{1}", text, SceneManager.GetActiveScene().buildIndex)
			})
			{
				if (Configuration.TextureReplacement.VerboseLogging.Value && !skipLogging)
				{
					Core.Logger.LogInfo("Trying with name " + text2 + ".png");
				}
				if (this.textureReplacements.ContainsKey(text2))
				{
					return this.LoadReplacement(text2);
				}
			}
			return null;
		}

		private TextureReplacement LoadReplacement(string name)
		{
			LinkedListNode<TextureReplacement> linkedListNode;
			if (this.texReplacementLookup.TryGetValue(name, out linkedListNode))
			{
				this.texReplacementCache.Remove(linkedListNode);
				this.texReplacementCache.AddFirst(linkedListNode);
				return linkedListNode.Value;
			}
			if (this.texReplacementLookup.Count == Configuration.TextureReplacement.MaxTexturesCached.Value)
			{
				linkedListNode = this.texReplacementCache.Last;
				this.texReplacementCache.RemoveLast();
				this.texReplacementLookup.Remove(linkedListNode.Value.Name);
			}
			TextureReplacement result;
			try
			{
				TextureReplacement textureReplacement = new TextureReplacement(name, this.textureReplacements[name]);
				textureReplacement.Load();
				linkedListNode = this.texReplacementCache.AddFirst(textureReplacement);
				this.texReplacementLookup.Add(name, linkedListNode);
				result = textureReplacement;
			}
			catch (Exception ex)
			{
				Core.Logger.LogError("Failed to load texture \"" + name + "\" because: " + ex.Message);
				this.textureReplacements.Remove(name);
				result = null;
			}
			return result;
		}

		public TextureReplaceManager()
		{
		}

		private readonly HashSet<string> dumpedItems = new HashSet<string>();

		private readonly HashSet<string> missingTextures = new HashSet<string>();

		private readonly LinkedList<TextureReplacement> texReplacementCache = new LinkedList<TextureReplacement>();

		private readonly Dictionary<string, LinkedListNode<TextureReplacement>> texReplacementLookup = new Dictionary<string, LinkedListNode<TextureReplacement>>(StringComparer.InvariantCultureIgnoreCase);

		private readonly Dictionary<string, string> textureReplacements = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
	}
}
