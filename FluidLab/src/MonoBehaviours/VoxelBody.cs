using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine; 

#if MELONLOADER
using MelonLoader;

using Il2CppSLZ.Marrow.Interaction;

using Il2CppInterop.Runtime.Attributes;

using SystemVector3 = System.Numerics.Vector3;
using SystemQuaternion = System.Numerics.Quaternion;
#endif

namespace FluidLab
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#endif
    public class VoxelBody : MonoBehaviour
    {
#if MELONLOADER
        public VoxelBody(IntPtr intPtr) : base(intPtr) { }

        private MarrowBody _marrowBody;

        public Rigidbody Body => _marrowBody._rigidbody;
#endif

        [Serializable]
        public class VoxelLevel
        {
            public Voxel[] voxels;
            public SystemVector3 voxelSize;
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

        public float DragMultiplier { get; set; } = 1f;

        [HideFromIl2Cpp]
        public event Action OnEnterLiquid, OnExitLiquid;

        private void Awake()
        {
#if MELONLOADER
            _marrowBody = GetComponent<MarrowBody>();
#endif

            RecalculateVoxels();
        }

        [HideFromIl2Cpp]
        private Collider[] GetColliders()
        {
#if MELONLOADER
            return _marrowBody.Colliders;
#else
            return GetComponentsInChildren<Collider>();
#endif
        }

        public void RecalculateVoxels()
        {
            int voxelCount = 128 / _marrowBody.Entity.Bodies.Length;

            _voxelLevel = CutIntoVoxels(voxelCount);
        }

        private LiquidVolume _activeLiquid = null;
        private FlowVolume _activeFlow = null;

        private void OnTriggerEnter(Collider other)
        {
            CheckLiquidEnter(other);
            CheckFlowEnter(other);
        }

        private void CheckLiquidEnter(Collider other)
        {
            var liquidVolume = other.GetComponent<LiquidVolume>();

            if ((liquidVolume == _activeLiquid || _activeLiquid == null) && liquidVolume != null)
            {
                _liquidCount++;

                if (_liquidCount == 1)
                {
                    RegisterLiquid(liquidVolume);
                }
            }
        }

        private void CheckFlowEnter(Collider other)
        {
            var flowVolume = other.GetComponent<FlowVolume>();

            if ((flowVolume == _activeFlow || _activeFlow == null) && flowVolume != null)
            {
                _flowCount++;

                if (_flowCount == 1)
                {
                    _activeFlow = flowVolume;
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
            if (!_activeLiquid)
            {
                return;
            }

            if (other.gameObject == _activeLiquid.gameObject)
            {
                _liquidCount--;

                if (_liquidCount <= 0)
                {
                    UnregisterLiquid();
                }
            }
        }

        private void CheckFlowExit(Collider other)
        {
            if (!_activeFlow)
            {
                return;
            }

            if (other.gameObject == _activeFlow.gameObject)
            {
                _flowCount--;

                if (_flowCount <= 0)
                {
                    _activeFlow = null;
                    _flowCount = 0;
                }
            }
        }

        public void RegisterLiquid(LiquidVolume liquid)
        {
            _activeLiquid = liquid;

            OnEnterLiquid?.Invoke();
        }

        public void UnregisterLiquid()
        {
            _activeLiquid = null;

            _liquidCount = 0;

            OnExitLiquid?.Invoke();
        }

        private void OnDisable()
        {
            UnregisterLiquid();

            _activeFlow = null;
            _flowCount = 0;

            VoxelBodyManager.VoxelBodies.Remove(this);
        }

        private void OnEnable()
        {
            UnregisterLiquid();

            _activeFlow = null;
            _flowCount = 0;

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

        public void OnPreFixedUpdate()
        {
            _validUpdate = true;

            if (!_marrowBody.HasRigidbody)
            {
                _validUpdate = false;
                return;
            }

            if (!_activeLiquid)
            {
                _validUpdate = false;
                return;
            }

            if (Body.IsSleeping())
            {
                _validUpdate = false;
                return;
            }

            _mass = Body.mass;

            _gravity = ToSystemVector3(Physics.gravity);

            var unityRotation = transform.rotation;

            _bodyPosition = ToSystemVector3(transform.position);
            _bodyRotation = new SystemQuaternion(unityRotation.x, unityRotation.y, unityRotation.z, unityRotation.w);

            _bodyCenterOfMass = ToSystemVector3(Body.worldCenterOfMass);
            _bodyVelocity = ToSystemVector3(Body.velocity);
            _bodyAngularVelocity = ToSystemVector3(Body.angularVelocity);

            _liquidDensity = _activeLiquid.Density;
            _liquidVelocity = ToSystemVector3(_activeLiquid.Velocity);
            _liquidHeight = _activeLiquid.Height;

            if (_activeFlow != null)
            {
                _flowVelocity = ToSystemVector3(_activeFlow.Velocity);
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

            // Buoyancy
            var voxelVolume = voxelSize.X * voxelSize.Y * voxelSize.Z;

            float fluidDensity = _liquidDensity;
            var fluidVelocity = _liquidVelocity + _flowVelocity;

            foreach (var voxel in voxels)
            {
                var worldVoxel = SystemVector3.Transform(voxel.position, _bodyRotation) + _bodyPosition;

                float voxelHeight = worldVoxel.Y - voxelSize.Y / 2f;
                float liquidHeight = _liquidHeight;

                float submergedHeight = liquidHeight - voxelHeight;
                submergedHeight = Math.Clamp(submergedHeight / voxelSize.Y, 0f, 1f);

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
        }

        [HideFromIl2Cpp]
        private void SolveFluidDrag(SystemVector3 fluidVelocity, float fluidDensity, VoxelLevel voxelLevel)
        {
            var voxelSize = voxelLevel.voxelSize;
        
            // Drag
            var projectedVoxels = GetProjectedVoxels(SystemVector3.Normalize(fluidVelocity - _bodyVelocity), voxelLevel);
            var voxelArea = GetVoxelArea(voxelLevel.voxelSize);
            var dragCoefficient = 0.5f;

            foreach (var voxel in projectedVoxels)
            {
                var worldVoxel = SystemVector3.Transform(voxel.position, _bodyRotation) + _bodyPosition;

                float voxelHeight = worldVoxel.Y - voxelSize.Y / 2f;
                float liquidHeight = _liquidHeight;
        
                float submergedHeight = liquidHeight - voxelHeight;
                submergedHeight = Math.Clamp(submergedHeight / voxelSize.Y, 0f, 1f);
        
                if (submergedHeight <= 0f)
                {
                    continue;
                }

                var voxelVelocity = GetPointVelocity(_bodyCenterOfMass, _bodyVelocity, _bodyAngularVelocity, worldVoxel);
                var flowVelocity = fluidVelocity - voxelVelocity;
        
                var flowSquared = flowVelocity.LengthSquared();
        
                var dynamicPressure = fluidDensity * flowSquared * 0.5f;
        
                var decay = dragCoefficient * dynamicPressure * voxelArea * submergedHeight;
        
                var drag = 2f * (1f - (float)Math.Exp(-decay));
        
                var dragForce = drag * DragMultiplier * _mass * flowVelocity;

                _totalForce += dragForce;
                _centerOfForce += worldVoxel;
                _forceCount++;
            }
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

#if UNITY_EDITOR
        public void OnDrawGizmosSelected()
        {
            var voxelLevel = GetVoxels(0);

            if (voxelLevel == null || _activeLiquid == null)
            {
                return;
            }

            var matrix = transform.localToWorldMatrix;
            Gizmos.matrix = matrix;

            foreach (var voxel in voxelLevel.voxels)
            {
                var worldVoxel = matrix.MultiplyPoint3x4(voxel.position - voxelLevel.voxelSize / 2f);

                if (worldVoxel.y < _activeLiquid.GetHeight(worldVoxel))
                {
                    Gizmos.color = Color.green;
                }
                else
                {
                    Gizmos.color = Color.gray;
                }

                Gizmos.DrawWireCube(voxel.position, voxelLevel.voxelSize);
            }
        }
#endif

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
            // Get minimum voxel number
            voxelCount = Math.Max(voxelCount, 4);

            Quaternion initialRotation = transform.rotation;
            transform.rotation = Quaternion.identity;

            Physics.SyncTransforms();

            Bounds bounds = new();
            bool hasBounds = false;

            var colliders = GetColliders();

            foreach (var collider in colliders)
            {
                if (!hasBounds)
                {
                    hasBounds = true;
                    bounds = collider.bounds;
                    continue;
                }

                bounds.Encapsulate(collider.bounds);
            }

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

                        Vector3 point = new(pX, pY, pZ);
                        if (IsPointInsideCollider(colliders, point))
                        {
                            var localPosition = Quaternion.Inverse(transform.rotation) * (point - transform.position);

                            var voxel = new Voxel()
                            {
                                position = new(localPosition.x, localPosition.y, localPosition.z),
                            };
                            voxels.Add(voxel);
                        }
                    }
                }
            }

            transform.rotation = initialRotation;

            Physics.SyncTransforms();

            return new VoxelLevel()
            {
                voxels = voxels.ToArray(),
                voxelSize = new(voxelSize.x, voxelSize.y, voxelSize.z),
            };
        }

        [HideFromIl2Cpp]
        private bool IsPointInsideCollider(Collider[] colliders, Vector3 point)
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
}