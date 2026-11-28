// (c)2024-2026 Team Pawouch
// Runtime.cs - 2026-11-28 08:11:02

#nullable enable
using System;
using Package.Com.Team_Pawouch.Interlayer.Runtime.Core.Bridge.Api;
using UnityEngine;

namespace Package.Com.Team_Pawouch.Interlayer.Runtime.Core.Bridge;

internal sealed class Runtime : MonoBehaviour, IRuntime {
	public Guid GUID { get; private set; }

	public static Func<int, string, IRuntime> GetConstructor() => (id, name) => {
		GameObject obj = new(name) {
			hideFlags = HideFlags.NotEditable,
		};

		Runtime runtime = obj.AddComponent<Runtime>();
		runtime.GUID         = Guid.NewGuid();
		runtime.InstanceName = name;
		runtime.InstanceId   = id;
		runtime._mono        = obj.AddComponent<Mono>();

		return runtime;
	};

	public void Construct(object[] args) {
		if(_mono == null) {
			Debug.LogError($"Runtime: Coroutine scheduler failure");
			return;
		}

		TickSystem = new TickSystem(50, _mono);

		DontDestroyOnLoad(gameObject);
	}

	public void Deconstruct() {
		Uninstall();

		GUID         = Guid.Empty;
		InstanceId   = -1;
		InstanceName = "_2beDisposed";

		DestroyImmediate(_mono);
		_mono      = null;
		TickSystem = null;

		Destroy(gameObject);
	}

	public string       InstanceName { get; set; } = "";
	public int          InstanceId   { get; private set; }
	public ITickSystem? TickSystem   { get; private set; }

	private Mono? _mono;

	public void Install() {
		if(_mono == null) {
			Debug.LogError($"Runtime: Coroutine scheduler failure");
			return;
		}

		if(TickSystem == null) {
			Debug.LogError($"Runtime: TickSystem failure");
			return;
		}

		TickSystem.Install();
	}

	public void Uninstall() {
		if(_mono == null) {
			Debug.LogError($"Coroutine scheduler failure");
			return;
		}

		if(TickSystem == null) {
			Debug.LogError($"Runtime: TickSystem failure");
			return;
		}

		TickSystem.Uninstall();
	}
}
