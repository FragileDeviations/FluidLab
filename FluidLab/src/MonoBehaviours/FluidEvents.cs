using System;
using Il2CppInterop.Runtime.InteropTypes.Fields;
using Il2CppSLZ.Marrow;
using Il2CppSLZ.VRMK;
using Il2CppUltEvents;
using MelonLoader;
using UnityEngine;

namespace FluidLab;

[RegisterTypeInIl2Cpp]
public class FluidEvents : MonoBehaviour
{
    public FluidEvents(IntPtr intPtr) : base(intPtr) { }

    public Il2CppReferenceField<UltEventHolder> onFluidEnteredHolder;
    public Il2CppReferenceField<UltEventHolder> onFluidExitedHolder;
    
    private VoxelBody _voxelBody;
    private VoxelBody _voxelBody2;
    private Avatar _avatar;

    private void Start()
    {
        _avatar = GetComponent<Avatar>();
        
        if (_avatar != null)
        {
            var avatarRig = _avatar.transform.GetParent().GetComponent<RigManager>();
            if (avatarRig == null) return; // if it can't find the rigmanager then it's probably a mirror rig
            _voxelBody = avatarRig.physicsRig._feetRb.GetComponent<VoxelBody>(); // feet enter the water first
            _voxelBody2 = avatarRig.physicsRig.m_footLf.GetComponent<VoxelBody>(); // for when player is ragdolled
        }
        else
        {
            _voxelBody = GetComponent<VoxelBody>();
        }
    }
    
    private void OnEnable()
    {
        if (_voxelBody == null) return;
        _voxelBody.OnEnterLiquid += OnFluidEnter;
        _voxelBody.OnExitLiquid += OnFluidExit;
        if (_voxelBody2 == null) return;
        _voxelBody2.OnEnterLiquid += OnFluidEnter;
        _voxelBody2.OnExitLiquid += OnFluidExit;
    }

    private void OnDisable()
    {
        if (_voxelBody)
        {
            _voxelBody.OnEnterLiquid -= OnFluidEnter;
            _voxelBody.OnExitLiquid -= OnFluidExit;
        }

        if (!_voxelBody2) return;
        _voxelBody2.OnEnterLiquid -= OnFluidEnter;
        _voxelBody2.OnExitLiquid -= OnFluidExit;
    }

    private void OnFluidEnter()
    {
        onFluidEnteredHolder.Get()?.Invoke();
    }
    
    private void OnFluidExit()
    {
        onFluidExitedHolder.Get()?.Invoke();
    }
}