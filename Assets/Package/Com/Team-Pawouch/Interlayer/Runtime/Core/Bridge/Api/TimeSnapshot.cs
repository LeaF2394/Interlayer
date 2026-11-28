// (c)2024-2026 Team Pawouch
// TimeSnapshot.cs - 2026-11-28 00:11:08

#nullable enable
namespace Package.Com.Team_Pawouch.Interlayer.Runtime.Core.Bridge.Api;

public record struct TimeSnapshot(int Snapshots, int Tick, float DeltaTime, float ElapsedTime, float Alpha);
