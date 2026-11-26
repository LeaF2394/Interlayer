// (c)2024-2026 Team Pawouch
// ModManifest.cs - 2026-11-27 03:11:49

#nullable enable
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Package.Com.Team_Pawouch.Interlayer.Runtime.Core.Addon;

[JsonObject]
public sealed class ModManifest {
	[JsonProperty("Id")]           public string                     ModId        { get; set; } = "";
	[JsonProperty("Version")]      public string                     ModVersion   { get; set; } = "1.0.0";
	[JsonProperty("References")]   public string[]                   References   { get; set; } = [];
	[JsonProperty("Dependencies")] public Dictionary<string, string> Dependencies { get; set; } = [];
}

// END FILE
