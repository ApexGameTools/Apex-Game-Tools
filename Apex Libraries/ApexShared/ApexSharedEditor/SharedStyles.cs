/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.Editor
{
    using UnityEditor;
    using UnityEngine;

    public static class SharedStyles
    {
        public const int defaultFontSize = 11;

        public static readonly GUIContent deleteTooltip = new GUIContent(string.Empty, "Delete");
        public static readonly GUIContent addTooltip = new GUIContent(string.Empty, "Add");
        public static readonly GUIContent selectTooltip = new GUIContent(string.Empty, "Select");
        public static readonly GUIContent changeSelectionTooltip = new GUIContent(string.Empty, "Change Selection");

        public static readonly Color defaultTextColor = new Color(235f / 255f, 235f / 255f, 235f / 255f, 0.75f);
        public static readonly GUIStyle smallButtonBase = new GUIStyle()
        {
            fixedHeight = 16f,
            fixedWidth = 16f,
            stretchWidth = false
        };

        private static BuiltInStyles _builtIn;

        public static BuiltInStyles BuiltIn
        {
            get
            {
                if (_builtIn == null)
                {
                    _builtIn = new BuiltInStyles();
                }

                return _builtIn;
            }
        }

        public sealed class BuiltInStyles
        {
            internal BuiltInStyles()
            {
                var unitySkinTextColor = EditorGUIUtility.isProSkin ? defaultTextColor : Color.black;
                normalText = new GUIStyle(EditorStyles.label)
                {
                    alignment = TextAnchor.MiddleLeft,
                    fontSize = defaultFontSize,
                    wordWrap = false
                };

                normalText.normal.textColor = unitySkinTextColor;

                header = new GUIStyle(normalText)
                {
                    fontStyle = FontStyle.Bold
                };

                objectSelectorBoxNormal = new GUIStyle(EditorStyles.label)
                {
                    alignment = TextAnchor.MiddleLeft,
                    wordWrap = false
                };

                objectSelectorBoxNormal.normal.textColor = unitySkinTextColor;
                objectSelectorBoxNormal.padding = new RectOffset(objectSelectorBoxNormal.padding.left + 5, objectSelectorBoxNormal.padding.right, objectSelectorBoxNormal.padding.top, objectSelectorBoxNormal.padding.bottom);

                objectSelectorBoxActive = new GUIStyle("MeTransitionSelectHead")
                {
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleLeft,
                    wordWrap = false
                };

                objectSelectorBoxActive.normal.textColor = defaultTextColor;
                objectSelectorBoxActive.padding = new RectOffset(objectSelectorBoxActive.padding.left + 5, objectSelectorBoxActive.padding.right, objectSelectorBoxActive.padding.top, objectSelectorBoxActive.padding.bottom);

                searchFieldStyle = new GUIStyle("SearchTextField");
                searchFieldStyle.normal.textColor = unitySkinTextColor;

                searchCancelButton = new GUIStyle("SearchCancelButton");
                searchCancelButtonEmpty = new GUIStyle("SearchCancelButtonEmpty");

                centeredText = new GUIStyle(normalText)
                {
                    alignment = TextAnchor.MiddleCenter
                };

                wrappedText = new GUIStyle(normalText)
                {
                    wordWrap = true
                };

                centeredWrappedText = new GUIStyle(normalText)
                {
                    alignment = TextAnchor.MiddleCenter,
                    wordWrap = true
                };

                listItemHeaderIndented = new GUIStyle()
                {
                    padding = new RectOffset(30, 0, 0, 4),
                    overflow = new RectOffset(2, 2, 2, 0),
                    wordWrap = false
                };

                listItemHeader = new GUIStyle()
                {
                    padding = new RectOffset(0, 0, 0, 4),
                    overflow = new RectOffset(2, 2, 2, 0),
                    wordWrap = false
                };

                deleteButtonSmall = new GUIStyle(smallButtonBase);

                addButtonSmall = new GUIStyle(smallButtonBase);

                changeButtonSmall = new GUIStyle(smallButtonBase);

                selectButtonSmall = (GUIStyle)"ObjectField"; // new GUIStyle(smallButtonBase);

                deleteButtonSmall.normal.background = UIResources.DeleteButtonSmall.texture;
                addButtonSmall.normal.background = UIResources.AddButtonSmall.texture;
                changeButtonSmall.normal.background = UIResources.ChangeButtonSmall.texture;
                listItemHeaderIndented.normal.background = UIResources.ListItemHeaderBackground.texture;
                listItemHeader.normal.background = UIResources.ListItemHeaderBackground.texture;
            }

            public GUIStyle normalText
            {
                get;
                private set;
            }

            public GUIStyle header
            {
                get;
                private set;
            }

            public GUIStyle objectSelectorBoxNormal
            {
                get;
                private set;
            }

            public GUIStyle objectSelectorBoxActive
            {
                get;
                private set;
            }

            public GUIStyle searchFieldStyle
            {
                get;
                private set;
            }

            public GUIStyle searchCancelButton
            {
                get;
                private set;
            }

            public GUIStyle searchCancelButtonEmpty
            {
                get;
                private set;
            }

            public GUIStyle centeredText
            {
                get;
                private set;
            }

            public GUIStyle wrappedText
            {
                get;
                private set;
            }

            public GUIStyle centeredWrappedText
            {
                get;
                private set;
            }

            public GUIStyle listItemHeaderIndented
            {
                get;
                private set;
            }

            public GUIStyle listItemHeader
            {
                get;
                private set;
            }

            public GUIStyle deleteButtonSmall
            {
                get;
                private set;
            }

            public GUIStyle addButtonSmall
            {
                get;
                private set;
            }

            public GUIStyle changeButtonSmall
            {
                get;
                private set;
            }

            public GUIStyle selectButtonSmall
            {
                get;
                private set;
            }
        }
    }
}
