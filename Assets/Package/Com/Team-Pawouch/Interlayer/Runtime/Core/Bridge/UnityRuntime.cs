// (c)2024-2026 Team Pawouch
// UnityRuntime.cs - 2026-11-27 02:11:32

#nullable enable
using System;
using System.Collections;
using Package.Com.Team_Pawouch.Interlayer.Runtime.Core.Bridge.Api;
using UnityEngine;

namespace Package.Com.Team_Pawouch.Interlayer.Runtime.Core.Bridge;

[Obsolete("该实现将在下个版本中废弃，请使用新的Package.Com.Team_Pawouch.Interlayer.Runtime.Core.Bridge.Api.RuntimeState")]
public enum RuntimeState {
	None,
	Initialized,
	Runtime,
	Stopped,
	Destroy,
}

[Obsolete("该实现将在下个版本中废弃，请使用新的Package.Com.Team_Pawouch.Interlayer.Runtime.Core.Bridge.Runtime")]
internal class UnityRuntime : IUnityRuntime {
	public Guid   GUID         { get; }
	public string InstanceName { get; set; }
	public int    InstanceId   { get; }

	public RuntimeState State { get; private set; }

	public int TickRate {
		get => _tickRate;
		set {
			if(value <= 0) throw new ArgumentException("Tick rate must be greater than zero");

			if(_tickRate == value) return;

			_tickRate = value;
			OnTickRateChanged?.Invoke(value);
		}
	}

	public int Tick { get; private set; }

	public float Interval => 1f / TickRate;

	private int _tickRate;

	public TimeSnapshot Snapshot => new(0, Tick, Interval, Tick * Interval, Tick % TickRate);

	public event Action<TimeSnapshot>? OnTick;
	public event Action<int>?          OnTickRateChanged;
	public event Action?               OnTickStarted;
	public event Action?               OnTickStopped;
	public event Action?               OnTickUnpaused;
	public event Action?               OnTickPaused;

	public static Func<int, string, IUnityRuntime> GetConstructor() => (id, name) => new UnityRuntime(id, name);

	private UnityRuntime(int id, string name) {
		GUID         = Guid.NewGuid();
		InstanceName = name;
		InstanceId   = id;

		TickRate = (int)MathF.Ceiling((float)Screen.currentResolution.refreshRateRatio.value);
	}

	public void Construct(object[] args) { }

	public void Deconstruct() { }

	private IEnumerator TickIterator() {
		float elapsed = 0f;

		State = RuntimeState.Runtime;
		while(State is RuntimeState.Runtime or RuntimeState.Stopped) {
			if(State is RuntimeState.Stopped) {
				yield return null;
				continue;
			}

			elapsed += Time.deltaTime;

			while(elapsed >= Interval) {
				Tick++;
				elapsed -= Interval;
				OnTick?.Invoke(Snapshot);
			}

			yield return null;
		}
	}
}
