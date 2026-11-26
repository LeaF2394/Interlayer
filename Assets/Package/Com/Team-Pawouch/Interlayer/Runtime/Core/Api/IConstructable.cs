// (c)2024-2026 Team Pawouch
// IConstructable.cs - 2026-11-25 02:11:50

#nullable enable
namespace Package.Com.Team_Pawouch.Interlayer.Runtime.Core.Api;

public interface IConstructable {
	void Construct(object[] args);
	void Deconstruct();
}

// END FILE
