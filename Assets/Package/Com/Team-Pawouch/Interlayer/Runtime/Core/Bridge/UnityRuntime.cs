// (c)2024-2026 Team Pawouch
// UnityRuntime.cs - 2026-11-27 02:11:32

#nullable enable
using System;
using Package.Com.Team_Pawouch.Interlayer.Runtime.Core.Bridge.Api;
using UnityEngine;

namespace Package.Com.Team_Pawouch.Interlayer.Runtime.Core.Bridge;

internal class UnityRuntime : IUnityRuntime {
	public Guid   GUID         { get; }
	public string InstanceName { get; set; }
	public int    InstanceId   { get; }

	public event Action<TimeSnapshot>? OnTick;

	public static Func<int, string, IUnityRuntime> GetConstructor() => (id, name) => new UnityRuntime(id, name);

	private UnityRuntime(int id, string name) {
		GUID         = Guid.NewGuid();
		InstanceName = name;
		InstanceId   = id;
	}

	public void Construct(object[] args) { }

	public void Deconstruct() { }
}

// END FILE
