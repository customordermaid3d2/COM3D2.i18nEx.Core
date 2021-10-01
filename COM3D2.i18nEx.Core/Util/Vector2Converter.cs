using System;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using UnityEngine;

namespace COM3D2.i18nEx.Core.Util
{
	internal static class Vector2Converter
	{
		// Note: this type is marked as 'beforefieldinit'.
		static Vector2Converter()
		{
		}

		private static readonly Regex Vec2Pattern = new Regex("\\(\\s*(?<x>-?\\d+\\.\\d+)\\s*,\\s*(?<y>-?\\d+\\.\\d+)\\s*\\)", RegexOptions.Compiled);

		public static readonly Func<Vector2, string> Vector2ToString = (Vector2 arg) => arg.ToString();

		public static readonly Func<string, Vector2> Vector2FromString = delegate(string str)
		{
			Match match = Vector2Converter.Vec2Pattern.Match(str);
			if (!match.Success)
			{
				throw new FormatException("Invalid Vec2 format");
			}
			float x = float.Parse(match.Groups["x"].Value);
			float y = float.Parse(match.Groups["y"].Value);
			return new Vector2(x, y);
		};
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

			internal string <.cctor>b__3_0(Vector2 arg)
			{
				return arg.ToString();
			}

			internal Vector2 <.cctor>b__3_1(string str)
			{
				Match match = Vector2Converter.Vec2Pattern.Match(str);
				if (!match.Success)
				{
					throw new FormatException("Invalid Vec2 format");
				}
				float x = float.Parse(match.Groups["x"].Value);
				float y = float.Parse(match.Groups["y"].Value);
				return new Vector2(x, y);
			}

			public static readonly Vector2Converter.<>c <>9 = new Vector2Converter.<>c();
		}
		*/
	}
}
