#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

// (c)2024-2026 Team Pawouch
// GameBuilder.cs - 2026-11-28 15:11:49

#nullable enable
namespace Package.Com.Team_Pawouch.Interlayer.Editor;

internal static class GameBuilder {
	private const string PrefRelease = "Pawouch.Build.ReleaseCount.";
	private const string PrefDev     = "Pawouch.Build.DevCount.";

	// Persists across domain reloads so the original version can be restored even if
	// BuildPlayer triggers a recompile and the finally block never runs.
	private const string SessionOriginalVersion = "Pawouch.Build.OriginalVersion";

	private enum Plat {
		Win64,
		Win32,
		Arm64
	}

	// Architecture is decided solely by BuildTarget; no reflection into UnityEditor.Build.Architecture.
	// Note: there is no dedicated ARM64 BuildTarget; ARM64 is an architecture variant of StandaloneWindows64,
	// so Arm64 maps to the same target as Win64.
	private static BuildTarget TargetOf(Plat p) => p switch {
		Plat.Win64 => BuildTarget.StandaloneWindows64,
		Plat.Win32 => BuildTarget.StandaloneWindows,
		Plat.Arm64 => BuildTarget.StandaloneWindows64,
		_          => BuildTarget.StandaloneWindows64,
	};

	private static string PlatName(Plat p) => p switch {
		Plat.Win64 => "win64",
		Plat.Win32 => "x86",
		Plat.Arm64 => "arm64",
		_          => "win64",
	};

	private static string ProjPath            => Path.GetDirectoryName(Application.dataPath)!.Replace('\\', '/');
	private static string VersionsRoot        => $"{ProjPath}/Build/Versions";
	private static string StreamingAssetsPath => Application.streamingAssetsPath;

	// Takes the first two numeric segments of the version, ignoring any suffix such as
	// "-snapshot N". This keeps parsing stable even if a previous build left a dirty value.
	private static void SplitVersion(out int x, out int y) {
		string raw = PlayerSettings.bundleVersion ?? "";
		Match  m   = Regex.Match(raw, @"(\d+)\.(\d+)");
		if(m.Success) {
			int.TryParse(m.Groups[1].Value, out x);
			int.TryParse(m.Groups[2].Value, out y);
		}
		else {
			x = 0;
			y = 0;
		}
	}

	private static int NextCount(string key) {
		int v = EditorPrefs.GetInt(key, 0) + 1;
		EditorPrefs.SetInt(key, v);
		return v;
	}

	private static string ExeName() => $"{Application.productName}.exe";

	// Restores the original bundleVersion if a build was interrupted by a domain reload.
	[InitializeOnLoadMethod]
	private static void RestoreAfterDomainReload() {
		string original = SessionState.GetString(SessionOriginalVersion, "");
		if(PlayerSettings.bundleVersion != original) {
			PlayerSettings.bundleVersion = original;
			EditorIO.Warn("Recovered bundleVersion after domain reload: {0}", original);
		}

		SessionState.EraseString(SessionOriginalVersion);
	}

	private static void BuildRelease(Plat plat) {
		SplitVersion(out int x, out int y);
		string xy       = $"{x}.{y}";
		int    build    = NextCount(PrefRelease + xy);
		string ver      = $"{x}.{y}.{build}";
		string original = PlayerSettings.bundleVersion;

		string outDir = $"{VersionsRoot}/{ver}-{PlatName(plat)}";
		Directory.CreateDirectory(outDir);

		EditorIO.Info("==== Start [RELEASE] Build ====");
		EditorIO.Info("Platform: {0} | Original Version: {1} -> Build Version: {2}", PlatName(plat), original, ver);
		EditorIO.Info("Release build count [{0}] = {1}",                             xy,             build);

		SessionState.SetString(SessionOriginalVersion, original);
		try {
			PlayerSettings.bundleVersion = ver;
			BuildPlayerOptions opts   = MakeOptions(plat, Path.Combine(outDir, ExeName()));
			BuildReport        report = BuildPipeline.BuildPlayer(opts);
			if(report.summary.result == BuildResult.Succeeded) {
				EditorIO.Info("RELEASE build succeeded: {0}", outDir);
				EditorUtility.RevealInFinder(outDir);
			}
			else {
				EditorIO.Error("RELEASE build failed: {0}", report.summary.result);
			}
		}
		catch(Exception ex) {
			EditorIO.Exception(ex);
		}
		finally {
			PlayerSettings.bundleVersion = original;
			SessionState.EraseString(SessionOriginalVersion);
			EditorIO.Info("Rollback: {0}", original);
		}
	}

	private static void BuildDev(Plat plat) {
		SplitVersion(out int x, out int y);
		string xy       = $"{x}.{y}";
		int    n        = NextCount(PrefDev + xy);
		string ver      = $"{x}.{y}-snapshot {n}";
		string original = PlayerSettings.bundleVersion;

		string outDir = $"{VersionsRoot}/{ver}-{PlatName(plat)}";
		Directory.CreateDirectory(outDir);

		EditorIO.Info("==== Start [DEV] Build ====");
		EditorIO.Info("Platform: {0} | Original Version: {1} -> Build Version: {2}", PlatName(plat), original, ver);
		EditorIO.Info("Dev build count [{0}] = {1}",                                 xy,             n);

		Dictionary<string, byte[]> backup = new();
		bool                       ok     = false;
		SessionState.SetString(SessionOriginalVersion, original);
		try {
			PrepareStreamingAssets(backup);

			PlayerSettings.bundleVersion = ver;
			BuildPlayerOptions opts   = MakeOptions(plat, Path.Combine(outDir, ExeName()));
			BuildReport        report = BuildPipeline.BuildPlayer(opts);
			ok = report.summary.result == BuildResult.Succeeded;
			if(ok) {
				EditorIO.Info("DEV build succeeded: {0}", outDir);
				EditorUtility.RevealInFinder(outDir);
			}
			else {
				EditorIO.Error("DEV build failed: {0}", report.summary.result);
			}
		}
		catch(Exception ex) {
			EditorIO.Exception(ex);
		}
		finally {
			PlayerSettings.bundleVersion = original;
			SessionState.EraseString(SessionOriginalVersion);
			RestoreStreamingAssets(backup, ok);
			EditorIO.Info("Rollback: {0}", original);
		}
	}

	private static List<string> ExcludeList() {
		List<string> list = ["editor.pid"];
		try {
			list.AddRange(ExcludeStreamingAssets.ExcludeFiles);
		}
		catch {
			// Ignored
		}

		return list;
	}

	private static void PrepareStreamingAssets(Dictionary<string, byte[]> backup) {
		if(!Directory.Exists(StreamingAssetsPath)) return;
		foreach(string name in ExcludeList()) {
			string full = Path.Combine(StreamingAssetsPath, name).Replace('\\', '/');
			if(!File.Exists(full)) continue;
			try {
				backup[name] = File.ReadAllBytes(full);
				File.Delete(full);
				EditorIO.Info("Excluded file removed: {0}", full);
			}
			catch(Exception ex) {
				EditorIO.Warn("Failed to exclude {0}: {1}", full, ex.Message);
			}
		}
	}

	private static void RestoreStreamingAssets(Dictionary<string, byte[]> backup, bool buildOk) {
		if(!buildOk) {
			EditorIO.Warn("DEV build did not succeed; still attempting to restore original excluded files.");
		}

		Directory.CreateDirectory(StreamingAssetsPath);
		foreach(KeyValuePair<string, byte[]> kv in backup) {
			string full = Path.Combine(StreamingAssetsPath, kv.Key).Replace('\\', '/');
			try {
				File.WriteAllBytes(full, kv.Value);
				EditorIO.Info("Excluded file restored: {0}", full);
			}
			catch(Exception ex) {
				EditorIO.Warn("Failed to restore excluded file {0}: {1}", full, ex.Message);
			}
		}

		WritePidMarker();
	}

	private static void WritePidMarker() {
		string marker = Path.Combine(StreamingAssetsPath, "editor.pid").Replace('\\', '/');
		if(File.Exists(marker)) {
			EditorIO.Info("editor.pid already exists, skipping idempotently: {0}", marker);
			return;
		}

		try {
			string content = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
			File.WriteAllText(marker, content);
			EditorIO.Info("editor.pid written: {0}", marker);
		}
		catch(Exception ex) {
			EditorIO.Warn("Failed to write editor.pid: {0}", ex.Message);
		}
	}

	private static BuildPlayerOptions MakeOptions(Plat plat, string exePath) {
		List<string> scenes = [
			.. from s in EditorBuildSettings.scenes where s.enabled select s.path
		];

		BuildPlayerOptions opts = new() {
			scenes           = [.. scenes],
			locationPathName = exePath.Replace('\\', '/'),
			target           = TargetOf(plat),
			targetGroup      = BuildTargetGroup.Standalone,
			options          = BuildOptions.None,
		};
		return opts;
	}

	[MenuItem("Team Pawouch/Build/Release - Win64", false, 100)]
	private static void MenuRelWin64() => BuildRelease(Plat.Win64);

	[MenuItem("Team Pawouch/Build/Release - x86 (Win32)", false, 101)]
	private static void MenuRelWin32() => BuildRelease(Plat.Win32);

	[MenuItem("Team Pawouch/Build/Release - ARM64", false, 102)]
	private static void MenuRelArm64() => BuildRelease(Plat.Arm64);

	[MenuItem("Team Pawouch/Build/Dev (Snapshot) - Win64", false, 200)]
	private static void MenuDevWin64() => BuildDev(Plat.Win64);

	[MenuItem("Team Pawouch/Build/Dev (Snapshot) - x86 (Win32)", false, 201)]
	private static void MenuDevWin32() => BuildDev(Plat.Win32);

	[MenuItem("Team Pawouch/Build/Dev (Snapshot) - ARM64", false, 202)]
	private static void MenuDevArm64() => BuildDev(Plat.Arm64);
}
#endif
