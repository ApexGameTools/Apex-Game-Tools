/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Editor
{
    using Apex.Services;
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(GameServicesInitializerComponent), false)]
    public class GameServicesInitializerComponentEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("This component initializes key services for use with Apex Products.", MessageType.Info);
        }
    }
}
