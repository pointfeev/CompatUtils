Usage
=====
All methods can be accessed via the CompatUtils.Compatibility class:
```c
using CompatUtils;
```
```c
bool combatExtendedActive = Compatibility.IsModActive("ceteam.combatextended");
```
Methods
======
```c
bool IsModActive(string modPackageId)

Summary:
    Goes through all installed mods checking for a mod with the specified PackageID and returns whether the mod is active.

Returns:
    A bool representing whether or not a mod with the specified PackageID is active.
```
```c
string GetModName(string modPackageId)

Summary:
    Goes through all installed mods checking for a mod with the specified PackageID and returns that mod's name if found.

Returns:
    A string representing the name of the mod with the specified PackageID, or null if no mod with the specified PackageID is active.
```
```c
MethodInfo GetMethod(string className, string methodName, Type[] parameters = null, Type[] generics = null)

Summary:
    Uses Harmony's AccessTools to get the reflection information for the specified method.

Returns:
    The MethodInfo of the specified method if found, otherwise null.
```
```c
MethodInfo GetMethod(string typeColonMethodName, Type[] parameters = null, Type[] generics = null)

Summary:
    Uses Harmony's AccessTools to get the reflection information for the specified method.

Returns:
    The MethodInfo of the specified method if found, otherwise null.
```
```c
bool IsMethodConsistent(MethodInfo methodInfo, Type[] correctMethodTypes, bool logError = false, string modPackageIdForLog = null)

Summary:
    Checks all of the specified method's types in order to make sure the method hasn't been changed from your current implementation of it.
    May print a detailed error message to RimWorld's console if logError is set to true.
    The detailed error message will also give the name of the mod at fault if modPackageIdForLog is specified.

Returns:
    A bool representing whether or not the specified method's types are all the same.
```
```c
MethodInfo GetConsistentMethod(string modPackageId, string className, string methodName, Type[] correctMethodTypes, bool logError = false)

Summary:
    Checks whether a mod is active, and if so gets the specified method and runs IsMethodConsistent on it.
    May print a detailed error message to RimWorld's console if logError is set to true.

Returns:
    If the specified mod and method both exist and the specified method is consistent, returns the MethodInfo of the specified method; otherwise returns null.
```
```c
MethodInfo GetConsistentMethod(string modPackageId, string typeColonMethodName, Type[] correctMethodTypes, bool logError = false)

Summary:
    Checks whether a mod is active, and if so gets the specified method and runs IsMethodConsistent on it.
    May print a detailed error message to RimWorld's console if logError is set to true.

Returns:
    If the specified mod and method both exist and the specified method is consistent, returns the MethodInfo of the specified method; otherwise returns null.
```
Examples
======
```c
public static bool combatExtendedActive = Compatibility.IsModActive("ceteam.combatextended");
```
```c
public static MethodInfo combatExtendedHasAmmoMethod = Compatibility.GetMethod("CombatExtended.CE_Utility", "HasAmmo");

public static bool isMethodConsistent = Compatibility.IsMethodConsistent(combatExtendedHasAmmoMethod, new Type[] {
    typeof(ThingWithComps)
}, logError: true, modPackageIdForLog: "ceteam.combatextended");
```
```c
public static MethodInfo combatExtendedHasAmmoMethod = Compatibility.GetConsistentMethod("ceteam.combatextended", "CombatExtended.CE_Utility", "HasAmmo", new Type[] {
    typeof(ThingWithComps)
}, logError: true);
```
```c
public static bool HasAmmo(ThingWithComps thingWithComps)
{
    return combatExtendedHasAmmoMethod is null || (bool)combatExtendedHasAmmoMethod.Invoke(null, new object[] { thingWithComps });
}
```