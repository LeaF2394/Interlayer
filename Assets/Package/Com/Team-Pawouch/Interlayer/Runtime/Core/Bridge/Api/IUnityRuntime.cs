// (c)2024-2026 Team Pawouch
// IUnityRuntime.cs - 2026-11-27 02:11:59

#nullable enable
using System;
using Package.Com.Team_Pawouch.Interlayer.Runtime.Core.Api;

namespace Package.Com.Team_Pawouch.Interlayer.Runtime.Core.Bridge.Api;

public struct TimeSnapshot(int Tick, int TickRate);

public interface IUnityRuntime : ISingleton {
	event Action<TimeSnapshot> OnTick;
}

// END FILE
