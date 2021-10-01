using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[CompilerGenerated]
internal sealed class <PrivateImplementationDetails>
{
	internal static uint ComputeStringHash(string s)
	{
		uint num;
		if (s != null)
		{
			num = 2166136261U;
			for (int i = 0; i < s.Length; i++)
			{
				num = ((uint)s[i] ^ num) * 16777619U;
			}
		}
		return num;
	}

	internal static readonly <PrivateImplementationDetails>.__StaticArrayInitTypeSize=1024 12F3E0576D447EB37B36D82BA0C1C5481B8F0D12FDC70347CE4A076B229D4C86;

	[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 1024)]
	private struct __StaticArrayInitTypeSize=1024
	{
	}
}
