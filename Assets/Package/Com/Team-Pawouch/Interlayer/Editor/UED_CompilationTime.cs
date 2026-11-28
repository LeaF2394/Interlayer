#if UNITY_EDITOR

// (c)2024-2026 Team Pawouch
// UED_CompilationTime.cs - 2026-11-26 23:11:55
// v 1.2.0

#nullable enable
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace Package.Com.Team_Pawouch.Interlayer.Editor;

[InitializeOnLoad]
internal static class UED_CompilationTime {
	private record Compilation(int Serial, Stopwatch Watch);

	private static readonly Dictionary<object, Compilation> _compilations = new();
	private const           string                          PREFSKey_CT   = "k.ctime";

	public static  string EnableFilePath => Path.Combine(Application.streamingAssetsPath, "compilation_time.enable");
	private static string NumberColorTag => ColorUtility.ToHtmlStringRGB(Color.paleGoldenRod);

	[MenuItem("Team Pawouch/Editor/Compilation Pipeline/Show Compilation Time")]
	internal static void CreateEnableFile() {
		if(File.Exists(EnableFilePath)) {
			EditorIO.Warn("Already Enabled!");
			return;
		}

		Directory.CreateDirectory(Application.streamingAssetsPath);
		File.Create(EnableFilePath).Dispose();

		EditorIO.Info("Compilation Time: <color=#{0}>Enabled</color>", ColorUtility.ToHtmlStringRGB(Color.mediumSpringGreen));
	}

	[MenuItem("Team Pawouch/Editor/Compilation Pipeline/Hide Compilation Time")]
	internal static void DeleteEnableFile() {
		if(!File.Exists(EnableFilePath)) {
			EditorIO.Warn("Already Hidden!");
			return;
		}

		File.Delete(EnableFilePath);

		EditorIO.Info("Compilation Time: <color=#{0}>Disabled</color>", ColorUtility.ToHtmlStringRGB(Color.coral));
	}

	static UED_CompilationTime() {
		if(!File.Exists(EnableFilePath)) return;

		CompilationPipeline.compilationStarted  -= CompilationStarted;
		CompilationPipeline.compilationStarted  += CompilationStarted;
		CompilationPipeline.compilationFinished -= CompilationFinished;
		CompilationPipeline.compilationFinished += CompilationFinished;
	}

	private static void CompilationStarted(object obj) {
		int time = EditorPrefs.GetInt(PREFSKey_CT, 0);
		EditorPrefs.SetInt(PREFSKey_CT, time + 1);

		Compilation compilation = new(time, Stopwatch.StartNew());
		_compilations[obj] = compilation;

		EditorIO.Info("Compiler: Start <color=#{0}>#{1}</color> Compilation", NumberColorTag, time);
	}

	private static void CompilationFinished(object obj) {
		if(_compilations.TryGetValue(obj, out Compilation? compilation)) {
			compilation.Watch.Stop();
			EditorIO.Info("Compiler: Finish <color=#{0}>#{1}</color> Compilation, Duration: <color=#{0}>{2:F2}</color> s (<color=#{0}>{3:F4}</color> ms)", NumberColorTag, compilation.Serial, compilation.Watch.Elapsed.TotalSeconds, compilation.Watch.Elapsed.TotalMilliseconds);

			_compilations.Remove(obj);
			return;
		}

		EditorIO.Warn("Compiler: Finish an Unknown Compilation, TOKEN REFERENCE MISSING.");
	}
}

// END FILE
#endif

// CHANGELOGS

// [v 1.0.0]
// Basic timing feature

// [v 1.1.0]
// Toggle to show/hide compilation time

// [v 1.2.0]
// Colorize log message
