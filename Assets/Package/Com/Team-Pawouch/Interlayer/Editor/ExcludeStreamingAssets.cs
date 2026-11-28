#if UNITY_EDITOR
// (c)2024-2026 Team Pawouch
// ExcludeStreamingAssets.cs - 2026-11-28 14:11:07

#nullable enable
namespace Package.Com.Team_Pawouch.Interlayer.Editor;

internal static class ExcludeStreamingAssets {
	internal static string[] ExcludeFiles = [
		UED_CompilationTime.EnableFilePath,
		"editor.pid",
	];
}
#endif
