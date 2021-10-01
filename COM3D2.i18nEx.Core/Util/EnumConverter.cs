using System;
using System.Runtime.CompilerServices;

namespace COM3D2.i18nEx.Core.Util
{
	internal static class EnumConverter<T> where T : Enum
	{
		// Note: this type is marked as 'beforefieldinit'.
		static EnumConverter()
		{
		}

		public static readonly Func<T, string> EnumToString = (T arg) => arg.ToString();

		public static readonly Func<string, T> EnumFromString = (string s) => (T)((object)Enum.Parse(typeof(T), s, true));
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

			internal string <.cctor>b__2_0(T arg)
			{
				return arg.ToString();
			}

			internal T <.cctor>b__2_1(string s)
			{
				return (T)((object)Enum.Parse(typeof(T), s, true));
			}

			public static readonly EnumConverter<T>.<>c <>9 = new EnumConverter<T>.<>c();
		}
		*/
	}
}
