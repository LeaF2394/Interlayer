// (c)2024-2026 Team Pawouch
// IClass.cs - 2026-11-25 02:11:43

#nullable enable
namespace Package.Com.Team_Pawouch.Interlayer.Runtime.Core.Api;

public interface IClass : IIdentifier, IConstructable {
	string InstanceName { get; set; }
	int    InstanceId   { get; }
}

// END FILE
