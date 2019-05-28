//Not currently used
//namespace Apex.Tutorials.Editor
//{
//    using UnityEditor;
//    using UnityEngine;

//    [InitializeOnLoad]
//    public class DocumentLink
//    {
//        private static string _currentScene;
//        private static bool _sceneHooked;
//        private static string _docsUrl;

//        static DocumentLink()
//        {
//            EditorApplication.hierarchyWindowChanged += hierarchyWindowChanged;
//        }

//        private static void hierarchyWindowChanged()
//        {
//            if (_currentScene != EditorApplication.currentScene)
//            {
//                _currentScene = EditorApplication.currentScene;

//                var docsUrlSource = Resources.FindObjectsOfTypeAll<DocumentationUrl>();
//                if (docsUrlSource == null || docsUrlSource.Length == 0)
//                {
//                    if (_sceneHooked)
//                    {
//                        SceneView.onSceneGUIDelegate -= OnSceneGUI;
//                        _sceneHooked = false;
//                    }

//                    _docsUrl = null;
//                }
//                else
//                {
//                    _docsUrl = docsUrlSource[0].url;

//                    if (!_sceneHooked)
//                    {
//                        _sceneHooked = true;
//                        SceneView.onSceneGUIDelegate += OnSceneGUI;
//                    }
//                }
//            }
//        }

//        private static void OnSceneGUI(SceneView sceneView)
//        {
//            if (Application.isPlaying)
//            {
//                return;
//            }

//            Handles.BeginGUI();

//            GUILayout.Window(
//            2,
//            new Rect(5f, 20f, 180f, 30f),
//            (id) =>
//            {
//                if (GUILayout.Button("View Documentation"))
//                {
//                    Application.OpenURL(string.Concat("Http://apexgametools.com/", _docsUrl.TrimStart('/')));
//                }
//            },
//            "Documentation Link");

//            Handles.EndGUI();
//        }
//    }
//}
