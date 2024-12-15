using BoneLib;

using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Audio;
using Il2CppSLZ.Marrow.Warehouse;
using Il2CppSLZ.VRMK;

using MelonLoader;

using UnityEngine;

namespace FluidLab;

public class FluidLabMod : MelonMod
{
    public const string Version = "1.0.0";

    private VoxelBody[] _playerVoxelBodies = null;

    private VoxelBody _leftHandVoxelBody = null;
    private VoxelBody _rightHandVoxelBody = null;

    private VoxelBody _headVoxelBody = null;

    private AudioSource _headAmbience = null;

    public override void OnInitializeMelon()
    {
        Hooking.OnLevelLoaded += OnLevelLoaded;
        Hooking.OnSwitchAvatarPostfix += OnSwitchAvatarPostfix;
    }

    private void OnLevelLoaded(LevelInfo level)
    {
        TuscanyManager.OnLevelLoaded();
        MagmaGateManager.OnLevelLoaded();
        AscentManager.OnLevelLoaded();
        DungeonWarriorManager.OnLevelLoaded();

        var physicsRig = Player.PhysicsRig;

        _playerVoxelBodies = physicsRig.GetComponentsInChildren<VoxelBody>();

        _leftHandVoxelBody = physicsRig.leftHand.GetComponent<VoxelBody>();
        _rightHandVoxelBody = physicsRig.rightHand.GetComponent<VoxelBody>();

        _headVoxelBody = physicsRig.m_head.GetComponent<VoxelBody>();

        _headVoxelBody.OnEnterLiquid += OnPlayerEnterLiquid;
        _headVoxelBody.OnExitLiquid += OnPlayerExitLiquid;

        _headAmbience = physicsRig.m_head.gameObject.AddComponent<AudioSource>();
        _headAmbience.spatialBlend = 0f;
        _headAmbience.outputAudioMixerGroup = Audio3dManager.ambience;
        _headAmbience.playOnAwake = false;
        _headAmbience.loop = true;
        _headAmbience.volume = 0.2f;
    }

    private void OnPlayerEnterLiquid()
    {
        Audio3dPlugin.Audio3dManager.SetLowPassFilter(1000f);

        if (_headVoxelBody.SubmergedLiquid != null)
        {
            var monoDiscReference = new MonoDiscReference(_headVoxelBody.SubmergedLiquid.AmbienceBarcode);

            if (monoDiscReference.DataCard != null)
            {
                monoDiscReference.DataCard.AudioClip.LoadAsset((Il2CppSystem.Action<AudioClip>)OnAmbienceLoaded);
            }
        }

        void OnAmbienceLoaded(AudioClip clip)
        {
            if (_headAmbience == null)
            {
                return;
            }

            _headAmbience.clip = clip;
            _headAmbience.Play();
        }
    }

    private void OnPlayerExitLiquid()
    {
        Audio3dPlugin.Audio3dManager.SetLowPassFilter(22000f);

        if (_headAmbience != null)
        {
            _headAmbience.Stop();
            _headAmbience.clip = null;
        }
    }

    private void OnSwitchAvatarPostfix(Avatar avatar)
    {
        foreach (var voxelBody in _playerVoxelBodies)
        {
            voxelBody.UnregisterLiquid();

            voxelBody.RecalculateVoxels();
        }
    }

    public override void OnFixedUpdate()
    {
        VoxelBodyManager.OnFixedUpdate();

        if (Player.RigManager != null && _playerVoxelBodies != null)
        {
            OnUpdatePlayer();
        }
    }

    private void OnUpdatePlayer()
    {
        var rig = Player.RigManager;

        float crouch = rig.controllerRig.GetCrouch();

        float buoyancy = Mathf.Clamp01((crouch + 1f) * 0.5f);
        buoyancy *= buoyancy;

        foreach (var body in _playerVoxelBodies)
        {
            body.BuoyancyMultiplier = buoyancy;
        }

        var physicsRig = rig.physicsRig;

        OnUpdateHand(physicsRig.leftHand, _leftHandVoxelBody);
        OnUpdateHand(physicsRig.rightHand, _rightHandVoxelBody);
    }

    private static void OnUpdateHand(Hand hand, VoxelBody body)
    {
        var controllerVelocity = hand.Controller.GetRelativeVelocityInWorld().sqrMagnitude;

        float newDrag = Mathf.Lerp(1f, 50f, controllerVelocity / 4f);
        float smoothDrag = Mathf.Lerp(body.DragMultiplier, newDrag, 1f - Mathf.Exp(-24f * Time.deltaTime));

        body.DragMultiplier = smoothDrag;
    }
}