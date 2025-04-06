using System;
using UltEvents;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FluidLab
{
    [AddComponentMenu("Fluids/Fluid Events")]
    public class FluidEvents : MonoBehaviour
    {
        public UltEventHolder onFluidEnteredHolder;
        public UltEventHolder onFluidExitedHolder;
    
        private void Start()
        {
        
        }

        private void OnFluidEnter()
        {
            onFluidEnteredHolder.Invoke();
        }
    
        private void OnFluidExit()
        {
            onFluidExitedHolder.Invoke();
        }
    }
}