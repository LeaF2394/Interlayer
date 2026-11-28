// (c)2024-2026 Team Pawouch
// Bootstrap.cs - 2026-11-26 02:11:44

#nullable enable
using System.Collections.Generic;
using Package.Com.Team_Pawouch.Interlayer.Runtime.Core.Addon;
using Package.Com.Team_Pawouch.Interlayer.Runtime.Core.Addon.Api;
using Package.Com.Team_Pawouch.Interlayer.Runtime.Core.Api;
using Package.Com.Team_Pawouch.Interlayer.Runtime.Core.Bridge.Api;
using Package.Com.Team_Pawouch.Interlayer.Runtime.Core.Kernel;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Package.Com.Team_Pawouch.Interlayer.Runtime.Core.App;

internal static class Bootstrap {
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
	private static void Complete() {
		Class.Install();
		Singleton.Install();

		RegisterDispose<AppPath>();

		Class<AppPath>.SetConstructor(AppPath.GetConstructor());

		Singleton<AppPath>.Construct();
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void Subsystem() {
		RegisterDispose<CSharpCLR>();
		RegisterDispose<IRuntime>();
		RegisterDispose<IModLoader>();

		Class<CSharpCLR>.SetConstructor(CSharpCLR.GetConstructor());
		Class<IRuntime>.SetConstructor(Bridge.Runtime.GetConstructor());
		Class<IModLoader>.SetConstructor(ModLoader.GetConstructor());

		Singleton<CSharpCLR>.Construct();
		Singleton<IRuntime>.Construct();
		Singleton<IModLoader>.Construct();
	}

	private static void RegisterDispose<T>() where T : class, ISingleton {
		Application.quitting -= Singleton<T>.Dispose;
		Application.quitting += Singleton<T>.Dispose;
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void Boot() {
		Singleton<IModLoader>.ClassInstance!.LoadAll(Singleton<AppPath>.ClassInstance!.ModsPath);
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	private static void Run() {
		Singleton<IRuntime>.ClassInstance!.Install();
		Singleton<IModLoader>.ClassInstance!.Run(Singleton<IModLoader>.ClassInstance.LoadedMods, Singleton<IRuntime>.ClassInstance!);
	}
}

// END FILE
