using System;
using System.Collections.Generic;

namespace COM3D2.i18nEx.Core.Util
{
	internal static class KeyCommandHandler
	{
		public static void Register(KeyCommand command)
		{
			KeyCommandHandler.KeyCommands.Add(command);
		}

		public static void Unregister(KeyCommand command)
		{
			KeyCommandHandler.KeyCommands.Remove(command);
		}

		public static void UpdateState()
		{
			foreach (KeyCommand keyCommand in KeyCommandHandler.KeyCommands)
			{
				keyCommand.UpdateState();
			}
		}

		// Note: this type is marked as 'beforefieldinit'.
		static KeyCommandHandler()
		{
		}

		public static List<KeyCommand> KeyCommands = new List<KeyCommand>();
	}
}
