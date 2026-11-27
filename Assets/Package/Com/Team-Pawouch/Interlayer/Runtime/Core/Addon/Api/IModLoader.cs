// (c)2024-2026 Team Pawouch
// IModLoader.cs - 2026-11-27 03:11:25

#nullable enable
using System.Collections.Generic;
using Package.Com.Team_Pawouch.Interlayer.Runtime.Core.Api;
using Package.Com.Team_Pawouch.Interlayer.Runtime.Core.Bridge.Api;

namespace Package.Com.Team_Pawouch.Interlayer.Runtime.Core.Addon.Api;

public interface IModLoader : ISingleton {
	IReadOnlyList<ModInfo> LoadedMods { get; }
	IReadOnlyList<ModInfo> LoadAll(string           modsRoot);
	void                   Run(IEnumerable<ModInfo> mods, IUnityRuntime runtime);
}

// END FILE
