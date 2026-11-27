// (c)2024-2026 Team Pawouch
// Singleton.cs - 2026-11-25 02:11:28

#nullable enable
using System;
using System.IO;
using System.Text;
using Package.Com.Team_Pawouch.Interlayer.Runtime.Core.Api;
using UnityEngine;
using static UnityEditor.EnterPlayModeOptions;

namespace Package.Com.Team_Pawouch.Interlayer.Runtime.Core;

public static class Singleton {
	public static bool IsInstalled {
		get {
			lock(_lock) return Directory.Exists(BaseClasspath);
		}
	}

	public static string BaseClasspath { get; } = Path.GetFullPath(Path.Combine(Application.persistentDataPath, "bin", "runtime", "classpath"));

	private static readonly object _lock = new();

	public static void Install() {
		lock(_lock) {
			if(IsInstalled) return;

			Directory.CreateDirectory(BaseClasspath);

			Debug.LogFormat("Runtime: Base Classpath at: {0}", BaseClasspath);
		}
	}

	public static void Uninstall() {
		if(!IsInstalled) {
			Debug.LogWarning("Runtime: Singleton has not been installed.");
			return;
		}

		lock(_lock) {
			Directory.Delete(BaseClasspath, true);
		}
	}
}

public static class Singleton<T> where T : class, ISingleton {
	public static T? ClassInstance { get; private set; }

	public static string Classpath { get; } = Path.Combine(
		Singleton.BaseClasspath,
		typeof(T).Namespace?.Replace('.', '/') ?? "Global",
		typeof(T).Name).Replace('\\', '/');

	private static readonly object _lock = new();

	public static event Action<T>? OnClassConstruct, OnClassChanged;
	public static event Action?    OnClassDeconstruct;

	static Singleton() {
		ClassInstance = null;

		Application.quitting -= Dispose;
		Application.quitting += Dispose;

#if UNITY_EDITOR
		UnityEditor.EditorApplication.playModeStateChanged -= EditorDispose;
		UnityEditor.EditorApplication.playModeStateChanged += EditorDispose;
#endif
	}

#if UNITY_EDITOR
	private static void EditorDispose(UnityEditor.PlayModeStateChange obj) {
		if(obj is not UnityEditor.PlayModeStateChange.ExitingPlayMode) return;
		Dispose();

		if(UnityEditor.EditorSettings.enterPlayModeOptions is not DisableDomainReload)
			UnityEditor.EditorApplication.playModeStateChanged -= EditorDispose;
	}
#endif

	private static void Dispose() {
		Application.quitting -= Dispose;

		T? old;
		lock(_lock) {
			if(!Singleton.IsInstalled) {
				Debug.LogWarning("Runtime: Singleton has not been installed.");
				return;
			}

			old           = ClassInstance;
			ClassInstance = null;

			OnClassConstruct   = null;
			OnClassChanged     = null;
			OnClassDeconstruct = null;
		}

		old?.Deconstruct();
	}

	public static void Construct(params object[] args) {
		T instance = Class<T>.Construct(typeof(T).FullName!);
		if(instance == null)
			throw new InvalidOperationException(
				$"Runtime: Singleton[{typeof(T).FullName}] producer returned null.");

		bool constructed;
		lock(_lock) {
			if(!Singleton.IsInstalled) {
				Debug.LogWarning("Runtime: Singleton has not been installed.");
				constructed = false;
			}
			else if(ClassInstance != null) {
				Debug.LogWarningFormat("Runtime: Singleton[{0}] has already been constructed.", typeof(T).FullName);
				constructed = false;
			}
			else {
				ClassInstance = instance;
				Directory.CreateDirectory(Classpath);
				constructed = true;
			}
		}

		if(!constructed) return;

		instance.Construct(args);
		OnClassConstruct?.Invoke(instance);
		Debug.LogFormat("Runtime: Singleton[{0}] constructed.", typeof(T).FullName);
	}

	public static bool SwapInstance(T? instance, out T? swap) {
		if(instance == null) {
			Debug.LogWarningFormat("Runtime: Singleton[{0}] received null instance to swap.", typeof(T).FullName);
			swap = null;
			return false;
		}

		T? old;
		lock(_lock) {
			if(!Singleton.IsInstalled) {
				Debug.LogWarning("Runtime: Singleton has not been installed.");
				swap = null;
				return false;
			}

			if(ClassInstance == null) {
				Debug.LogWarningFormat("Runtime: Singleton[{0}] has not been constructed. Please call Construct first", typeof(T).FullName);
				swap = null;
				return false;
			}

			if(ReferenceEquals(instance, ClassInstance)) {
				Debug.LogWarningFormat("Runtime: Singleton[{0}] received request to swap a same instance.", typeof(T).FullName);
				swap = null;
				return false;
			}

			if(instance.GUID == Guid.Empty) {
				Debug.LogWarningFormat("Runtime: Singleton[{0}] received request to swap an unconstructed instance.", typeof(T).FullName);
				swap = null;
				return false;
			}

			old           = ClassInstance;
			swap          = old;
			ClassInstance = instance;
		}

		OnClassChanged?.Invoke(instance);

		Debug.LogFormat("Runtime: Singleton[{0}] swapped ({1}:{2}) -> ({3}:{4}).",
		                typeof(T).FullName, old.InstanceId, old.GUID, instance.InstanceId, instance.GUID);
		return true;
	}

	public static void Deconstruct() {
		T? old;
		lock(_lock) {
			if(!Singleton.IsInstalled) {
				Debug.LogWarning("Runtime: Singleton has not been installed.");
				return;
			}

			if(ClassInstance == null) {
				Debug.LogWarningFormat("Runtime: Singleton[{0}] has not been constructed. Please call Construct first", typeof(T).FullName);
				return;
			}

			old           = ClassInstance;
			ClassInstance = null;
		}

		old.Deconstruct();
		OnClassDeconstruct?.Invoke();
	}

	public static string? Read(string file) => Read(file, Encoding.UTF8);

	public static string? Read(string file, Encoding encode) {
		if(file == null) throw new ArgumentNullException(nameof(file));

		lock(_lock) {
			if(!Singleton.IsInstalled) {
				Debug.LogWarning("Runtime: Singleton has not been installed.");
				return null;
			}

			string full = VerifyFile(Path.Combine(Classpath, file));
			return File.Exists(full)? File.ReadAllText(full, encode) : null;
		}
	}

	public static void Write(string file, string content) => Write(file, content, Encoding.UTF8);

	public static void Write(string file, string content, Encoding encode) {
		if(file == null) throw new ArgumentNullException(nameof(file));

		lock(_lock) {
			if(!Singleton.IsInstalled) {
				Debug.LogWarning("Runtime: Singleton has not been installed.");
				return;
			}

			Directory.CreateDirectory(Classpath);
			File.WriteAllText(VerifyFile(Path.Combine(Classpath, file)), content, encode);
		}
	}

	private static string VerifyFile(string combined) {
		string root = Path.GetFullPath(Classpath);
		string full = Path.GetFullPath(combined);

		if(!full.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) &&
		   !full.Equals(root, StringComparison.OrdinalIgnoreCase))
			throw new ArgumentException("The security of the path cannot be guaranteed.");

		return full;
	}
}
