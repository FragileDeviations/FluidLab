using UnityEngine;

namespace FluidLab;

public static class MagmaGateManager 
{
    public static void OnLevelLoaded()
    {
        if (LevelChecker.IsLevel("08 - Magma Gate"))
        {
            // Create liquid trigger for the lava
            LiquidCreator.CreateLiquid(
                new Vector3(1.8f, -28.1f, 23.2f),
                Quaternion.identity,
                new Vector3(360.3596f, 54.99212f, 213.0187f), CommonDensities.Lava);
        }
    }
}