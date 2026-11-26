// (c)2024-2026 Team Pawouch
// AppPath.cs - 2026-11-27 04:11:55

#nullable enable
using System;
using System.IO;
using Package.Com.Team_Pawouch.Interlayer.Runtime.Core.Api;
using UnityEngine;

namespace Package.Com.Team_Pawouch.Interlayer.Runtime.Core.App;

public sealed class AppPath : ISingleton {
	public Guid GUID { get; }

	public string InstanceName { get; set; }
	public int    InstanceId   { get; }

	public string BasePath { get; }
	public string GamePath { get; }
	public string ModsPath { get; }

	public static Func<int, string, AppPath> GetConstructor() => (id, name) => new AppPath(id, name);

	private AppPath(int id, string name) {
		GUID         = Guid.NewGuid();
		InstanceName = name;
		InstanceId   = id;

		BasePath = Path.GetFullPath(Path.Combine(Application.dataPath, "..")).Replace('\\', '/');
		GamePath = Path.Combine(BasePath, $".{Application.productName.ToLower()}").Replace('\\', '/');
		ModsPath = Path.Combine(GamePath, "Mods").Replace('\\', '/');
	}

	public void Construct(object[] args) {
		Directory.CreateDirectory(BasePath);
		Directory.CreateDirectory(GamePath);
		Directory.CreateDirectory(ModsPath);
	}

	public void Deconstruct() { }
}

// END FILE
