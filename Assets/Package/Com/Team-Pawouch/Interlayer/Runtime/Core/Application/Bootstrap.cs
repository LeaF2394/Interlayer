// (c)2024-2026 Team Pawouch
// Bootstrap.cs - 2026-11-26 02:11:44

#nullable enable
using System.Collections.Generic;
using Package.Com.Team_Pawouch.Interlayer.Runtime.Core.Addon;
using Package.Com.Team_Pawouch.Interlayer.Runtime.Core.Addon.Api;
using Package.Com.Team_Pawouch.Interlayer.Runtime.Core.Bridge;
using Package.Com.Team_Pawouch.Interlayer.Runtime.Core.Bridge.Api;
using Package.Com.Team_Pawouch.Interlayer.Runtime.Core.Kernel;
using UnityEngine;
#if UNITY_EDITOR
#endif

namespace Package.Com.Team_Pawouch.Interlayer.Runtime.Core.App;

internal static class Bootstrap {
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
	private static void Complete() {
		// 1. Complete Game Dir

		// 2. Check Update and Download new updates -> Reload Game
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void Subsystem() {
		Class.Install();
		Singleton.Install();

		Class<AppPath>.SetConstructor(AppPath.GetConstructor());
		Class<CSharpCLR>.SetConstructor(CSharpCLR.GetConstructor());
		Class<IUnityRuntime>.SetConstructor(UnityRuntime.GetConstructor());
		Class<IModLoader>.SetConstructor(ModLoader.GetConstructor());

		Singleton<AppPath>.Construct();
		Singleton<CSharpCLR>.Construct();
		Singleton<IUnityRuntime>.Construct();
		Singleton<IModLoader>.Construct();
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void Boot() {
		IReadOnlyList<ModInfo> infos = Singleton<IModLoader>.ClassInstance!.LoadAll(Singleton<AppPath>.ClassInstance!.ModsPath);
		Singleton<IModLoader>.ClassInstance!.Run(infos, Singleton<IUnityRuntime>.ClassInstance!);
	}
}

// END FILE
