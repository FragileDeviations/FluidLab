using System;

using UnityEngine;

using MelonLoader;

using Il2CppInterop.Runtime.InteropTypes.Fields;
using Il2CppInterop.Runtime.Attributes;

using Il2CppSLZ.Marrow.Pool;
using Il2CppSLZ.Marrow.Data;
using Il2CppSLZ.Marrow.Audio;
using Il2CppSLZ.Marrow.Warehouse;

namespace FluidLab;

[RegisterTypeInIl2Cpp]
public class LiquidVolume : MonoBehaviour
{
    public LiquidVolume(IntPtr intPtr) : base(intPtr) { }

    public Il2CppValueField<float> density;

    public Il2CppValueField<Vector3> center;

    public Il2CppValueField<Vector3> size;

    public Il2CppValueField<Vector3> flow;

    public float Density => density;

    private Vector3 _worldFlow = Vector3.zero;

    public Vector3 Velocity
    {
        get
        {
            return _worldFlow;
        }
    }

    private Vector3 _worldCenter = Vector3.zero;
    private Vector3 _worldSize = Vector3.zero;

    private float _height = 0f;

    public Vector3 WorldCenter => _worldCenter;

    public Vector3 WorldSize => _worldSize;

    public float Height => _height;

    public string AmbienceBarcode { get; set; } = BarcodeReferences.WaterAmbience;

    public bool AmbienceEnabled { get; set; } = true;

    public string SplashVFXBarcode { get; set; } = BarcodeReferences.WaterSplashVFX;

    public string SplashSFXBarcode { get; set; } = BarcodeReferences.WaterSplashSFX;

    public bool SplashEnabled { get; set; } = true;

    [HideFromIl2Cpp]
    public void Splash(Vector3 position, float speed, float size)
    {
        if (!SplashEnabled)
        {
            return;
        }

        // Get position on water's surface
        position.y = Height;

        float speedPercent = speed / 10f;
        float sizePercent = Math.Clamp(size / 0.05f, 0f, 1f);

        float volume = Math.Clamp(speedPercent * sizePercent, 0f, 1f);
        volume *= volume;

        if (volume < 0.05f)
        {
            return;
        }

        SpawnSplashEffect(position, size * Mathf.Lerp(0.2f, 1f, volume));

        // Play sound effect
        var monoDiscReference = new MonoDiscReference(SplashSFXBarcode);

        if (monoDiscReference.DataCard != null)
        {
            monoDiscReference.DataCard.AudioClip.LoadAsset((Il2CppSystem.Action<AudioClip>)OnLoadedSound);
        }

        void OnLoadedSound(AudioClip clip)
        {
            float pitch = Mathf.Pow(UnityEngine.Random.Range(0.7f, 1.2f), volume);

            Audio3dManager.PlayAtPoint(clip, position, Audio3dManager.impact, volume, pitch, new(0f), new(1f), new(1f));
        }
    }

    private void SpawnSplashEffect(Vector3 position, float size)
    {
        var spawnable = new Spawnable()
        {
            crateRef = new(SplashVFXBarcode),
            policyData = null,
        };

        AssetSpawner.Register(spawnable);

        AssetSpawner.Spawn(spawnable, position, Quaternion.identity, new(Vector3.one * size), null, true, new(0), null, null);
    }

    private void LateUpdate()
    {
        var matrix = transform.localToWorldMatrix;

        _worldCenter = matrix.MultiplyPoint3x4(center);
        _worldSize = Vector3.Scale(matrix.lossyScale, size);

        _height = WorldCenter.y + WorldSize.y * 0.5f;

        _worldFlow = transform.rotation * flow;
    }

    public void SetAmbience(string barcode)
    {
        AmbienceBarcode = barcode;
    }

    public void ToggleAmbience(bool enabled)
    {
        AmbienceEnabled = enabled;
    }

    public void SetSplashVFX(string barcode)
    {
        SplashVFXBarcode = barcode;
    }

    public void SetSplashSFX(string barcode)
    {
        SplashSFXBarcode = barcode;
    }

    public void ToggleSplash(bool enabled)
    {
        SplashEnabled = enabled;
    }
}
