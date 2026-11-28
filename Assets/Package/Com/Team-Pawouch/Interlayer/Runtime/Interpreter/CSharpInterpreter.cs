// (c)2024-2026 Team Pawouch
// CSharpInterpreter.cs - 2026-11-26 22:11:20

#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using UnityEngine;

namespace Package.Com.Team_Pawouch.Interlayer.Runtime.Interpreter;

public static class CSharpInterpreter {
	public static byte[]? Interpret(string code, string[] dependencies, Action<bool, string> onCompiled, Action<Exception> onError)
		=> InterpretCore([code], dependencies, null, onCompiled, onError);

	public static byte[]? InterpretProject(string sourceDirectory, string[] dependencies, IEnumerable<byte[]>? dependencyImages, Action<bool, string>? onCompiled = null, Action<Exception>? onError = null) {
		if(!Directory.Exists(sourceDirectory)) {
			onError?.Invoke(new DirectoryNotFoundException("No source directory exists: " + sourceDirectory));
			return null;
		}

		string[] files = Directory.GetFiles(sourceDirectory, "*.cs", SearchOption.AllDirectories);
		if(files.Length == 0) {
			onError?.Invoke(new FileNotFoundException($"No .cs source files in: {sourceDirectory}"));
			return null;
		}

		List<string> codes = [];
		foreach(string file in files) {
			try {
				codes.Add(File.ReadAllText(file));
			}
			catch(Exception ex) {
				onError?.Invoke(ex);
				return null;
			}
		}

		return InterpretCore(codes, dependencies, dependencyImages,
		                     (ok, msg) => {
			                     if(!ok) onError?.Invoke(new Exception(msg));
			                     onCompiled?.Invoke(ok, msg);
		                     },
		                     ex => {
			                     onError?.Invoke(ex);
			                     onError?.Invoke(ex);
		                     });
	}

	private static byte[]? InterpretCore(IEnumerable<string> codes, string[] dependencies, IEnumerable<byte[]>? dependencyImages, Action<bool, string> onCompiled, Action<Exception>? onError) {
		try {
			List<MetadataReference> refs = [];
			HashSet<string>         seen = new(StringComparer.OrdinalIgnoreCase);

			foreach(string dependency in dependencies) {
				if(string.IsNullOrWhiteSpace(dependency)) continue;
				Assembly? asm = Resolve(dependency);
				if(asm == null) {
					onCompiled(false, $"Dependency '{dependency}' not found");
					return null;
				}

				string location = asm.GetLoadedAssemblyPath();
				if(!string.IsNullOrEmpty(location) && File.Exists(location) && seen.Add(location)) refs.Add(MetadataReference.CreateFromFile(location));
			}

			if(dependencyImages != null)
				foreach(byte[] image in dependencyImages)
					if(image is { Length: > 0 })
						refs.Add(MetadataReference.CreateFromImage(image));

			AddCoreReference(typeof(object),     refs, seen);
			AddCoreReference(typeof(Uri),        refs, seen);
			AddCoreReference(typeof(Enumerable), refs, seen);

			SyntaxTree[]      syntaxTrees = [.. codes.Select(c => CSharpSyntaxTree.ParseText(c))];
			CSharpCompilation compilation = CSharpCompilation.Create(Guid.NewGuid().ToString("N"), syntaxTrees, refs, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release));

			using MemoryStream pe     = new();
			EmitResult         result = compilation.Emit(pe);

			if(!result.Success) {
				string errors = string.Join(Environment.NewLine, result.Diagnostics.Where(d => d.Severity is DiagnosticSeverity.Error).Select(d => d.ToString()));
				onCompiled(false, errors);
				return null;
			}

			onCompiled(true, "Compilation succeeded.");
			return pe.ToArray();
		}
		catch(Exception ex) {
			onError?.Invoke(ex);
			return null;
		}

		void AddCoreReference(Type type, List<MetadataReference> localRefs, HashSet<string> localSeen) {
			string location = type.Assembly.GetLoadedAssemblyPath();
			if(!string.IsNullOrEmpty(location) && File.Exists(location) && localSeen.Add(location)) localRefs.Add(MetadataReference.CreateFromFile(location));
		}
	}

	private static Assembly? Resolve(string name) {
		foreach(Assembly assembly in UnityEngine.Assemblies.CurrentAssemblies.GetLoadedAssemblies()) {
			string package = assembly.GetName().Name;
			if(string.Equals(package, name, StringComparison.OrdinalIgnoreCase)) return assembly;
		}

		try {
			return Assembly.Load(name);
		}
		catch {
			try {
				return Assembly.Load(new AssemblyName(name));
			}
			catch {
				return null;
			}
		}
	}
}
