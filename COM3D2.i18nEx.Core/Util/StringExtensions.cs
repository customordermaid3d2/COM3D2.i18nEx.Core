using System;
using System.Collections.Generic;
using System.Text;

namespace COM3D2.i18nEx.Core.Util
{
	internal static class StringExtensions
	{
		public static string Splice(this string self, int start, int end)
		{
			if (start < 0)
			{
				start += self.Length;
			}
			if (end < 0)
			{
				end += self.Length;
			}
			return self.Substring(start, end - start + 1);
		}

		public static ulong KnuthHash(this string read)
		{
			ulong num = 3074457345618258791UL;
			foreach (char c in read)
			{
				num += (ulong)c;
				num *= 3074457345618258799UL;
			}
			return num;
		}

		public static KeyValuePair<string, string> SplitTranslation(this string txt)
		{
			int num;
			if ((num = txt.IndexOf("<E>", StringComparison.InvariantCultureIgnoreCase)) > 0)
			{
				return new KeyValuePair<string, string>(txt.Substring(0, num).Trim(), txt.Substring(num + 3).Trim());
			}
			return new KeyValuePair<string, string>(txt.Trim(), string.Empty);
		}

		public static string Escape(this string txt)
		{
			if (string.IsNullOrEmpty(txt))
			{
				return txt;
			}
			StringBuilder stringBuilder = new StringBuilder(txt.Length + 2);
			int i = 0;
			while (i < txt.Length)
			{
				char c = txt[i];
				if (c <= '"')
				{
					switch (c)
					{
					case '\0':
						stringBuilder.Append("\\0");
						break;
					case '\u0001':
					case '\u0002':
					case '\u0003':
					case '\u0004':
					case '\u0005':
					case '\u0006':
						goto IL_12E;
					case '\a':
						stringBuilder.Append("\\a");
						break;
					case '\b':
						stringBuilder.Append("\\b");
						break;
					case '\t':
						stringBuilder.Append("\\t");
						break;
					case '\n':
						stringBuilder.Append("\\n");
						break;
					case '\v':
						stringBuilder.Append("\\v");
						break;
					case '\f':
						stringBuilder.Append("\\f");
						break;
					case '\r':
						stringBuilder.Append("\\r");
						break;
					default:
						if (c != '"')
						{
							goto IL_12E;
						}
						stringBuilder.Append("\\\"");
						break;
					}
				}
				else if (c != '\'')
				{
					if (c != '\\')
					{
						goto IL_12E;
					}
					stringBuilder.Append("\\");
				}
				else
				{
					stringBuilder.Append("\\'");
				}
				IL_136:
				i++;
				continue;
				IL_12E:
				stringBuilder.Append(c);
				goto IL_136;
			}
			return stringBuilder.ToString();
		}

		public static string Unescape(this string txt)
		{
			if (string.IsNullOrEmpty(txt))
			{
				return txt;
			}
			StringBuilder stringBuilder = new StringBuilder(txt.Length);
			int i = 0;
			while (i < txt.Length)
			{
				int num = txt.IndexOf('\\', i);
				if (num < 0 || num == txt.Length - 1)
				{
					num = txt.Length;
				}
				stringBuilder.Append(txt, i, num - i);
				if (num < txt.Length)
				{
					char c = txt[num + 1];
					if (c <= '\\')
					{
						if (c <= '\'')
						{
							if (c != '"')
							{
								if (c != '\'')
								{
									goto IL_143;
								}
								stringBuilder.Append('\'');
							}
							else
							{
								stringBuilder.Append('"');
							}
						}
						else if (c != '0')
						{
							if (c != '\\')
							{
								goto IL_143;
							}
							stringBuilder.Append('\\');
						}
						else
						{
							stringBuilder.Append('\0');
						}
					}
					else if (c <= 'b')
					{
						if (c != 'a')
						{
							if (c != 'b')
							{
								goto IL_143;
							}
							stringBuilder.Append('\b');
						}
						else
						{
							stringBuilder.Append('\a');
						}
					}
					else if (c != 'f')
					{
						if (c != 'n')
						{
							switch (c)
							{
							case 'r':
								stringBuilder.Append('\r');
								break;
							case 's':
							case 'u':
								goto IL_143;
							case 't':
								stringBuilder.Append('\t');
								break;
							case 'v':
								stringBuilder.Append('\v');
								break;
							default:
								goto IL_143;
							}
						}
						else
						{
							stringBuilder.Append('\n');
						}
					}
					else
					{
						stringBuilder.Append('\f');
					}
					IL_152:
					i = num + 2;
					continue;
					IL_143:
					stringBuilder.Append('\\').Append(c);
					goto IL_152;
				}
				break;
			}
			return stringBuilder.ToString();
		}
	}
}
