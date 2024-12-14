using HarmonyLib;

using Il2CppSLZ.Marrow.Interaction;

namespace FluidLab.Patching;

[HarmonyPatch(typeof(MarrowBody))]
public static class MarrowBodyPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(MarrowBody.Awake))]
    public static void Awake(MarrowBody __instance)
    {
        // Add voxel body
        __instance.gameObject.AddComponent<VoxelBody>();
    }
}
