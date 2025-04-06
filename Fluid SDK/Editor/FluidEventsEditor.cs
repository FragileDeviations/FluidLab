using System;
using UltEvents;
using UnityEditor;
using SLZ.VRMK;

using UnityEngine;

namespace FluidLab
{
    [CustomEditor(typeof(FluidEvents))]
    public class FluidEventsEditor : Editor
    {
        private SerializedProperty _onFluidEnteredHolder;
        private SerializedProperty _onFluidExitedHolder;
        
        private void OnEnable()
        {
            _onFluidEnteredHolder = serializedObject.FindProperty(nameof(FluidEvents.onFluidEnteredHolder));
            _onFluidExitedHolder = serializedObject.FindProperty(nameof(FluidEvents.onFluidExitedHolder));
        }
        
        public override void OnInspectorGUI()
        {
            var events = target as FluidEvents;
            var avatar = events.GetComponent<SLZ.VRMK.Avatar>();
            var rigidBody = events.GetComponent<Rigidbody>();
            
            if (!rigidBody && !avatar)
            {
                EditorGUILayout.HelpBox("This Fluid Event is missing an avatar or a rigidbody! Please add one!", MessageType.Error);
                return;
            }

            EditorGUI.BeginChangeCheck();
            
            EditorGUILayout.PropertyField(_onFluidEnteredHolder);
            EditorGUILayout.PropertyField(_onFluidExitedHolder);
            
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}