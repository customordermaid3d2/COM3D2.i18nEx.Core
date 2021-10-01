using System;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using UnityEngine;

namespace COM3D2.i18nEx.Core.Util
{
	internal static class ColorConverter
	{
		// Note: this type is marked as 'beforefieldinit'.
		static ColorConverter()
		{
		}

		private static readonly Regex RgbaPattern = new Regex("RGBA\\(\\s*(?<r>\\d\\.\\d+)\\s*,\\s*(?<g>\\d\\.\\d+)\\s*,\\s*(?<b>\\d\\.\\d+)\\s*,\\s*(?<a>\\d\\.\\d+)\\s*\\)", RegexOptions.Compiled);

		public static readonly Func<Color, string> ColorToString = (Color arg) => arg.ToString();

		public static readonly Func<string, Color> ColorFromString = delegate(string str)
		{
			Match match = ColorConverter.RgbaPattern.Match(str);
			if (!match.Success)
			{
				throw new FormatException("Invalid RGBA format");
			}
			float r = float.Parse(match.Groups["r"].Value);
			float g = float.Parse(match.Groups["g"].Value);
			float b = float.Parse(match.Groups["b"].Value);
			float a = float.Parse(match.Groups["a"].Value);
			return new Color(r, g, b, a);
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

			internal string <.cctor>b__3_0(Color arg)
			{
				return arg.ToString();
			}

			internal Color <.cctor>b__3_1(string str)
			{
				Match match = ColorConverter.RgbaPattern.Match(str);
				if (!match.Success)
				{
					throw new FormatException("Invalid RGBA format");
				}
				float r = float.Parse(match.Groups["r"].Value);
				float g = float.Parse(match.Groups["g"].Value);
				float b = float.Parse(match.Groups["b"].Value);
				float a = float.Parse(match.Groups["a"].Value);
				return new Color(r, g, b, a);
			}

			public static readonly ColorConverter.<>c <>9 = new ColorConverter.<>c();
		}
		*/
	}
}
