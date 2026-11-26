// (c)2024-2026 Team Pawouch
// ModInfo.cs - 2026-11-27 03:11:38

#nullable enable
namespace Package.Com.Team_Pawouch.Interlayer.Runtime.Core.Addon;

public sealed class ModInfo {
	public string      Id        { get; init; } = "";
	public string      Directory { get; init; } = "";
	public string?     IconPath  { get; init; }
	public ModManifest Manifest  { get; init; } = new();

	public byte[]? Assembly { get; set; }

	public bool    Loaded { get; set; }
	public string? Error  { get; set; }
}
