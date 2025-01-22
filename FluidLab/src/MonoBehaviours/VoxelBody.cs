using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine; 

using MelonLoader;

using Il2CppSLZ.Marrow.Interaction;

using Il2CppInterop.Runtime.Attributes;

using SystemVector3 = System.Numerics.Vector3;
using SystemQuaternion = System.Numerics.Quaternion;

namespace FluidLab;

[RegisterTypeInIl2Cpp]
public class VoxelBody : MonoBehaviour
{
    public VoxelBody(IntPtr intPtr) : base(intPtr) { }

    private MarrowBody _marrowBody = null;
    private int _colliderCount = 0;

    public Rigidbody Body => _marrowBody._rigidbody;

    [Serializable]
    public class VoxelLevel
    {
        public Voxel[] voxels;

        public SystemVector3 voxelSize;
        public AABB voxelAABB;
    }

    public struct AABB
    {
        public SystemVector3 c1;
        public SystemVector3 c2;
        public SystemVector3 c3;
        public SystemVector3 c4;
        public SystemVector3 c5;
        public SystemVector3 c6;
        public SystemVector3 c7;
        public SystemVector3 c8;

        public AABB(SystemVector3 size)
        {
            var extents = size * 0.5f;
            var width = extents.X;
            var height = extents.Y;
            var depth = extents.Z;

            this.c1 = new SystemVector3(width, height, depth);
            this.c2 = new SystemVector3(-width, height, depth);
            this.c3 = new SystemVector3(width, -height, depth);
            this.c4 = new SystemVector3(width, height, -depth);
            this.c5 = new SystemVector3(-width, -height, depth);
            this.c6 = new SystemVector3(width, -height, -depth);
            this.c7 = new SystemVector3(-width, height, -depth);
            this.c8 = new SystemVector3(-width, -height, -depth);
        }

        public readonly SystemVector3 RotateAABB(SystemQuaternion rotation) 
        {
            var c1 = SystemVector3.Transform(this.c1, rotation);
            var c2 = SystemVector3.Transform(this.c2, rotation);
            var c3 = SystemVector3.Transform(this.c3, rotation);
            var c4 = SystemVector3.Transform(this.c4, rotation);
            var c5 = SystemVector3.Transform(this.c5, rotation);
            var c6 = SystemVector3.Transform(this.c6, rotation);
            var c7 = SystemVector3.Transform(this.c7, rotation);
            var c8 = SystemVector3.Transform(this.c8, rotation);

            var min = SystemVector3.Min(SystemVector3.Min(SystemVector3.Min(SystemVector3.Min(SystemVector3.Min(SystemVector3.Min(SystemVector3.Min(c1, c2), c3), c4), c5), c6), c7), c8);
            var max = SystemVector3.Max(SystemVector3.Max(SystemVector3.Max(SystemVector3.Max(SystemVector3.Max(SystemVector3.Max(SystemVector3.Max(c1, c2), c3), c4), c5), c6), c7), c8);

            return max - min;
        }
    }

    public class VoxelReference
    {
        public Voxel voxel;
        public SystemVector3 overridePosition;
    }

    private VoxelLevel _voxelLevel = null;

    private int _liquidCount = 0;
    private int _flowCount = 0;

    public float BuoyancyMultiplier { get; set; } = 1f;

    public Vector3 ExtraVelocity { get; set; } = Vector3.zero;

    [HideFromIl2Cpp]
    public event Action OnEnterLiquid, OnExitLiquid;

    public LiquidVolume SubmergedLiquid
    {
        get
        {
            return _submergedLiquid;
        }
        set
        {
            // Don't invoke any events if its the same liquid
            if (_submergedLiquid == value)
            {
                return;
            }

            var previousLiquid = _submergedLiquid;

            if (previousLiquid != null)
            {
                previousLiquid.OnDisabledEvent -= OnLiquidDisabled;

                OnExitLiquid?.Invoke();
            }

            var newLiquid = value;

            _submergedLiquid = newLiquid;

            if (newLiquid != null)
            {
                newLiquid.OnDisabledEvent += OnLiquidDisabled;

                OnEnterLiquid?.Invoke();

                if (_marrowBody.HasRigidbody)
                {
                    Splash(newLiquid);
                }
            }
            else
            {
                _liquidCount = 0;
            }
        }
    }

    public FlowVolume SubmergedFlow
    {
        get
        {
            return _submergedFlow;
        }
        set
        {
            // Don't invoke any events if its the same flow
            if (_submergedFlow == value)
            {
                return;
            }

            var previousFlow = _submergedFlow;

            if (previousFlow != null)
            {
                previousFlow.OnDisabledEvent -= OnFlowDisabled;
            }

            var newFlow = value;

            _submergedFlow = newFlow;

            if (newFlow != null)
            {
                newFlow.OnDisabledEvent += OnFlowDisabled;
            }
            else
            {
                _flowCount = 0;
            }
        }
    }

    private void Awake()
    {
        _marrowBody = GetComponent<MarrowBody>();
        _colliderCount = _marrowBody.Colliders.Count;

        RecalculateVoxels();
    }

    [HideFromIl2Cpp]
    private Collider[] GetColliders()
    {
        return _marrowBody.Colliders;
    }

    public void RecalculateVoxels()
    {
        _liquidCount = 0;
        _flowCount = 0;

        int voxelCount = 128 / _marrowBody.Entity.Bodies.Length;

        _voxelLevel = CutIntoVoxels(voxelCount);
    }

    private LiquidVolume _submergedLiquid = null;
    private FlowVolume _submergedFlow = null;

    private void OnTriggerEnter(Collider other)
    {
        CheckLiquidEnter(other);
        CheckFlowEnter(other);
    }

    private void CheckLiquidEnter(Collider other)
    {
        var liquidVolume = other.GetComponent<LiquidVolume>();

        if ((liquidVolume == _submergedLiquid || _submergedLiquid == null) && liquidVolume != null)
        {
            _liquidCount++;
            _liquidCount = Math.Min(_liquidCount, _colliderCount);

            if (_liquidCount == 1)
            {
                SubmergedLiquid = liquidVolume;
            }
        }
    }

    private void CheckFlowEnter(Collider other)
    {
        var flowVolume = other.GetComponent<FlowVolume>();

        if ((flowVolume == SubmergedFlow || SubmergedFlow == null) && flowVolume != null)
        {
            _flowCount++;
            _flowCount = Math.Min(_flowCount, _colliderCount);

            if (_flowCount == 1)
            {
                SubmergedFlow = flowVolume;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        CheckLiquidExit(other);
        CheckFlowExit(other);
    }

    private void CheckLiquidExit(Collider other)
    {
        if (!_submergedLiquid)
        {
            return;
        }

        if (other.gameObject == _submergedLiquid.gameObject)
        {
            _liquidCount--;

            if (_liquidCount <= 0)
            {
                SubmergedLiquid = null;
            }
        }
    }

    private void CheckFlowExit(Collider other)
    {
        if (!SubmergedFlow)
        {
            return;
        }

        if (other.gameObject == SubmergedFlow.gameObject)
        {
            _flowCount--;

            if (_flowCount <= 0)
            {
                SubmergedFlow = null;
            }
        }
    }

    private void OnLiquidDisabled()
    {
        SubmergedLiquid = null;
    }

    private void OnFlowDisabled()
    {
        SubmergedFlow = null;
    }

    private void Splash(LiquidVolume liquid)
    {
        var velocity = Body.velocity;
        float speed = velocity.magnitude;

        if (speed < 2f)
        {
            return;
        }

        var bounds = _marrowBody.Bounds.size;
        var size = (bounds.x + bounds.y + bounds.z) / 3f;

        var position = Body.worldCenterOfMass;

        liquid.Splash(position, speed, size);
    }

    private void OnDisable()
    {
        SubmergedLiquid = null;
        SubmergedFlow = null;

        VoxelBodyManager.VoxelBodies.Remove(this);
    }

    private void OnEnable()
    {
        SubmergedLiquid = null;
        SubmergedFlow = null;

        VoxelBodyManager.VoxelBodies.Add(this);
    }

    private bool _validUpdate = false;

    private SystemVector3 _bodyPosition = SystemVector3.Zero;
    private SystemQuaternion _bodyRotation = SystemQuaternion.Identity;

    private SystemVector3 _bodyCenterOfMass = SystemVector3.Zero;
    private SystemVector3 _bodyVelocity = SystemVector3.Zero;
    private SystemVector3 _bodyAngularVelocity = SystemVector3.Zero;

    private float _liquidDensity = 1f;
    private float _liquidHeight = 0f;
    private SystemVector3 _liquidVelocity = SystemVector3.Zero;
    private SystemVector3 _flowVelocity = SystemVector3.Zero;

    private SystemVector3 _gravity = SystemVector3.Zero;
    private float _mass = 1f;

    private float _fixedDeltaTime = 1f;

    public void OnPreFixedUpdate()
    {
        _validUpdate = true;

        if (!_marrowBody.HasRigidbody)
        {
            _validUpdate = false;
            return;
        }

        if (!_submergedLiquid)
        {
            _validUpdate = false;
            return;
        }

        if (Body.IsSleeping())
        {
            _validUpdate = false;
            return;
        }

        _fixedDeltaTime = Time.fixedDeltaTime;

        _mass = Body.mass;

        _gravity = ToSystemVector3(Physics.gravity);

        var unityRotation = transform.rotation;

        _bodyPosition = ToSystemVector3(transform.position);
        _bodyRotation = new SystemQuaternion(unityRotation.x, unityRotation.y, unityRotation.z, unityRotation.w);

        _bodyCenterOfMass = ToSystemVector3(Body.worldCenterOfMass);
        _bodyVelocity = ToSystemVector3(Body.velocity);
        _bodyAngularVelocity = ToSystemVector3(Body.angularVelocity);

        _liquidDensity = _submergedLiquid.Density;
        _liquidVelocity = ToSystemVector3(_submergedLiquid.Velocity + ExtraVelocity);
        _liquidHeight = _submergedLiquid.Height;

        if (_submergedFlow != null)
        {
            _flowVelocity = ToSystemVector3(_submergedFlow.Velocity);
        }
        else
        {
            _flowVelocity = SystemVector3.Zero;
        }
    }

    private SystemVector3 _totalForce = SystemVector3.Zero;
    private SystemVector3 _centerOfForce = SystemVector3.Zero;
    private int _forceCount = 0;

    [HideFromIl2Cpp]
    public void OnParallelFixedUpdate()
    {
        _totalForce = SystemVector3.Zero;
        _centerOfForce = SystemVector3.Zero;
        _forceCount = 0;

        if (!_validUpdate)
        {
            return;
        }

        var voxelLevel = _voxelLevel;

        var voxels = voxelLevel.voxels;
        var voxelSize = voxelLevel.voxelSize;

        var voxelDepth = voxelLevel.voxelAABB.RotateAABB(_bodyRotation).Y;
        var voxelHalfDepth = voxelDepth * 0.5f;

        // Buoyancy
        var voxelVolume = voxelSize.X * voxelSize.Y * voxelSize.Z;

        float fluidDensity = _liquidDensity;
        var fluidVelocity = _liquidVelocity + _flowVelocity;

        foreach (var voxel in voxels)
        {
            var worldVoxel = SystemVector3.Transform(voxel.position, _bodyRotation) + _bodyPosition;

            var voxelVelocity = GetPointVelocity(_bodyCenterOfMass, _bodyVelocity, _bodyAngularVelocity, worldVoxel);
            worldVoxel += voxelVelocity * _fixedDeltaTime;

            float voxelHeight = worldVoxel.Y - voxelHalfDepth;
            float liquidHeight = _liquidHeight;

            float submergedHeight = liquidHeight - voxelHeight;
            submergedHeight = Math.Clamp(submergedHeight / voxelDepth, 0f, 1f);

            voxel.submersion = submergedHeight;

            if (submergedHeight > 0f)
            {
                float displacedVolume = voxelVolume * submergedHeight;

                var buoyantForce = BuoyancyMultiplier * fluidDensity * displacedVolume * -_gravity;

                _totalForce += buoyantForce;
                _centerOfForce += worldVoxel;
                _forceCount++;
            }
        }

        SolveFluidDrag(fluidVelocity, fluidDensity, voxelLevel);
    }

    public void OnPostFixedUpdate()
    {
        if (_forceCount <= 0)
        {
            return;
        }

        Body.AddForceAtPosition(ToUnityVector3(_totalForce), ToUnityVector3(_centerOfForce / _forceCount));

        // Damping to stabilize angular velocity (especially on liquid surface)
        Body.angularVelocity *= Math.Clamp(1f - (_fixedDeltaTime * 6f), 0f, 1f);
    }

    [HideFromIl2Cpp]
    private void SolveFluidDrag(SystemVector3 fluidVelocity, float fluidDensity, VoxelLevel voxelLevel)
    {
        var voxelSize = voxelLevel.voxelSize;

        // Drag
        var projectedVoxels = GetProjectedVoxels(MathUtilities.NormalizeSafe(fluidVelocity - _bodyVelocity), voxelLevel);
        var voxelArea = GetVoxelArea(voxelSize);
        var dragCoefficient = 0.5f;

        SystemVector3 accumulatedDrag = SystemVector3.Zero;
        SystemVector3 centerOfDrag = SystemVector3.Zero;
        int totalDrag = 0;

        foreach (var voxel in projectedVoxels)
        {
            float submergedHeight = voxel.submersion;

            var worldVoxel = SystemVector3.Transform(voxel.position, _bodyRotation) + _bodyPosition;

            var voxelVelocity = GetPointVelocity(_bodyCenterOfMass, _bodyVelocity, _bodyAngularVelocity, worldVoxel);
            var flowVelocity = fluidVelocity - voxelVelocity;
    
            var dynamicPressure = fluidDensity * flowVelocity.Length() * 0.5f;
    
            var drag = dragCoefficient * dynamicPressure * voxelArea * submergedHeight;
    
            var dragForce = drag * flowVelocity;

            accumulatedDrag += dragForce;
            centerOfDrag += worldVoxel;
            totalDrag++;
        }

        float maxDrag = _bodyVelocity.Length() / _fixedDeltaTime * _mass;
        accumulatedDrag = MathUtilities.NormalizeSafe(accumulatedDrag) * Math.Min(accumulatedDrag.Length(), maxDrag);

        _totalForce += accumulatedDrag;
        _centerOfForce += centerOfDrag;
        _forceCount += totalDrag;
    }

    private static SystemVector3 ToSystemVector3(Vector3 vector)
    {
        return new SystemVector3(vector.x, vector.y, vector.z);
    }

    private static Vector3 ToUnityVector3(SystemVector3 vector)
    {
        return new Vector3(vector.X, vector.Y, vector.Z);
    }

    private static SystemVector3 GetPointVelocity(SystemVector3 worldCenterOfMass, SystemVector3 velocity, SystemVector3 angularVelocity, SystemVector3 point)
    {
        return SystemVector3.Cross(angularVelocity, point - worldCenterOfMass) + velocity;
    }

    [HideFromIl2Cpp]
    private Voxel[] GetProjectedVoxels(SystemVector3 direction, VoxelLevel voxelLevel)
    {
        var voxels = voxelLevel.voxels;
        var voxelSize = voxelLevel.voxelSize;

        var projectionOffset = MathUtilities.FromToRotation(SystemVector3.UnitZ, direction);

        List<VoxelReference> projectedVoxels = new();

        var directionInMatrix = SystemVector3.Transform(direction, SystemQuaternion.Inverse(_bodyRotation)) * 1000f;
        var sortedVoxels = voxels.OrderByDescending((voxel) => (voxel.position - directionInMatrix).LengthSquared());

        foreach (var voxel in sortedVoxels)
        {
            if (voxel.submersion <= 0f)
            {
                continue;
            }

            var worldVoxel = SystemVector3.Transform(voxel.position, _bodyRotation) + _bodyPosition;

            worldVoxel = SystemVector3.Transform(worldVoxel, projectionOffset);
            worldVoxel.Z = 0f;
            worldVoxel = SystemVector3.Transform(worldVoxel, SystemQuaternion.Inverse(projectionOffset));

            projectedVoxels.Add(new VoxelReference()
            {
                voxel = voxel,
                overridePosition = worldVoxel,
            });
        }

        List<VoxelReference> projectedWithoutDupes = new();

        float maxDistanceSqr = voxelSize.LengthSquared() / 2f;

        foreach (var voxel in projectedVoxels)
        {
            if (!projectedWithoutDupes.Exists((nonDupe) => (voxel.overridePosition - nonDupe.overridePosition).LengthSquared() < maxDistanceSqr))
            {
                projectedWithoutDupes.Add(voxel);
            }
        }

        Voxel[] finalVoxels = new Voxel[projectedWithoutDupes.Count];

        for (var i = 0; i < projectedWithoutDupes.Count; i++)
        {
            finalVoxels[i] = projectedWithoutDupes[i].voxel;
        }

        return finalVoxels;
    }

    private static float GetVoxelArea(SystemVector3 voxelSize)
    {
        return Math.Abs(voxelSize.X * voxelSize.Y);
    }

    [HideFromIl2Cpp]
    private VoxelLevel CutIntoVoxels(int voxelCount)
    {
        // Calculating the bounds of bodies with null colliders or no rigidbody can cause weird physics issues
        if (_marrowBody.HasNullColliders() || !_marrowBody.HasRigidbody)
        {
            return new VoxelLevel()
            {
                voxels = Array.Empty<Voxel>(),
                voxelSize = SystemVector3.One,
                voxelAABB = new(SystemVector3.One),
            };
        }

        // Get transform values
        var position = transform.position;
        var rotation = transform.rotation;

        // Get minimum voxel number
        voxelCount = Math.Max(voxelCount, 4);

        _marrowBody.CalculateBounds();

        Bounds bounds = _marrowBody.Bounds;

        var colliders = GetColliders();

        var normalizedVoxelSize = 1f / (float)Math.Cbrt(voxelCount);

        var voxelSize = bounds.size * normalizedVoxelSize;
        int voxelsCountForEachAxis = Mathf.RoundToInt(1f / normalizedVoxelSize);
        List<Voxel> voxels = new(voxelsCountForEachAxis * voxelsCountForEachAxis * voxelsCountForEachAxis);

        for (int i = 0; i < voxelsCountForEachAxis; i++)
        {
            for (int j = 0; j < voxelsCountForEachAxis; j++)
            {
                for (int k = 0; k < voxelsCountForEachAxis; k++)
                {
                    var min = bounds.min;
                    float pX = min.x + voxelSize.x * (0.5f + i);
                    float pY = min.y + voxelSize.y * (0.5f + j);
                    float pZ = min.z + voxelSize.z * (0.5f + k);

                    var localPoint = new Vector3(pX, pY, pZ);
                    var worldPoint = (rotation * localPoint) + position;

                    if (IsPointInsideCollider(colliders, worldPoint))
                    {
                        var voxel = new Voxel()
                        {
                            position = ToSystemVector3(localPoint),
                        };

                        voxels.Add(voxel);
                    }
                }
            }
        }

        var systemVoxelSize = ToSystemVector3(voxelSize);

        return new VoxelLevel()
        {
            voxels = voxels.ToArray(),
            voxelSize = systemVoxelSize,
            voxelAABB = new(systemVoxelSize),
        };
    }

    private static bool IsPointInsideCollider(Collider[] colliders, Vector3 point)
    {
        foreach (var collider in colliders)
        {
            var closestPoint = collider.ClosestPoint(point);

            if (closestPoint == point)
            {
                return true;
            }
        }

        return false;
    }
}