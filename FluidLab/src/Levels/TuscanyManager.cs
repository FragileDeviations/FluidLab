using MelonLoader;

using System.Collections;

using UnityEngine;

namespace FluidLab;

public static class TuscanyManager
{
    public static void OnLevelLoaded() 
    {
        if (LevelChecker.IsLevel("Tuscany")) 
        {
            // Create water trigger for the main body of water
            LiquidCreator.CreateLiquid(
                new Vector3(-7.137532f, -215.7f, -224f),
                Quaternion.identity,
                new Vector3(889.0146f, 407.6404f, 669.338f));

            // Create flow triggers near the shore
            var flow = Vector3.down * 1f;

            LiquidCreator.CreateFlow(
                new Vector3(-58.7f, -39.8f, -4.2f),
                Quaternion.Euler(-90f, 0f, 25f),
                new Vector3(123.769f, 37.42f, 56f),
                flow
                );

            LiquidCreator.CreateFlow(
                new Vector3(18.9f, -39.8f, -28.5f),
                Quaternion.Euler(-90f, 0f, -8f),
                new Vector3(67f, 32.82f, 56f),
                flow
                );

            LiquidCreator.CreateFlow(
                new Vector3(122.3f, -39.8f, -50.6f),
                Quaternion.Euler(-90f, 0f, 17f),
                new Vector3(178.11f, 39.35f, 56f),
                flow
                );

            LiquidCreator.CreateFlow(
                new Vector3(-217.4f, -39.8f, -1f),
                Quaternion.Euler(-90f, 0f, -17f),
                new Vector3(78.3f, 27.23f, 56f),
                flow
                );

            // Remove kill volume from water
            MelonCoroutines.Start(CoWaitForDeathTrigger("UNDER WATER KILL VOLUME"));
        }
    }

    private static IEnumerator CoWaitForDeathTrigger(string name) {

        while (LevelChecker.IsLoading())
        {
            yield return null;
        }

        GameObject killVolume;

        while ((killVolume = GameObject.Find(name)) == null)
        {
            yield return null;
        }

        killVolume.SetActive(false);
    }
}