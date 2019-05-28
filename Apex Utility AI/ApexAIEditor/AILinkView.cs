/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI.Editor
{
    using System;
    using UnityEngine;

    public sealed class AILinkView : TopLevelView
    {
        private Guid _aiId;
        private string _aiName;

        internal AILinkView()
        {
        }

        internal AILinkView(Guid aiId, Rect viewArea)
            : base(viewArea)
        {
            _aiId = aiId;
        }

        internal Guid aiId
        {
            get
            {
                return _aiId;
            }

            set
            {
                _aiId = value;
                _aiName = null;
            }
        }

        internal string title
        {
            get
            {
                if (string.IsNullOrEmpty(this.name))
                {
                    return "AI Link";
                }

                return this.name;
            }
        }

        internal string aiName
        {
            get
            {
                if (_aiName == null)
                {
                    var aiStorage = StoredAIs.GetById(aiId.ToString());
                    if (aiStorage != null)
                    {
                        _aiName = aiStorage.name;
                    }
                    else
                    {
                        _aiName = "Missing";
                    }
                }

                return _aiName;
            }
        }

        internal void Refresh()
        {
            _aiName = null;
        }

        public override string ToString()
        {
            return this.title;
        }

        internal override void RecalcHeight(ScaleSettings scaling)
        {
            var h = scaling.titleHeight + scaling.aiLinkBodyHeight;

            viewArea.height = h;
        }
    }
}
