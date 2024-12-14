using System;

using UnityEngine;

#if MELONLOADER
using MelonLoader;

using Il2CppInterop.Runtime.InteropTypes.Fields;
#endif

namespace FluidLab
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#endif
    public class FlowVolume : MonoBehaviour
    {
#if MELONLOADER
        public FlowVolume(IntPtr intPtr) : base(intPtr) { }

        public Il2CppValueField<Vector3> center;

        public Il2CppValueField<Vector3> size;

        public Il2CppValueField<Vector3> flow;
#else
        [Tooltip("The center of the flow bounds.")]
        public Vector3 center = Vector3.zero;

        [Tooltip("The size of the flow bounds.")]
        public Vector3 size = Vector3.one;

        [Tooltip("The additional flow applied to the fluid in m/s, relative to this volume.")]
        public Vector3 flow = Vector3.zero;
#endif

        public Vector3 Velocity
        {
            get
            {
                var transform = this.transform;

                return transform.rotation * flow;
            }
        }

#if UNITY_EDITOR
        public void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;

            Gizmos.color = Color.blue;

            Gizmos.DrawWireCube(center, size);
        }
#endif
    }
}