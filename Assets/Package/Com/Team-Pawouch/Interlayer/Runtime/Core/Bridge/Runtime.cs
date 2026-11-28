// (c)2024-2026 Team Pawouch
// Runtime.cs - 2026-11-28 08:11:02

#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Package.Com.Team_Pawouch.Interlayer.Runtime.Core.Bridge.Api;
using UnityEngine;

namespace Package.Com.Team_Pawouch.Interlayer.Runtime.Core.Bridge;

internal sealed class Runtime : Behaviour, IRuntime {
	public Guid GUID { get; private set; }

	public static Func<int, string, IRuntime> GetConstructor() => (id, name) => {
		GameObject obj = new(name) {
			hideFlags = HideFlags.NotEditable,
		};

		Runtime runtime = obj.AddComponent<Runtime>();
		runtime.GUID         = Guid.NewGuid();
		runtime.InstanceName = name;
		runtime.InstanceId   = id;
		runtime.Stage        = RuntimeStage.None;
		runtime._mono        = obj.AddComponent<Mono>();

		return runtime;
	};

	public void Construct(object[] args) {
		Stage = RuntimeStage.None;

		_iterator    = null;
		_snapshot    = new TimeSnapshot(0, 0, 0, 0, 0);
		_tickRate    = (int)Math.Ceiling(Screen.currentResolution.refreshRateRatio.value);
		_accumulator = 0;
		_elapsed     = 0;
		_captured.Clear();

		DontDestroyOnLoad(gameObject);
	}

	public void Deconstruct() {
		Switch(RuntimeStage.None);

		GUID         = Guid.Empty;
		InstanceId   = -1;
		InstanceName = "_2beDisposed";

		DestroyImmediate(_mono);
		_mono = null;

		Destroy(gameObject);
	}

	public string       InstanceName { get; set; } = "";
	public int          InstanceId   { get; private set; }
	public RuntimeStage Stage        { get; private set; }

	private Mono?        _mono;
	private Coroutine?   _iterator;
	private TimeSnapshot _snapshot;
	private int          _tickRate;
	private float        _accumulator;
	private float        _elapsed;

	private readonly Dictionary<int, TimeSnapshot> _captured   = new();
	private const    int                           MAX_CAPTURE = 65536;

	public event Action<TimeSnapshot, ValueChangeContext<RuntimeStage>>? StageChanged;
	public event Action<TimeSnapshot>?                                   Tick;
	public event Action<TimeSnapshot, ValueChangeContext<int>>?          TickRateChanged;
	public event Action<TimeSnapshot>?                                   TickStarted;
	public event Action<TimeSnapshot>?                                   TickStopped;

	private event Action<TimeSnapshot, int>? Advanced;

	public void Install() {
		Switch(RuntimeStage.Install);

		if(_mono == null) {
			Debug.LogError($"Coroutine scheduler failure");
			return;
		}

		_iterator = _mono.StartCoroutine(TickIterator());
	}

	public void Uninstall() {
		Switch(RuntimeStage.Uninstall);

		if(_mono == null) {
			Debug.LogError($"Coroutine scheduler failure");
			return;
		}

		if(_iterator != null) {
			_mono.StopCoroutine(_iterator);
			_iterator = null;
		}

		StageChanged    = null;
		Tick            = null;
		TickRateChanged = null;
		TickStarted     = null;
		TickStopped     = null;
		Advanced        = null;
		_captured.Clear();
	}

	public void Recover() {
		Switch(RuntimeStage.Runtime);

		TickStarted?.Invoke(_snapshot);
	}

	public void Break() {
		Switch(RuntimeStage.Break);

		TickStopped?.Invoke(_snapshot);
	}

	private void Switch(RuntimeStage will) {
		if(!Capable(Stage, will))
			throw new Exception($"Can not switch stage: {Stage.ToString().ToUpper()} -> {will.ToString().ToUpper()}");

		RuntimeStage raw = Stage;
		Stage = will;
		StageChanged?.Invoke(_snapshot, new ValueChangeContext<RuntimeStage>(raw, Stage));

		return;

		bool Capable(RuntimeStage from, RuntimeStage to) => from switch {
			RuntimeStage.None      => to is RuntimeStage.Install,
			RuntimeStage.Install   => to is RuntimeStage.Runtime or RuntimeStage.Break,
			RuntimeStage.Runtime   => to is RuntimeStage.Break or RuntimeStage.Uninstall,
			RuntimeStage.Break     => to is RuntimeStage.Runtime or RuntimeStage.Uninstall,
			RuntimeStage.Uninstall => to is RuntimeStage.None,
			_                      => throw new ArgumentOutOfRangeException(nameof(from), from, null)
		};
	}

	private IEnumerator TickIterator() {
		Switch(RuntimeStage.Runtime);

		TickStarted?.Invoke(_snapshot);
		_accumulator = 0;

		float interval = 1f / _tickRate;

		Tick            += Capture;
		TickRateChanged += OnTickRateChanged;
		Advanced        += OnAdvanced;

		while(Stage is RuntimeStage.Runtime or RuntimeStage.Break) {
			int tick = _snapshot.Tick;
			_elapsed += Time.deltaTime;

			if(Stage is RuntimeStage.Break) {
				yield return null;
				continue;
			}

			if(_snapshot.Alpha > 0f) Tick?.Invoke(_snapshot);

			_accumulator += Time.deltaTime;
			yield return null;

			while(_accumulator >= interval) {
				_accumulator -= interval;
				tick++;

				_snapshot = _snapshot with {
					Snapshots = _snapshot.Snapshots + 1,
					Tick = tick,
					DeltaTime = interval,
					ElapsedTime = _elapsed,
					Alpha = 0,
				};

				Tick?.Invoke(_snapshot);
			}

			_snapshot = _snapshot with { Alpha = _accumulator / interval };
		}

		Advanced        -= OnAdvanced;
		TickRateChanged -= OnTickRateChanged;
		Tick            -= Capture;
		TickStopped?.Invoke(_snapshot);
		yield break;

		void OnAdvanced(TimeSnapshot snapshot, int a) {
			switch(a) {
				case < 0:
					throw new ArgumentException("Advanced time cannot be negative");
				case 0:
					return;
				default:
					_accumulator = 0;

					_snapshot = snapshot with { Tick = snapshot.Tick + a, Alpha = 0 };
					Tick?.Invoke(_snapshot);
					break;
			}
		}

		void OnTickRateChanged(TimeSnapshot snapshot, ValueChangeContext<int> ctx) {
			interval     = 1f / ctx.New;
			_accumulator = 0;
		}

		void Capture(TimeSnapshot snapshot) {
			if(_captured.Count >= MAX_CAPTURE) _captured.Remove(_captured.Keys.Min());

			_captured.TryAdd(snapshot.Tick, snapshot);
		}
	}

	public void Advance(int tick) => Advanced?.Invoke(_snapshot, tick);

	public void SetTick(int tick) => throw new NotImplementedException();

	public TimeSnapshot? Query(int tick) {
		if(!_captured.TryGetValue(tick, out TimeSnapshot snapshot)) return null;

		return snapshot;
	}

	public void SetTickRate(int tickRate) {
		switch(tickRate) {
			case <= 5:
				throw new ArgumentException("The minimum value for Tick is 5");
			case >= 255:
				throw new ArgumentException("The maximum value for Tick is 255");
			default:
				int raw = _tickRate;
				_tickRate = tickRate;
				TickRateChanged?.Invoke(_snapshot, new ValueChangeContext<int>(raw, tickRate));
				break;
		}
	}

	private sealed class Mono : MonoBehaviour;
}
