using System.Collections.Generic;

using System.Threading.Tasks;

using System;

using MelonLoader;

namespace FluidLab;

public static class VoxelBodyManager
{
    public static HashSet<VoxelBody> VoxelBodies { get; } = new();

    public static void OnFixedUpdate()
    {
        foreach (var body in VoxelBodies)
        {
            try
            {
                body.OnPreFixedUpdate();
            }
            catch (Exception e)
            {
                MelonLogger.Error("Error caught running OnPreFixedUpdate.", e);
            }
        }

        Parallel.ForEach(VoxelBodies, OnParallelFixedUpdate);

        foreach (var body in VoxelBodies)
        {
            try
            {
                body.OnPostFixedUpdate();
            }
            catch (Exception e)
            {
                MelonLogger.Error("Error caught running OnPostFixedUpdate.", e);
            }
        }
    }

    private static void OnParallelFixedUpdate(VoxelBody body)
    {
        try
        {
            body.OnParallelFixedUpdate();
        }
        catch (Exception e)
        {
            MelonLogger.Error("Error caught running OnParallelFixedUpdate.", e);
        }
    }
}
