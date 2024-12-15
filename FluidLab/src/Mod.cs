using BoneLib;

using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Audio;
using Il2CppSLZ.VRMK;

using MelonLoader;
using UnityEngine;
using static Il2CppSLZ.Bonelab.Feedback_Audio;

namespace FluidLab;

public class FluidLabMod : MelonMod
{
    public const string Version = "1.0.0";

    private VoxelBody[] _playerVoxelBodies = null;

    private VoxelBody _leftHandVoxelBody = null;
    private VoxelBody _rightHandVoxelBody = null;

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

        var headVoxelBody = physicsRig.m_head.GetComponent<VoxelBody>();

        headVoxelBody.OnEnterLiquid += OnPlayerEnterLiquid;
        headVoxelBody.OnExitLiquid += OnPlayerExitLiquid;
    }

    private void OnPlayerEnterLiquid()
    {
        Audio3dPlugin.Audio3dManager.SetLowPassFilter(1000f);
    }

    private void OnPlayerExitLiquid()
    {
        Audio3dPlugin.Audio3dManager.SetLowPassFilter(22000f);
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