// (c)2024-2026 Team Pawouch
// IRuntime.cs - 2026-11-28 00:11:21

#nullable enable
using Package.Com.Team_Pawouch.Interlayer.Runtime.Core.Api;

namespace Package.Com.Team_Pawouch.Interlayer.Runtime.Core.Bridge.Api;

public interface IRuntime : ISingleton {
	ITickSystem? TickSystem { get; }

	void Install();
	void Uninstall();
}