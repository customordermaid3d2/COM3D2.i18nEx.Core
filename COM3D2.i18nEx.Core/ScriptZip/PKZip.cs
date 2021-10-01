using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace COM3D2.i18nEx.Core.ScriptZip
{
	public static class PKZip
	{
		public static bool LoadZipFile(string zipPath)
		{
			try
			{
				PKZip.fileStreamList[zipPath] = File.OpenRead(zipPath);
			}
			catch
			{
				Core.Logger.LogError("Failed to open file: " + zipPath);
				return false;
			}
			return true;
		}

		public static Dictionary<string, string> ReadScriptNames(string zipPath)
		{
			FileStream fileStream;
			if (!PKZip.fileStreamList.TryGetValue(zipPath, out fileStream))
			{
				return null;
			}
			if (fileStream.Length < 98L)
			{
				return null;
			}
			BinaryReader binaryReader = new BinaryReader(fileStream);
			uint num = (uint)fileStream.Seek(-22L, SeekOrigin.End);
			uint num2 = uint.MaxValue;
			long num3 = (long)((ulong)((num < 65535U) ? 0U : (num - 65535U)));
			while (num3 <= fileStream.Position)
			{
				if (binaryReader.ReadUInt32() == 101010256U)
				{
					num = (uint)(fileStream.Position - 4L);
					fileStream.Position += 12L;
					num2 = binaryReader.ReadUInt32();
					break;
				}
				fileStream.Position -= 5L;
			}
			if (num2 == 4294967295U || num2 + 46U >= num)
			{
				return null;
			}
			Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			while (num2 < num)
			{
				fileStream.Position = (long)((ulong)num2);
				uint num4 = binaryReader.ReadUInt32();
				if (num4 == 101075792U)
				{
					break;
				}
				if (num4 != 33639248U)
				{
					return null;
				}
				fileStream.Position += 20L;
				bool flag = binaryReader.ReadUInt32() != 0U;
				ushort count = binaryReader.ReadUInt16();
				ushort num5 = binaryReader.ReadUInt16();
				ushort num6 = binaryReader.ReadUInt16();
				fileStream.Position += 8L;
				uint num7 = binaryReader.ReadUInt32();
				byte[] bytes = binaryReader.ReadBytes((int)count);
				num2 = (uint)(fileStream.Position + (long)((ulong)num5) + (long)((ulong)num6));
				if (flag)
				{
					num3 = fileStream.Position + (long)((ulong)num5);
					while (fileStream.Position < num3)
					{
						if (binaryReader.ReadUInt16() == 28789)
						{
							ushort num8 = binaryReader.ReadUInt16();
							binaryReader.ReadByte();
							uint num9 = binaryReader.ReadUInt32();
							byte[] array = binaryReader.ReadBytes((int)(num8 - 5));
							if (num9 == CRC32.ComputeCRC32(array))
							{
								bytes = array;
								num3 = 0L;
								break;
							}
							return null;
						}
						else
						{
							fileStream.Position += (long)((ulong)binaryReader.ReadUInt16());
						}
					}
					string text = (num3 == 0L) ? Encoding.UTF8.GetString(bytes) : Encoding.Default.GetString(bytes);
					if (text.EndsWith(".txt"))
					{
						dictionary[Path.GetFileNameWithoutExtension(text)] = zipPath + "\t" + num7.ToString();
					}
				}
			}
			if (dictionary.Count <= 0)
			{
				return null;
			}
			return dictionary;
		}

		public static byte[] ReadScriptData(string zipPath, uint offset)
		{
			FileStream fileStream;
			if (!PKZip.fileStreamList.TryGetValue(zipPath, out fileStream))
			{
				return null;
			}
			BinaryReader binaryReader = new BinaryReader(fileStream);
			fileStream.Position = (long)((ulong)offset);
			if (binaryReader.ReadUInt32() != 67324752U)
			{
				return null;
			}
			fileStream.Position += 2L;
			if ((binaryReader.ReadUInt16() & 1) == 1)
			{
				return null;
			}
			ushort num = binaryReader.ReadUInt16();
			fileStream.Position += 4L;
			uint num2 = binaryReader.ReadUInt32();
			fileStream.Position += 4L;
			uint num3 = binaryReader.ReadUInt32();
			if (num3 == 0U)
			{
				return null;
			}
			int num4 = (int)(binaryReader.ReadUInt16() + binaryReader.ReadUInt16());
			fileStream.Position += (long)num4;
			byte[] array = new byte[num3];
			if (num == 8)
			{
				new DeflateStream(fileStream, CompressionMode.Decompress).Read(array, 0, array.Length);
			}
			else
			{
				if (num != 0)
				{
					return null;
				}
				fileStream.Read(array, 0, array.Length);
			}
			if (num2 != CRC32.ComputeCRC32(array))
			{
				return null;
			}
			return array;
		}

		// Note: this type is marked as 'beforefieldinit'.
		static PKZip()
		{
		}

		private const uint minSizeECDR = 22U;

		private const uint minSizeCDR = 46U;

		private const uint minSizeLFH = 30U;

		private const uint minSizeFile = 98U;

		private const uint maxSizeZipComment = 65535U;

		private static Dictionary<string, FileStream> fileStreamList = new Dictionary<string, FileStream>(StringComparer.OrdinalIgnoreCase);
	}
}
