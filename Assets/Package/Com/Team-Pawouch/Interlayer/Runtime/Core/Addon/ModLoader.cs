// (c)2024-2026 Team Pawouch
// ModLoader.cs - 2026-11-27 03:11:55

#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Package.Com.Team_Pawouch.Interlayer.Runtime.Core.Addon.Api;
using Package.Com.Team_Pawouch.Interlayer.Runtime.Core.Bridge.Api;
using Package.Com.Team_Pawouch.Interlayer.Runtime.Core.Kernel;
using Package.Com.Team_Pawouch.Interlayer.Runtime.Interpreter;
using UnityEngine;

namespace Package.Com.Team_Pawouch.Interlayer.Runtime.Core.Addon;

internal sealed class ModLoader : IModLoader {
	public Guid   GUID         { get; }
	public string InstanceName { get; set; }
	public int    InstanceId   { get; }

	public           IReadOnlyList<ModInfo> LoadedMods => _loaded;
	private readonly List<ModInfo>          _loaded = [];

	public static Func<int, string, IModLoader> GetConstructor() => (id, name) => new ModLoader(id, name);

	private ModLoader(int id, string name) {
		GUID         = Guid.NewGuid();
		InstanceId   = id;
		InstanceName = name;
	}

	public void Construct(object[] args) { }

	public void Deconstruct() {
		_loaded.Clear();
	}

	public IReadOnlyList<ModInfo> LoadAll(string modsRoot) {
		_loaded.Clear();

		if(!Directory.Exists(modsRoot)) {
			Debug.LogWarning($"[ModLoader] Mods root not found: {modsRoot}");
			return _loaded;
		}

		List<ModInfo> mods        = [];
		string[]      directories = Directory.GetDirectories(modsRoot);

		if(directories.Length <= 0) return [];

		foreach(string dir in directories) {
			string manifestPath = Path.Combine(dir, "Manifest.json");
			if(!File.Exists(manifestPath)) {
				Debug.LogWarning($"[ModLoader] Skip (no Manifest.json): {dir}");
				continue;
			}

			ModManifest? manifest;
			try {
				manifest = JsonConvert.DeserializeObject<ModManifest>(File.ReadAllText(manifestPath));
			}
			catch(Exception ex) {
				Debug.LogError($"[ModLoader] Bad Manifest in {dir}: {ex.Message}");
				continue;
			}

			if(manifest == null || string.IsNullOrWhiteSpace(manifest.ModId)) {
				Debug.LogError($"[ModLoader] Invalid Manifest (no Id): {dir}");
				continue;
			}

			string? icon                   = null;
			string  iconPath               = Path.Combine(dir, "Icon.png");
			if(File.Exists(iconPath)) icon = iconPath;

			mods.Add(new ModInfo { Id = manifest.ModId, Directory = dir, IconPath = icon, Manifest = manifest });
		}

		ResolveAndLoad(mods);

		return _loaded;
	}

	public void Run(IEnumerable<ModInfo> mods, IUnityRuntime runtime) {
		foreach(ModInfo mod in mods) {
			Debug.Log($"{mod.Id} - {( mod.Loaded? "Loaded" : "Errors" )}");
			string entry = $"{mod.Id}.Entry";

			if(mod.Assembly == null) continue;

			Singleton<CSharpCLR>.ClassInstance!.Assemble(mod.Assembly);
			Singleton<CSharpCLR>.ClassInstance!.ExecuteStatic(entry, "Main", runtime);
		}
	}

	private void ResolveAndLoad(List<ModInfo> mods) {
		Dictionary<string, ModInfo> byId = new(StringComparer.OrdinalIgnoreCase);
		foreach(ModInfo m in mods.Where(m => !byId.TryAdd(m.Id, m))) {
			Fail(m, $"Duplicate mod id: {m.Id}");
		}

		HashSet<string> resolved = new(StringComparer.OrdinalIgnoreCase);
		int             progress;

		do {
			progress = 0;
			foreach(ModInfo mod in mods) {
				if(resolved.Contains(mod.Id)) continue;

				bool    ready           = true;
				string? blockingFailure = null;

				foreach(( string depId, string minVer ) in mod.Manifest.Dependencies) {
					if(!byId.TryGetValue(depId, out ModInfo? dep)) {
						Fail(mod, $"Missing dependency '{depId}'");
						resolved.Add(mod.Id);
						progress++;
						ready = false;
						break;
					}

					if(!resolved.Contains(depId)) {
						ready = false;
						break;
					}

					if(!dep.Loaded) {
						blockingFailure ??= dep.Error ?? $"Dependency '{depId}' failed";
					}
					else if(!Satisfies(dep.Manifest.ModVersion, minVer)) {
						blockingFailure = $"Dependency '{depId}' version {dep.Manifest.ModVersion} < {minVer}";
					}
				}

				if(!ready || resolved.Contains(mod.Id)) continue;

				if(blockingFailure != null) {
					Fail(mod, $"Dependency unsatisfied: {blockingFailure}");
					resolved.Add(mod.Id);
					progress++;
					continue;
				}

				IEnumerable<byte[]> images = mod.Manifest.Dependencies.Keys
				                                .Select(id => byId[id].Assembly)
				                                .Where(a => a != null)!;

				string error = "";

				List<string> refs = [.. mod.Manifest.References];
				string[] core = [
					"Package.Com.Team-Pawouch.Runtime.Core",
					// TODO: "Package.Com.Team-Pawouch.Runtime.Gameplay",
				];

				foreach(string refer in core) {
					if(refs.Contains(refer, StringComparer.OrdinalIgnoreCase)) continue;

					refs.Add(refer);
				}

				byte[]? bin = CSharpInterpreter.InterpretProject(mod.Directory, [.. refs], images,
				                                                 onCompiled: (ok, msg) => {
					                                                 error = msg;
					                                                 if(ok) Debug.Log($"[ModLoader] Compiled '{mod.Id}': {msg}");
				                                                 });

				if(bin == null) {
					Fail(mod, $"Compile failed: {error}");
					resolved.Add(mod.Id);
					progress++;
					continue;
				}

				mod.Assembly = bin;
				mod.Loaded   = true;
				_loaded.Add(mod);
				resolved.Add(mod.Id);
				progress++;
			}
		} while(progress > 0);

		foreach(ModInfo mod in mods.Where(mod => !resolved.Contains(mod.Id)))
			Fail(mod, "Circular dependency detected");
	}

	private static void Fail(ModInfo mod, string reason) {
		mod.Loaded = false;
		mod.Error  = reason;
		Debug.LogError($"[ModLoader] Mod '{mod.Id}' removed from load queue: {reason}");
	}

	private static bool Satisfies(string installed, string required) {
		if(string.IsNullOrWhiteSpace(required)) return true;
		if(Version.TryParse(installed, out Version? a) && Version.TryParse(required, out Version? b)) return a >= b;
		return string.Equals(installed, required, StringComparison.OrdinalIgnoreCase);
	}
}
