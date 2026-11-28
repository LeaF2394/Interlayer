// (c)2024-2026 Team Pawouch
// ValueChangeContext.cs - 2026-11-28 08:11:46

#nullable enable
namespace Package.Com.Team_Pawouch.Interlayer.Runtime.Core;

public record ValueChangeContext<T>(T Old, T New) where T : notnull;
