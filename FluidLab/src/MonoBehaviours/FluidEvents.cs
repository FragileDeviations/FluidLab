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

    private void Start()
    {
        var avatar = GetComponent<Avatar>();
        if (avatar != null)
        {
            var avatarRig = avatar.transform.GetParent().GetComponent<RigManager>();
            if (avatarRig == null) return; // if it can't find the rigmanager then it's probably a mirror rig
            _voxelBody = avatarRig.physicsRig.m_footLf.GetComponent<VoxelBody>(); // feet enter the water first
        }
        else
        {
            _voxelBody = GetComponent<VoxelBody>();
        }
        
        if (_voxelBody == null) return;
        _voxelBody.OnEnterLiquid += OnFluidEnter;
        _voxelBody.OnExitLiquid += OnFluidExit;
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