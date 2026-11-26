// (c)2024-2026 Team Pawouch
// CSharpCLR.cs - 2026-11-27 01:11:17

#nullable enable
using System;
using System.Linq;
using System.Reflection;
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

internal sealed class CSharpEnvironment : MarshalByRefObject {
	private Assembly? _assembly;

	internal CSharpEnvironment() {
		AppDomain.CurrentDomain.AssemblyResolve += (_, e) => {
			string name = new AssemblyName(e.Name).Name;

			return AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(assembly => string.Equals(assembly.GetName().Name, name, StringComparison.OrdinalIgnoreCase));
		};
	}

	internal void Load(byte[] bin) {
		_assembly = Assembly.Load(bin);
	}

	internal object? InvokeStatic(string typeFullName, string methodName, object?[] args) {
		Type?       type   = _assembly?.GetType(typeFullName);
		MethodInfo? method = type?.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
		return method?.Invoke(null, args);
	}

	internal object? Invoke(string typeFullName, string methodName, object?[] args) {
		Type? type = _assembly?.GetType(typeFullName);
		if(type == null) return null;
		object      instance = Activator.CreateInstance(type);
		MethodInfo? method   = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
		return method?.Invoke(instance, args);
	}
}

// END FILE
