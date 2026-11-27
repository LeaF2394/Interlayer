// (c)2024-2026 Team Pawouch
// CSharpCLR.cs - 2026-11-27 01:11:17

#nullable enable
using System;
using Package.Com.Team_Pawouch.Interlayer.Runtime.Core.Api;

namespace Package.Com.Team_Pawouch.Interlayer.Runtime.Core.Kernel;

internal sealed class CSharpCLR : ISingleton {
	public Guid   GUID         { get; }
	public string InstanceName { get; set; }
	public int    InstanceId   { get; }

	private const string DOMAIN_NAME = "ScriptDM";

	private CSharpEnvironment? _env;

	internal static Func<int, string, CSharpCLR> GetConstructor() => (id, name) => new CSharpCLR(id, name);

	private CSharpCLR(int id, string name) {
		GUID         = Guid.NewGuid();
		InstanceId   = id;
		InstanceName = name;
	}

	public void Construct(object[] args) => _env = new CSharpEnvironment();
	public void Deconstruct()            => _env = null;

	internal void Assemble(byte[] bin) => _env!.Load(bin);

	internal object? ExecuteStatic(string typeFullName, string methodName, params object[] args) => _env!.InvokeStatic(typeFullName, methodName, args);
	internal object? Execute(string       typeFullName, string methodName, params object[] args) => _env!.Invoke(typeFullName, methodName, args);
}

// END FILE
