using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;
using COM3D2.i18nEx.Core.ScriptZip;
using COM3D2.i18nEx.Core.Util;
using UnityEngine;

namespace COM3D2.i18nEx.Core.TranslationManagers
{
	internal class ScriptTranslationManager : TranslationManagerBase
	{
		public void OnLevelWasLoaded(int level)
		{
			SubtitleHelper.currentLevel = level;
		}

		private void Update()
		{
			if (Configuration.ScriptTranslations.ReloadTranslationsKey.Value.IsPressed)
			{
				this.ReloadActiveTranslations();
			}
			if (SubtitleHelper.yotogiGlobal && SubtitleHelper.currentLevel != 63)
			{
				SubtitleMovieManager.GetGlobalInstance(false).Clear();
				SubtitleHelper.yotogiGlobal = false;
			}
		}

		public override void LoadLanguage()
		{
			Core.Logger.LogInfo("Loading script translations");
			this.namesFile = null;
			this.translationFiles.Clear();
			this.translationFileCache.Clear();
			this.translationFileLookup.Clear();
			IEnumerable<string> scriptTranslationFileNames = Core.TranslationLoader.GetScriptTranslationFileNames();
			IEnumerable<string> scriptTranslationZipNames = Core.TranslationLoader.GetScriptTranslationZipNames();
			if (scriptTranslationFileNames == null && scriptTranslationZipNames == null)
			{
				Core.Logger.LogInfo("No script translation found! Skipping...");
				return;
			}
			if (scriptTranslationFileNames != null)
			{
				foreach (string text in scriptTranslationFileNames)
				{
					string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(text);
					if (this.translationFiles.ContainsKey(fileNameWithoutExtension))
					{
						Core.Logger.LogWarning(string.Concat(new string[]
						{
							"Script translation file ",
							fileNameWithoutExtension,
							" is declared twice in different locations (",
							text,
							" and ",
							this.translationFiles[fileNameWithoutExtension],
							")"
						}));
					}
					else if (fileNameWithoutExtension == "__npc_names" && this.namesFile == null)
					{
						this.namesFile = new ScriptTranslationFile(fileNameWithoutExtension, text);
						this.namesFile.LoadTranslations();
					}
					else
					{
						this.translationFiles[fileNameWithoutExtension] = text;
					}
				}
			}
			if (scriptTranslationZipNames == null)
			{
				return;
			}
			foreach (string zipPath in scriptTranslationZipNames)
			{
				if (PKZip.LoadZipFile(zipPath))
				{
					foreach (KeyValuePair<string, string> keyValuePair in PKZip.ReadScriptNames(zipPath))
					{
						if (this.translationFiles.ContainsKey(keyValuePair.Key))
						{
							Core.Logger.LogWarning(string.Concat(new string[]
							{
								"Script translation file ",
								keyValuePair.Key,
								" is declared twice in different locations (",
								this.translationFiles[keyValuePair.Key].Replace("\t", ":"),
								" and ",
								keyValuePair.Value.Replace("\t", ":"),
								")"
							}));
						}
						else
						{
							this.translationFiles[keyValuePair.Key] = keyValuePair.Value;
						}
					}
				}
			}
		}

		public List<SubtitleData> GetSubtitle(string fileName, string voiceName)
		{
			if (fileName == null || !this.translationFiles.ContainsKey(fileName))
			{
				return null;
			}
			LinkedListNode<ScriptTranslationFile> linkedListNode;
			if (!this.translationFileLookup.TryGetValue(fileName, out linkedListNode))
			{
				linkedListNode = this.LoadFile(fileName);
				if (linkedListNode == null)
				{
					return null;
				}
			}
			List<SubtitleData> result;
			if (!linkedListNode.Value.Subtitles.TryGetValue(voiceName, out result))
			{
				return null;
			}
			return result;
		}

		public string GetTranslation(string fileName, string text, out bool isName)
		{
			isName = false;
			string result;
			if (this.namesFile != null && this.namesFile.Translations.TryGetValue(text, out result))
			{
				isName = true;
				return result;
			}
			if (fileName == null || !this.translationFiles.ContainsKey(fileName))
			{
				return this.NoTranslation(text);
			}
			LinkedListNode<ScriptTranslationFile> linkedListNode;
			if (!this.translationFileLookup.TryGetValue(fileName, out linkedListNode))
			{
				linkedListNode = this.LoadFile(fileName);
				if (linkedListNode == null)
				{
					return this.NoTranslation(text);
				}
			}
			string result2;
			if (linkedListNode.Value.Translations.TryGetValue(text, out result2))
			{
				return result2;
			}
			return this.NoTranslation(text);
		}

		private string NoTranslation(string inputText)
		{
			if (Configuration.ScriptTranslations.SendScriptToClipboard.Value)
			{
				this.clipboardBuffer.AppendLine(inputText);
			}
			return null;
		}

		private void Awake()
		{
			base.StartCoroutine(this.SendToClipboardRoutine());
		}

		private IEnumerator SendToClipboardRoutine()
		{
			for (;;)
			{
				yield return new WaitForSeconds((float)Configuration.ScriptTranslations.ClipboardCaptureTime.Value);
				if (this.clipboardBuffer.Length > 0)
				{
					Clipboard.SetText(this.clipboardBuffer.ToString());
					this.clipboardBuffer.Length = 0;
				}
			}
			//yield break;
		}

		public bool WriteTranslation(string fileName, string original, string translated)
		{
			ScriptTranslationFile scriptTranslationFile = null;
			if (this.translationFiles.ContainsKey(fileName))
			{
				scriptTranslationFile = this.LoadFile(fileName).Value;
				if (scriptTranslationFile.Translations.ContainsKey(original))
				{
					return false;
				}
				scriptTranslationFile.Translations[original] = translated;
			}
			string text = original.Escape() + "\t" + translated.Escape();
			if (scriptTranslationFile == null || scriptTranslationFile.RawString != null)
			{
				string text2 = Path.Combine(Path.Combine(Paths.TranslationsRoot, Configuration.General.ActiveLanguage.Value), "Script");
				string text3 = Path.Combine(text2, fileName + ".txt");
				if (!Directory.Exists(text2))
				{
					Directory.CreateDirectory(text2);
				}
				if (scriptTranslationFile == null)
				{
					File.WriteAllText(text3, text);
				}
				else
				{
					File.WriteAllLines(text3, new string[]
					{
						scriptTranslationFile.RawString,
						text
					});
				}
				this.translationFiles[fileName] = text3;
			}
			else
			{
				File.AppendAllText(this.translationFiles[fileName], Environment.NewLine + text);
			}
			return true;
		}

		public override void ReloadActiveTranslations()
		{
			this.LoadLanguage();
		}

		private LinkedListNode<ScriptTranslationFile> LoadFile(string fileName)
		{
			LinkedListNode<ScriptTranslationFile> linkedListNode;
			if (this.translationFileLookup.TryGetValue(fileName, out linkedListNode))
			{
				this.translationFileCache.Remove(linkedListNode);
				this.translationFileCache.AddFirst(linkedListNode);
				return linkedListNode;
			}
			if (this.translationFileCache.Count == Configuration.ScriptTranslations.MaxTranslationFilesCached.Value)
			{
				this.translationFileLookup.Remove(this.translationFileCache.Last.Value.FileName);
				this.translationFileCache.RemoveLast();
			}
			LinkedListNode<ScriptTranslationFile> result;
			try
			{
				ScriptTranslationFile scriptTranslationFile = new ScriptTranslationFile(fileName, this.translationFiles[fileName]);
				scriptTranslationFile.LoadTranslations();
				LinkedListNode<ScriptTranslationFile> linkedListNode2 = this.translationFileCache.AddFirst(scriptTranslationFile);
				this.translationFileLookup.Add(fileName, linkedListNode2);
				result = linkedListNode2;
			}
			catch (Exception ex)
			{
				Core.Logger.LogError(string.Concat(new string[]
				{
					"Failed to load translations for file ",
					fileName,
					" because: ",
					ex.Message,
					". Skipping file..."
				}));
				this.translationFiles.Remove(fileName);
				result = null;
			}
			return result;
		}

		public ScriptTranslationManager()
		{
		}

		private readonly StringBuilder clipboardBuffer = new StringBuilder();

		private readonly LinkedList<ScriptTranslationFile> translationFileCache = new LinkedList<ScriptTranslationFile>();

		private readonly Dictionary<string, LinkedListNode<ScriptTranslationFile>> translationFileLookup = new Dictionary<string, LinkedListNode<ScriptTranslationFile>>();

		private readonly Dictionary<string, string> translationFiles = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

		private ScriptTranslationFile namesFile;

		/*
		[CompilerGenerated]
		private sealed class <SendToClipboardRoutine>d__12 : IEnumerator<object>, IDisposable, IEnumerator
		{
			[DebuggerHidden]
			public <SendToClipboardRoutine>d__12(int <>1__state)
			{
				this.<>1__state = <>1__state;
			}

			[DebuggerHidden]
			void IDisposable.Dispose()
			{
			}

			bool IEnumerator.MoveNext()
			{
				int num = this.<>1__state;
				ScriptTranslationManager scriptTranslationManager = this;
				if (num != 0)
				{
					if (num != 1)
					{
						return false;
					}
					this.<>1__state = -1;
					if (scriptTranslationManager.clipboardBuffer.Length > 0)
					{
						Clipboard.SetText(scriptTranslationManager.clipboardBuffer.ToString());
						scriptTranslationManager.clipboardBuffer.Length = 0;
					}
				}
				else
				{
					this.<>1__state = -1;
				}
				this.<>2__current = new WaitForSeconds((float)Configuration.ScriptTranslations.ClipboardCaptureTime.Value);
				this.<>1__state = 1;
				return true;
			}

			object IEnumerator<object>.Current
			{
				[DebuggerHidden]
				get
				{
					return this.<>2__current;
				}
			}

			[DebuggerHidden]
			void IEnumerator.Reset()
			{
				throw new NotSupportedException();
			}

			object IEnumerator.Current
			{
				[DebuggerHidden]
				get
				{
					return this.<>2__current;
				}
			}

			private int <>1__state;

			private object <>2__current;

			public ScriptTranslationManager <>4__this;
		}
		*/
	}
}
