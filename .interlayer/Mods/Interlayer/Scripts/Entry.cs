// (c)2024-2026 Team Pawouch
// Entry.cs - 2026-11-27 04:11:49

// An Example Mod

#nullable enable
using UnityEngine;
using Package.Com.Team_Pawouch.Interlayer.Runtime.Core.Bridge;
using Package.Com.Team_Pawouch.Interlayer.Runtime.Core.Bridge.Api;

namespace Interlayer;

public static class Entry {
	public static void Main(IRuntime runtime) {
		Debug.Log("Hello World!");

		runtime.Tick += s => {
			Debug.Log(s.Tick);
		};
	}
}
