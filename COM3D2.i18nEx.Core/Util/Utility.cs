using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace COM3D2.i18nEx.Core.Util
{
	public static class Utility
	{
		public static bool IsNullOrWhiteSpace(this string str)
		{
			return str == null || str.All(new Func<char, bool>(char.IsWhiteSpace));
		}

		public static string ToLF(this string val)
		{
			StringBuilder stringBuilder = new StringBuilder(val.Length);
			foreach (char c in val)
			{
				if (c != '\r')
				{
					stringBuilder.Append(c);
				}
			}
			return stringBuilder.ToString();
		}

		public static string CombinePaths(string part1, string part2)
		{
			return Path.Combine(part1, part2);
		}

		public static string CombinePaths(params string[] parts)
		{
			if (parts.Length == 0)
			{
				return null;
			}
			if (parts.Length == 1)
			{
				return parts[0];
			}
			string text = parts[0];
			for (int i = 1; i < parts.Length; i++)
			{
				text = Path.Combine(text, parts[i]);
			}
			return text;
		}

		public static byte[] TexToPng(Texture2D tex)
		{
			if (tex.format == TextureFormat.DXT1 || tex.format == TextureFormat.DXT5)
			{
				return Utility.DuplicateTextureToPng(tex);
			}
			byte[] result;
			try
			{
				result = tex.EncodeToPNG();
			}
			catch (Exception)
			{
				result = Utility.DuplicateTextureToPng(tex);
			}
			return result;
		}

		private static byte[] DuplicateTextureToPng(Texture2D tex)
		{
			Texture2D texture2D = Utility.Duplicate(tex);
			byte[] result = texture2D.EncodeToPNG();
			UnityEngine.Object.Destroy(texture2D);
			return result;
		}

		private static Texture2D Duplicate(Texture texture)
		{
			RenderTexture temporary = RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
			Graphics.Blit(texture, temporary);
			RenderTexture active = RenderTexture.active;
			RenderTexture.active = temporary;
			Texture2D texture2D = new Texture2D(texture.width, texture.height, TextureFormat.ARGB32, false);
			texture2D.ReadPixels(new Rect(0f, 0f, (float)temporary.width, (float)temporary.height), 0, 0);
			texture2D.Apply();
			RenderTexture.active = active;
			RenderTexture.ReleaseTemporary(temporary);
			return texture2D;
		}
	}
}
