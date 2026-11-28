// (c)2024-2026 Team Pawouch
// ITickSystem.cs - 2026-11-28 13:11:34

#nullable enable
using System;

namespace Package.Com.Team_Pawouch.Interlayer.Runtime.Core.Bridge.Api;

public interface ITickSystem {
	RuntimeStage Stage { get; }

	event Action<TimeSnapshot, ValueChangeContext<RuntimeStage>>? StageChanged;
	event Action<TimeSnapshot>?                                   Tick;
	event Action<TimeSnapshot, ValueChangeContext<int>>?          TickRateChanged;
	event Action<TimeSnapshot>?                                   TickStarted, TickStopped;

	void Install();
	void Uninstall();

	void Recover();
	void Break();

	void          Advance(int tick);
	void          SetTick(int tick);
	TimeSnapshot? Query(int   tick);

	void SetTickRate(int tickRate);
}
