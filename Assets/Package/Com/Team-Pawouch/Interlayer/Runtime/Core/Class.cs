// (c)2024-2026 Team Pawouch
// Class.cs - 2026-11-26 02:11:17

#nullable enable
using System;
using System.Collections.Generic;
using Package.Com.Team_Pawouch.Interlayer.Runtime.Core.Api;

namespace Package.Com.Team_Pawouch.Interlayer.Runtime.Core;

public static class Class {
	private static          int    _increment = 0;
	private static readonly object _lock      = new();

	public static void Install() {
		lock(_lock) {
			_increment = 0;
		}
	}

	public static void Uninstall() { }

	internal static int Increment() {
		lock(_lock) return _increment++;
	}
}

public static class Class<T> where T : class, IClass {
	private static readonly Dictionary<Guid, T>   _classes = new();
	private static readonly object                _lock    = new();
	private static          Func<int, string, T>? _constructor;

	public static bool HasConstructor() => _constructor != null;

	public static void SetConstructor(Func<int, string, T> constructor) {
		_constructor = constructor;
	}

	public static T Construct(string name) {
		Func<int, string, T> ctor = _constructor ?? throw new InvalidOperationException(
			                            $"Class: Cannot find constructor of [{typeof(T).FullName}].");
		int instanceId = Class.Increment();

		T @class = ctor.Invoke(instanceId, name) // 锁外
		        ?? throw new InvalidOperationException(
			           $"Class: constructor of [{typeof(T).FullName}] returned null.");

		lock(_lock) {
			if(!_classes.TryAdd(@class.GUID, @class))
				throw new InvalidOperationException(
					$"Class: duplicate GUID {@class.GUID:B} for [{typeof(T).FullName}].");
		}

		return @class;
	}

	public static void Deconstruct(Guid id) {
		T? @class = Search(id);

		if(@class == null)
			throw new NullReferenceException($"Class: Cannot find instance of ID:{id:B}");

		@class.Deconstruct();
		lock(_lock) {
			_classes.Remove(id);
		}
	}

	public static T? Search(Guid id) {
		lock(_lock) {
			return _classes.GetValueOrDefault(id);
		}
	}
}

// END FILE
