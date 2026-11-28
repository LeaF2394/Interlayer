// (c)2024-2026 Team Pawouch
// IUnityRuntime.cs - 2026-11-27 02:11:59

#nullable enable
using System;
using Package.Com.Team_Pawouch.Interlayer.Runtime.Core.Api;

namespace Package.Com.Team_Pawouch.Interlayer.Runtime.Core.Bridge.Api;

[Obsolete("该Api接口及其实现将在下个版本中废弃，请使用新的Package.Com.Team_Pawouch.Interlayer.Runtime.Core.Bridge.Api.IRuntime")]
public interface IUnityRuntime : ISingleton {
	int Tick     { get; }
	int TickRate { get; set; }

	event Action<int>?          OnTickRateChanged;
	event Action<TimeSnapshot>? OnTick;
	event Action?               OnTickStarted,  OnTickStopped;
	event Action?               OnTickUnpaused, OnTickPaused;
}

// END FILE
