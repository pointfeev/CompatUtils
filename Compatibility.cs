using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace CompatUtils
{
    /// <summary>RimWorld.<see cref="CompatUtils" /> main class</summary>
    public static class Compatibility
    {
        /// <summary>
        ///     Goes through all installed mods checking for a mod with the specified PackageID and returns whether the mod is
        ///     active.
        /// </summary>
        /// <returns>A <see cref="bool" /> representing whether or not a mod with the specified PackageID is active.</returns>
        public static bool IsModActive(string modPackageId)
            => modPackageId != null && ModLister.AllInstalledMods.Any(mod
                => mod.Active && string.Equals(mod.PackageId, modPackageId, StringComparison.CurrentCultureIgnoreCase));

        /// <summary>
        ///     Goes through all installed mods checking for a mod with the specified PackageID and returns that mod's name if
        ///     found.
        /// </summary>
        /// <returns>
        ///     A <see cref="string" /> representing the name of the mod with the specified PackageID, or <see langword="null" />
        ///     if no mod with the specified PackageID is active.
        /// </returns>
        public static string GetModName(string modPackageId)
            => modPackageId != null
                ? ModLister.AllInstalledMods
                           .FirstOrDefault(mod => mod.Active && string.Equals(mod.PackageId, modPackageId, StringComparison.CurrentCultureIgnoreCase))?.Name
                : null;

        /// <summary>
        ///     Uses <see cref="Harmony" />'s <seealso cref="AccessTools.Method(Type, string, Type[], Type[])" /> to get the
        ///     reflection information for the specified method.
        /// </summary>
        /// <returns>
        ///     The <see cref="MethodInfo" /> of the specified method if found, otherwise <see langword="null" />.
        /// </returns>
        public static MethodInfo GetMethod(string className, string methodName, Type[] parameters = null, Type[] generics = null)
            => AccessTools.Method(AccessTools.TypeByName(className), methodName, parameters, generics);

        /// <summary>
        ///     Uses <see cref="Harmony" />'s <seealso cref="AccessTools.Method(string, Type[], Type[])" /> to get the reflection
        ///     information for the specified method.
        /// </summary>
        /// <returns>
        ///     The <see cref="MethodInfo" /> of the specified method if found, otherwise <see langword="null" />.
        /// </returns>
        public static MethodInfo GetMethod(string typeColonMethodName, Type[] parameters = null, Type[] generics = null)
            => AccessTools.Method(typeColonMethodName, parameters, generics);

        /// <summary>
        ///     Checks all of the specified method's types in order to make sure the method hasn't been changed from your current
        ///     implementation of it.
        ///     <para>
        ///         May print a detailed error message to RimWorld's console if <paramref name="logError" /> is set to
        ///         <see langword="true" />.
        ///     </para>
        ///     <para>
        ///         The detailed error message will also give the name of the mod at fault if
        ///         <paramref name="modPackageIdForLog" /> is specified.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     A <see cref="bool" /> representing whether or not the specified method's types are all the same.
        /// </returns>
        public static bool IsMethodConsistent(MethodInfo methodInfo, Type[] correctMethodTypes, bool logError = false, string modPackageIdForLog = null)
        {
            if (!(methodInfo is object))
                return false;
            Type[] array = (from pi in methodInfo.GetParameters() select !pi.ParameterType.IsByRef ? pi.ParameterType : pi.ParameterType.GetElementType())
               .ToArray();
            if (array.Length != correctMethodTypes.Length)
            {
                if (logError)
                    Log.Error((modPackageIdForLog == null ? "Failed to support a mod: " : "Failed to support " + GetModName(modPackageIdForLog) + ": ")
                            + "Inconsistent number of parameters for method '"
                            + (!(methodInfo.ReflectedType is object) ? "" : methodInfo.ReflectedType.FullName + ".") + methodInfo.Name + "'");
                return false;
            }
            for (int i = 0; i < array.Length; i++)
                if (array[i] != correctMethodTypes[i])
                {
                    if (logError)
                        Log.Error((modPackageIdForLog == null ? "Failed to support a mod: " : "Failed to support " + GetModName(modPackageIdForLog) + ": ")
                                + $"Inconsistent parameter {i + 1} for method '{(!(methodInfo.ReflectedType is object) ? "" : methodInfo.ReflectedType.FullName + ".") + methodInfo.Name}'"
                                + "\n    " + array[i] + " != " + correctMethodTypes[i]);
                    return false;
                }
            return true;
        }

        /// <summary>
        ///     Checks whether a mod is active, and if so runs
        ///     <seealso cref="IsMethodConsistent(MethodInfo, Type[], bool, string)" /> on the specified method.
        ///     <para>
        ///         May print a detailed error message to RimWorld's console if <paramref name="logError" /> is set to
        ///         <see langword="true" />.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     If the specified mod and method both exist and the specified method is consistent, returns the
        ///     <see cref="MethodInfo" /> of the specified method; otherwise returns <see langword="null" />.
        /// </returns>
        private static MethodInfo GetConsistentMethod(string modPackageId, MethodInfo methodInfo, Type[] correctMethodTypes, bool logError = false,
            string methodNameForLog = null)
        {
            if (!IsModActive(modPackageId))
                return null;
            if (methodInfo is object)
                return !IsMethodConsistent(methodInfo, correctMethodTypes, logError, modPackageId) ? null : methodInfo;
            string modName = GetModName(modPackageId);
            if (!logError || modName == null)
                return null;
            Log.Error(
                "Failed to support " + modName + ": Couldn't get " + (methodNameForLog == null ? "an unknown method" : "method " + methodNameForLog) + "!");
            return null;
        }

        /// <summary>
        ///     Checks whether a mod is active, and if so gets the specified method via
        ///     <seealso cref="GetMethod(string, string, Type[], Type[])" /> and runs
        ///     <seealso cref="IsMethodConsistent(MethodInfo, Type[], bool, string)" /> on it.
        ///     <para>May print a detailed error message to RimWorld's console if <paramref name="logError" /> is set to true.</para>
        /// </summary>
        /// <returns>
        ///     If the specified mod and method both exist and the specified method is consistent, returns the
        ///     <see cref="MethodInfo" /> of the specified method; otherwise returns <see langword="null" />.
        /// </returns>
        public static MethodInfo GetConsistentMethod(string modPackageId, string className, string methodName, Type[] correctMethodTypes, bool logError = false)
            => GetConsistentMethod(modPackageId, GetMethod(className, methodName, correctMethodTypes) ?? GetMethod(className, methodName), correctMethodTypes,
                logError, className + "." + methodName);

        /// <summary>
        ///     Checks whether a mod is active, and if so gets the specified method via
        ///     <seealso cref="GetMethod(string, Type[], Type[])" /> and runs
        ///     <seealso cref="IsMethodConsistent(MethodInfo, Type[], bool, string)" /> on it.
        ///     <para>
        ///         May print a detailed error message to RimWorld's console if <paramref name="logError" /> is set to
        ///         <see langword="true" />.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     If the specified mod and method both exist and the specified method is consistent, returns the
        ///     <see cref="MethodInfo" /> of the specified method; otherwise returns <see langword="null" />.
        /// </returns>
        public static MethodInfo GetConsistentMethod(string modPackageId, string typeColonMethodName, Type[] correctMethodTypes, bool logError = false)
            => GetConsistentMethod(modPackageId, GetMethod(typeColonMethodName, correctMethodTypes) ?? GetMethod(typeColonMethodName), correctMethodTypes,
                logError, typeColonMethodName);
    }
}