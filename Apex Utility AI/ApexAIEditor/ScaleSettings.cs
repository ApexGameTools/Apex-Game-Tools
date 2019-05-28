/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI.Editor
{
    using UnityEngine;

    internal sealed class ScaleSettings
    {
        internal static readonly ScaleSettings FullScale = new ScaleSettings(1f);
#if UNITY_5 || UNITY_2017
        private const float connectorLineBaseWidth = 4f;
#else
        private const float connectorLineBaseWidth = 2f;
#endif

        private float _scale;
        private UserSettings _settings;

        internal float selectorResizeMargin;
        internal float connectorLineWidth;
        internal float viewMinWidth;

        internal float anchorAreaWidth;
        internal float dragHandleWidth;
        internal float toggleAreaWidth;

        internal ScaleSettings(float currentScale)
        {
            _settings = UserSettings.instance;
            UpdateScale(currentScale);
        }

        internal float scale
        {
            get { return _scale; }
        }

        internal float snapCellSize
        {
            get { return Mathf.Round(_settings.snapCellSize * _scale); }
        }

        internal float titleHeight
        {
            get { return Mathf.Round(_settings.titleHeight * _scale); }
        }

        internal float qualifierHeight
        {
            get { return Mathf.Round(_settings.qualifierHeight * _scale); }
        }

        internal float actionHeight
        {
            get { return Mathf.Round(_settings.actionHeight * _scale); }
        }

        internal float scorerHeight
        {
            get { return Mathf.Round(_settings.scorerHeight * _scale); }
        }

        internal float aiLinkBodyHeight
        {
            get { return Mathf.Round(_settings.qualifierHeight * _scale); }
        }

        internal void UpdateScale(float newScale)
        {
            _scale = newScale;
            selectorResizeMargin = 4f * _scale;
            connectorLineWidth = connectorLineBaseWidth * _scale;
            viewMinWidth = 100f * _scale;

            anchorAreaWidth = 20f * _scale;
            dragHandleWidth = 20f * _scale;
            toggleAreaWidth = 20f * _scale;
        }
    }
}
