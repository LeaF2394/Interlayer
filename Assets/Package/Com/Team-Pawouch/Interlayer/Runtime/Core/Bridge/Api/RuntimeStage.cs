// (c)2024-2026 Team Pawouch
// RuntimeStage.cs - 2026-11-28 00:11:02

namespace Package.Com.Team_Pawouch.Interlayer.Runtime.Core.Bridge.Api;

public enum RuntimeStage : byte {
	None      = 0,
	Install   = 1,
	Runtime   = 2,
	Break     = 3,
	Uninstall = 4,
}
