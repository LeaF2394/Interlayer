#if UNITY_EDITOR

// (c)2024-2026 Team Pawouch
// EditorIO.cs - 2026-11-26 23:11:05

#nullable enable
using System;
using System.Globalization;
using JetBrains.Annotations;
using UnityEngine;

namespace Package.Com.Team_Pawouch.Interlayer.Editor;

internal static class EditorIO {
	[HideInCallstack, StringFormatMethod("format")]
	internal static void Info(string format, params object[] args) => Debug.Log($"<color=#{ColorUtility.ToHtmlStringRGB(Color.gray6)}>{DateTime.UtcNow.ToString("yyyy MMMM dd - HH:mm:ss.fff", CultureInfo.InvariantCulture)}</color> <color=#{ColorUtility.ToHtmlStringRGB(Color.cornflowerBlue)}>[info]</color> {string.Format(format, args)}");

	[HideInCallstack, StringFormatMethod("format")]
	internal static void Warn(string format, params object[] args) => Debug.LogWarning($"<color=#{ColorUtility.ToHtmlStringRGB(Color.gray6)}>{DateTime.UtcNow.ToString("yyyy MMMM dd - HH:mm:ss.fff", CultureInfo.InvariantCulture)}</color> <color=#{ColorUtility.ToHtmlStringRGB(Color.coral)}>[warn]</color> {string.Format(format, args)}");

	[HideInCallstack, StringFormatMethod("format")]
	internal static void Error(string format, params object[] args) => Debug.LogError($"<color=#{ColorUtility.ToHtmlStringRGB(Color.gray6)}>{DateTime.UtcNow.ToString("yyyy MMMM dd - HH:mm:ss.fff", CultureInfo.InvariantCulture)}</color> <color=#{ColorUtility.ToHtmlStringRGB(Color.crimson)}>[error]</color> {string.Format(format, args)}");

	[HideInCallstack, StringFormatMethod("format")]
	internal static void Assert(bool condition, string format, params object[] args) => Debug.Assert(condition, $"<color=#{ColorUtility.ToHtmlStringRGB(Color.gray6)}>{DateTime.UtcNow.ToString("yyyy MMMM dd - HH:mm:ss.fff", CultureInfo.InvariantCulture)}</color> <color=#{ColorUtility.ToHtmlStringRGB(Color.deepPink)}>[assert]</color> {string.Format(format, args)}");

	[HideInCallstack, StringFormatMethod("format")]
	internal static void Exception(Exception ex) => Debug.LogError($"<color=#{ColorUtility.ToHtmlStringRGB(Color.gray6)}>{DateTime.UtcNow.ToString("yyyy MMMM dd - HH:mm:ss.fff", CultureInfo.InvariantCulture)}</color> <color=#{ColorUtility.ToHtmlStringRGB(Color.tomato)}>[except]</color> {ex}");
}

// END FILE
#endif
