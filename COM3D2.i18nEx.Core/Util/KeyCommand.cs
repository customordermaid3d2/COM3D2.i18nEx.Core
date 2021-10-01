using System;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace COM3D2.i18nEx.Core.Util
{
	internal class KeyCommand : IDisposable
	{
		public KeyCommand(params KeyCode[] keyCodes)
		{
			this.KeyCodes = keyCodes;
			this.KeyStates = new bool[this.KeyCodes.Length];
			KeyCommandHandler.Register(this);
		}

		private KeyCode[] KeyCodes;

		private bool[] KeyStates;

		public bool IsPressed
		{
			get
			{
				return this.KeyStates.All((bool k) => k);
			}
		}

		public void Dispose()
		{
			this.ReleaseUnmanagedResources();
			GC.SuppressFinalize(this);
		}

		public void UpdateState()
		{
			for (int i = 0; i < this.KeyCodes.Length; i++)
			{
				KeyCode key = this.KeyCodes[i];
				this.KeyStates[i] = Input.GetKey(key);
			}
		}

		private void ReleaseUnmanagedResources()
		{
			KeyCommandHandler.Unregister(this);
		}

		~KeyCommand()
		{
			this.ReleaseUnmanagedResources();
		}

		// Note: this type is marked as 'beforefieldinit'.
		static KeyCommand()
		{
		}

		public static readonly Func<KeyCommand, string> KeyCommandToString = (KeyCommand kc) => string.Join("+", (from k in kc.KeyCodes
		select k.ToString()).ToArray<string>());

		public static readonly Func<string, KeyCommand> KeyCommandFromString = (string s) => new KeyCommand((from k in s.Split(new char[]
		{
			'+'
		}, StringSplitOptions.RemoveEmptyEntries)
		select (KeyCode)Enum.Parse(typeof(KeyCode), k, true)).ToArray<KeyCode>());
		/*
		[CompilerGenerated]
		private readonly KeyCode[] <KeyCodes>k__BackingField;

		[CompilerGenerated]
		private readonly bool[] <KeyStates>k__BackingField;

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

			internal bool <get_IsPressed>b__10_0(bool k)
			{
				return k;
			}

			internal string <.cctor>b__15_0(KeyCommand kc)
			{
				return string.Join("+", (from k in kc.KeyCodes
				select k.ToString()).ToArray<string>());
			}

			internal string <.cctor>b__15_2(KeyCode k)
			{
				return k.ToString();
			}

			internal KeyCommand <.cctor>b__15_1(string s)
			{
				return new KeyCommand((from k in s.Split(new char[]
				{
					'+'
				}, StringSplitOptions.RemoveEmptyEntries)
				select (KeyCode)Enum.Parse(typeof(KeyCode), k, true)).ToArray<KeyCode>());
			}

			internal KeyCode <.cctor>b__15_3(string k)
			{
				return (KeyCode)Enum.Parse(typeof(KeyCode), k, true);
			}

			public static readonly KeyCommand.<>c <>9 = new KeyCommand.<>c();

			public static Func<bool, bool> <>9__10_0;

			public static Func<KeyCode, string> <>9__15_2;

			public static Func<string, KeyCode> <>9__15_3;
		}
		*/
	}
}
