using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using ExIni;

namespace COM3D2.i18nEx.Core.Util
{
	internal class ConfigWrapper<T> : IReloadable
	{
		public ConfigWrapper(IniFile file, string savePath, string section, string key, string description = null, T defaultValue = default(T), Func<T, string> toStringConvert = null, Func<string, T> fromStringConvert = null)
		{
			this.file = file;
			this.savePath = savePath;
			this.defaultValue = defaultValue;
			this.iniKey = file[section][key];
			this.iniKey.Comments.Comments = ((description != null) ? description.Split(new char[]
			{
				'\n'
			}).ToList<string>() : null);
			TypeConverter cvt = TypeDescriptor.GetConverter(typeof(T));
			if (fromStringConvert == null && !cvt.CanConvertFrom(typeof(string)))
			{
				throw new ArgumentException("Default TypeConverter can't convert from String");
			}
			if (toStringConvert == null && !cvt.CanConvertTo(typeof(string)))
			{
				throw new ArgumentException("Default TypeConverter can't convert to String");
			}
			this.toStringConvert = (toStringConvert ?? ((T v) => cvt.ConvertToInvariantString(v)));
			this.fromStringConvert = (fromStringConvert ?? ((string v) => (T)((object)cvt.ConvertFromInvariantString(v))));
			if (this.iniKey.Value == null)
			{
				this.Value = defaultValue;
				return;
			}
			this.prevValueRaw = this.iniKey.RawValue;
			try
			{
				this.prevValue = this.fromStringConvert(this.iniKey.Value);
			}
			catch (Exception)
			{
				this.Value = defaultValue;
			}
		}

		public T Value
		{
			get
			{
				return this.prevValue;
			}
			set
			{
				string text = this.toStringConvert(value);
				if (text == this.prevValueRaw)
				{
					return;
				}
				this.iniKey.Value = text;
				this.UnloadValue();
				this.prevValue = value;
				this.prevValueRaw = this.iniKey.RawValue;
				this.Save();
				Action<T> valueChanged = this.ValueChanged;
				if (valueChanged == null)
				{
					return;
				}
				valueChanged(value);
			}
		}

		public void Reload()
		{
			try
			{
				if (!(this.iniKey.RawValue == this.prevValueRaw))
				{
					this.UnloadValue();
					this.prevValue = this.fromStringConvert(this.iniKey.Value);
					Action<T> valueChanged = this.ValueChanged;
					if (valueChanged != null)
					{
						valueChanged(this.prevValue);
					}
				}
			}
			catch (Exception)
			{
				this.Value = this.defaultValue;
			}
		}

		public event Action<T> ValueChanged;/*
		{
			[CompilerGenerated]
			add
			{
				Action<T> action = this.ValueChanged;
				Action<T> action2;
				do
				{
					action2 = action;
					Action<T> value2 = (Action<T>)Delegate.Combine(action2, value);
					action = Interlocked.CompareExchange<Action<T>>(ref this.ValueChanged, value2, action2);
				}
				while (action != action2);
			}
			[CompilerGenerated]
			remove
			{
				Action<T> action = this.ValueChanged;
				Action<T> action2;
				do
				{
					action2 = action;
					Action<T> value2 = (Action<T>)Delegate.Remove(action2, value);
					action = Interlocked.CompareExchange<Action<T>>(ref this.ValueChanged, value2, action2);
				}
				while (action != action2);
			}
		}
		*/
		private void UnloadValue()
		{
			if (this.prevValue != null)
			{
				IDisposable disposable = this.prevValue as IDisposable;
				if (disposable != null)
				{
					disposable.Dispose();
				}
			}
		}

		private void Save()
		{
			this.file.Save(this.savePath);
		}

		private readonly T defaultValue;

		private readonly IniFile file;

		private readonly Func<string, T> fromStringConvert;

		private readonly IniKey iniKey;

		private readonly string savePath;

		private readonly Func<T, string> toStringConvert;

		private T prevValue;

		private string prevValueRaw;
		/*
		[CompilerGenerated]
		private Action<T> ValueChanged;

		[CompilerGenerated]
		private sealed class <>c__DisplayClass8_0
		{
			public <>c__DisplayClass8_0()
			{
			}

			internal string <.ctor>b__0(T v)
			{
				return this.cvt.ConvertToInvariantString(v);
			}

			internal T <.ctor>b__1(string v)
			{
				return (T)((object)this.cvt.ConvertFromInvariantString(v));
			}

			public TypeConverter cvt;
		}
		*/
	}
}
