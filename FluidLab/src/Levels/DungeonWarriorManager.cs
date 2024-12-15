using UnityEngine;

namespace FluidLab;

public static class DungeonWarriorManager {
    public static readonly LiquidDefinition[] Liquids = new LiquidDefinition[] {
        new(new Vector3(4.916908f, -15.06f, -16.8f), Quaternion.identity, new Vector3(12.74802f, 21.90735f, 49.21357f), CommonDensities.Lava),
        new(new Vector3(31.74236f, -37.95f, -43.49785f), Quaternion.identity, new Vector3(41.24413f, 21.01563f, 30.99791f), CommonDensities.Lava),
        new(new Vector3(53.82556f, -41.3f, -92.16464f), Quaternion.identity, new Vector3(10.99997f, 10.99804f, 23.03027f), CommonDensities.Lava),
        new(new Vector3(53.82556f, -80.91f, -110.8805f), Quaternion.identity, new Vector3(13f, 10.99988f, 22.99988f), CommonDensities.Lava),
    };

    public static void OnLevelLoaded() 
    {
        if (LevelChecker.IsLevel("Dungeon Warrior")) 
        {
            // Create liquid triggers for all lava
            foreach (var liquid in Liquids)
            {
                var lava = LiquidCreator.CreateLiquid(liquid.position, liquid.rotation, liquid.size, liquid.density);

                lava.SplashVFXBarcode = BarcodeReferences.LavaSplashVFX;
                lava.SplashSFXBarcode = BarcodeReferences.LavaSplashSFX;
                lava.AmbienceBarcode = BarcodeReferences.LavaAmbience;
            }
        }
    }
}