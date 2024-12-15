using UnityEngine;

namespace FluidLab;

public static class AscentManager 
{
    public static void OnLevelLoaded() 
    {
        if (LevelChecker.IsLevel("13 - Ascent")) 
        {
            // Create liquid triggers for the acid
            var acid = LiquidCreator.CreateLiquid(
                new Vector3(-102.341f, 22.32f, -138.4774f),
                Quaternion.identity,
                new Vector3(22.93945f, 6.998039f, 16.99998f), CommonDensities.Acid);

            acid.SplashVFXBarcode = BarcodeReferences.AcidSplashVFX;
            acid.SplashSFXBarcode = BarcodeReferences.AcidSplashSFX;
            acid.AmbienceBarcode = BarcodeReferences.LavaAmbience;
        }
    }
}