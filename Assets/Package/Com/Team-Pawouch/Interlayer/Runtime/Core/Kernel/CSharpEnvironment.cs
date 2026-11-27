// (c)2024-2026 Team Pawouch
// CSharpEnvironment.cs - 2026-11-27 18:11:44

#nullable enable
using System;
using System.Linq;
using System.Reflection;

namespace Package.Com.Team_Pawouch.Interlayer.Runtime.Core.Kernel;

internal sealed class CSharpEnvironment : MarshalByRefObject {
	private Assembly? _assembly;

	internal CSharpEnvironment() {
		AppDomain.CurrentDomain.AssemblyResolve += (_, e) => {
			string name = new AssemblyName(e.Name).Name;

			return UnityEngine.Assemblies.CurrentAssemblies.GetLoadedAssemblies().FirstOrDefault(assembly => string.Equals(assembly.GetName().Name, name, StringComparison.OrdinalIgnoreCase));
		};
	}

	internal void Load(byte[] bin) {
		_assembly = UnityEngine.Assemblies.CurrentAssemblies.LoadFromBytes(bin);
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
