/* Copyright © 2014 Apex Software. All rights reserved. */

namespace Apex.Editor
{
    using System;
    using Apex.Services;
    using Apex.Steering;
    using Apex.Steering.HeightNavigation;
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(DefaultHeightNavigator), false), CanEditMultipleObjects]
    public class DefaultHeightNavigatorEditor : Editor
    {
        private static readonly OptionsWrapper[] _options = new OptionsWrapper[]
        {
            //0 - box and mesh hm
            new OptionsWrapper
            {
                options = new string[] { "Height Map Three Point", "Height Map Five Point", "Ray Cast Three Point", "Ray Cast Five Point", "Gravity Only", "Custom" },
                descriptions = new string[] { "Samples front, mid and back.\nFast but not very accurate.", "Samples corners and mid.\nReasonably fast but not very accurate.", "Samples front, mid and back.\nMid range performance but more accurate than height map.", "Samples corners and mid.\nMid range performance but more accurate than height map.", "No sampling but gravity is active.\nIf gravity is not required either, remove this component altogether.", "Your own implementation." },
                providerMapping = new ProviderType[] { ProviderType.HeightMapBoxThreePoint, ProviderType.HeightMapBoxFivePoint, ProviderType.RaycastBoxThreePoint, ProviderType.RaycastBoxFivePoint, ProviderType.ZeroHeight, ProviderType.Custom }
            },

            //1 - box and mesh rc
            new OptionsWrapper
            {
                options = new string[] { "Ray Cast Three Point", "Ray Cast Five Point", "Gravity Only", "Custom" },
                descriptions = new string[] { "Samples front, mid and back.\nMid range performance but more accurate than height map.", "Samples corners and mid.\nMid range performance but more accurate than height map.", "No sampling but gravity is active.\nIf gravity is not required either, remove this component altogether.", "Your own implementation." },
                providerMapping = new ProviderType[] { ProviderType.RaycastBoxThreePoint, ProviderType.RaycastBoxFivePoint, ProviderType.ZeroHeight, ProviderType.Custom }
            },

            //2 - sphere and vertical capsule hm
            new OptionsWrapper
            {
                options = new string[] { "Height Map Three Point", "Ray Cast Three Point", "Sphere Cast", "Gravity Only", "Custom" },
                descriptions = new string[] { "Samples front, mid and back.\nFast but not very accurate.", "Samples front, mid and back.\nMid range performance but more accurate than height map.", "Highly accurate sampling, but relatively low performance.", "No sampling but gravity is active.\nIf gravity is not required either, remove this component altogether.", "Your own implementation." },
                providerMapping = new ProviderType[] { ProviderType.HeightMapSphericalThreePoint, ProviderType.RaycastSphericalThreePoint, ProviderType.SphereCast, ProviderType.ZeroHeight, ProviderType.Custom }
            },

            //3 - sphere and vertical capsule rc
            new OptionsWrapper
            {
                options = new string[] { "Ray Cast Three Point", "Sphere Cast", "Gravity Only", "Custom" },
                descriptions = new string[] { "Samples front, mid and back.\nMid range performance but more accurate than height map.", "Highly accurate sampling, but relatively low performance.", "No sampling but gravity is active.\nIf gravity is not required either, remove this component altogether.", "Your own implementation." },
                providerMapping = new ProviderType[] { ProviderType.RaycastSphericalThreePoint, ProviderType.SphereCast, ProviderType.ZeroHeight, ProviderType.Custom }
            },

            //4 - capsule hm 
            new OptionsWrapper
            {
                options = new string[] { "Height Map Three Point", "Ray Cast Three Point", "Capsule Cast", "Gravity Only", "Custom" },
                descriptions = new string[] { "Samples front, mid and back.\nFast but not very accurate.", "Samples front, mid and back.\nMid range performance but more accurate than height map.", "Highly accurate sampling, but relatively low performance.", "No sampling but gravity is active.\nIf gravity is not required either, remove this component altogether.", "Your own implementation." },
                providerMapping = new ProviderType[] { ProviderType.HeightMapSphericalThreePoint, ProviderType.RaycastSphericalThreePoint, ProviderType.CapsuleCast, ProviderType.ZeroHeight, ProviderType.Custom }
            },

            //5 - capsule rc
            new OptionsWrapper
            {
                options = new string[] { "Ray Cast Three Point", "Capsule Cast", "Gravity Only", "Custom" },
                descriptions = new string[] { "Samples front, mid and back.\nMid range performance but more accurate than height map.", "Highly accurate sampling, but relatively low performance.", "No sampling but gravity is active.\nIf gravity is not required either, remove this component altogether.", "Your own implementation." },
                providerMapping = new ProviderType[] { ProviderType.RaycastSphericalThreePoint, ProviderType.CapsuleCast, ProviderType.ZeroHeight, ProviderType.Custom }
            },

            //6 - Other
            new OptionsWrapper
            {
                options = new string[] { "Gravity Only", "Custom" },
                descriptions = new string[] { "No sampling but gravity is active.\nIf gravity is not required either, remove this component altogether.", "Your own implementation." },
                providerMapping = new ProviderType[] { ProviderType.ZeroHeight, ProviderType.Custom }
            }
        };

        private SerializedProperty _gravity;
        private SerializedProperty _groundStickynessFactor;
        private SerializedProperty _terminalVelocity;
        private SerializedProperty _providerType;

        private bool _heightStrategyMissing;

        public override void OnInspectorGUI()
        {
            GUI.enabled = !EditorApplication.isPlaying;

            this.serializedObject.Update();
            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(_gravity);
            EditorGUILayout.PropertyField(_groundStickynessFactor);
            EditorGUILayout.PropertyField(_terminalVelocity);
            EditorGUILayout.Separator();
            DisplayProviderOptions();
            this.serializedObject.ApplyModifiedProperties();

            GUI.enabled = true;
        }

        internal static void EnsureValidProviders(HeightSamplingMode mode)
        {
            var items = Resources.FindObjectsOfTypeAll<DefaultHeightNavigator>();
            for (int i = 0; i < items.Length; i++)
            {
                var dhn = items[i];
                if (dhn == null || dhn.Equals(null) || PrefabUtility.GetPrefabType(dhn) == PrefabType.Prefab)
                {
                    continue;
                }

                var c = dhn.GetComponent<Collider>();
                if (c == null)
                {
                    continue;
                }

                var idx = ResolveOptionsIdx(c, mode);
                var options = _options[idx];
                if (options.FindIndex(dhn.provider) == -1)
                {
                    dhn.provider = options.providerMapping[0];
                }
            }
        }

        internal static void EnsureValidProvider(DefaultHeightNavigator dhn, HeightSamplingMode mode)
        {
            var c = dhn.GetComponent<Collider>();

            var idx = ResolveOptionsIdx(c, mode);
            var options = _options[idx];
            if (options.FindIndex(dhn.provider) == -1)
            {
                dhn.provider = options.providerMapping[0];
            }
        }

        private static int ResolveOptionsIdx(Collider c, HeightSamplingMode activeHeightMode)
        {
            if (activeHeightMode == HeightSamplingMode.NoHeightSampling)
            {
                return 6;
            }

            if (c is CapsuleCollider)
            {
                var cc = c as CapsuleCollider;
                if (cc.direction == 1)
                {
                    return activeHeightMode == HeightSamplingMode.HeightMap ? 2 : 3;
                }

                return activeHeightMode == HeightSamplingMode.HeightMap ? 4 : 5;
            }

            if (c is SphereCollider)
            {
                return activeHeightMode == HeightSamplingMode.HeightMap ? 2 : 3;
            }

            if (c is BoxCollider || c is MeshCollider)
            {
                return activeHeightMode == HeightSamplingMode.HeightMap ? 0 : 1;
            }

            return 6;
        }

        private void DisplayProviderOptions()
        {
            if (_heightStrategyMissing)
            {
                EditorGUILayout.HelpBox("Unable to determine the height sampling mode since this is a prefab.\n Make sure to select a provider that is valid for the scene it is intended to be used in.", MessageType.Warning);
            }

            var label = new GUIContent("Height Sampler", "The height sampler to use, see the description on each below for details.");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, GUILayout.Width(EditorGUIUtility.labelWidth - 10f));

            var activeHeightMode = _heightStrategyMissing ? HeightSamplingMode.HeightMap : GameServices.heightStrategy.heightMode;
            var rootTarget = this.targets[0] as DefaultHeightNavigator;
            var refC = rootTarget.GetComponent<Collider>();
            var optionsIdx = ResolveOptionsIdx(refC, activeHeightMode);
            for (int i = 1; i < targets.Length; i++)
            {
                var b = this.targets[i] as MonoBehaviour;
                var c = b.GetComponent<Collider>();
                var tmpIdx = ResolveOptionsIdx(c, activeHeightMode);
                if (tmpIdx != optionsIdx)
                {
                    EditorGUILayout.LabelField("Multiple objects selected with different collider types.");
                    EditorGUILayout.EndHorizontal();
                    return;
                }
            }

            var options = _options[optionsIdx];
            var selectedIdx = -1;
            if (!_providerType.hasMultipleDifferentValues)
            {
                var selected = (ProviderType)_providerType.intValue;
                selectedIdx = options.FindIndex(selected);
                if (selectedIdx == -1)
                {
                    selectedIdx = 0;
                }
            }

            var newIdx = EditorGUILayout.Popup(selectedIdx, options.options);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.HelpBox(options.descriptions[newIdx], MessageType.Info);

            if (newIdx > -1)
            {
                _providerType.intValue = (int)options.providerMapping[newIdx];
            }
        }

        private void OnEnable()
        {
            _gravity = this.serializedObject.FindProperty("_gravity");
            _groundStickynessFactor = this.serializedObject.FindProperty("groundStickynessFactor");
            _terminalVelocity = this.serializedObject.FindProperty("terminalVelocity");
            _providerType = this.serializedObject.FindProperty("_providerType");

            _heightStrategyMissing = (GameServices.heightStrategy == null);

            for (int i = 0; i < _options.Length; i++)
            {
                var o = _options[i];
                var expected = o.options.Length;

                if (expected != o.providerMapping.Length || expected != o.descriptions.Length)
                {
                    throw new Exception("Options mismatch in DefaultHeightNavigatorEditor");
                }
            }
        }

        private class OptionsWrapper
        {
            internal string[] options;
            internal string[] descriptions;
            internal ProviderType[] providerMapping;

            internal int FindIndex(ProviderType t)
            {
                for (int i = 0; i < providerMapping.Length; i++)
                {
                    if (t == providerMapping[i])
                    {
                        return i;
                    }
                }

                return -1;
            }
        }
    }
}
