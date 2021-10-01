using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace COM3D2.i18nEx.Core.TranslationManagers
{
	internal class TextureReplacement
	{
		public TextureReplacement(string name, string fullPath)
		{
			this.Name = name;
			this.FullPath = fullPath;
		}

		public string Name;

		public string FullPath;

		public byte[] Data;
		public void Load()
		{
			using (Stream stream = Core.TranslationLoader.OpenTextureTranslation(this.FullPath))
			{
				this.Data = new byte[stream.Length];
				stream.Read(this.Data, 0, this.Data.Length);
			}
		}


	}
}
