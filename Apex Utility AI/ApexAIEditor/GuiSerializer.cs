/* Copyright © 2014 Apex Software. All rights reserved. */
namespace Apex.AI.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Apex.AI.Editor.UndoRedo;
    using Apex.Serialization;
    using UnityEditor;
    using UnityEngine;

    internal static class GuiSerializer
    {
        internal static string Serialize(AICanvas canvas)
        {
            var root = new StageElement(
                ElementName.Canvas,
                SerializationMaster.Stage(ElementName.Offset, canvas.offset),
                SerializationMaster.ToStageValue(ElementName.Zoom, canvas.zoom));

            var views = canvas.views;
            int svCount = views.Count;
            for (int i = 0; i < svCount; i++)
            {
                var view = views[i];
                StageElement viewElement;

                if (view is SelectorView)
                {
                    viewElement = WriteSelectorView((SelectorView)view);
                }
                else if (view is AILinkView)
                {
                    viewElement = WriteAILinkView((AILinkView)view);
                }
                else
                {
                    throw new NotImplementedException("The view type has not been implemented for serialization.");
                }

                root.Add(viewElement);
            }

            return SerializationMaster.Serialize(root);
        }

        internal static string Serialize(IList<TopLevelView> items)
        {
            var aiPart = new StageElement(ElementName.AIPart);
            var uiPart = new StageElement(ElementName.UIPart);
            var root = new StageElement(ElementName.ViewSnippet, aiPart, uiPart);
            root.AddTextAttribute(AttributeName.SnippetType, ElementName.ViewSnippet);

            var referencePos = new Vector2(float.MaxValue, float.MaxValue);
            for (int i = 0; i < items.Count; i++)
            {
                var view = items[i];

                var selectorView = view as SelectorView;
                if (selectorView != null)
                {
                    var item = selectorView.selector;

                    var aiItem = SerializationMaster.Stage(typeof(Selector).Name, item);
                    aiPart.Add(aiItem);

                    var viewItem = WriteSelectorView(selectorView);
                    uiPart.Add(viewItem);

                    //Even though reid also happens on paste we have to do it here as well, otherwise copying linked selectors will fail.
                    item.RegenerateId();
                }

                var linkView = view as AILinkView;
                if (linkView != null)
                {
                    var viewItem = WriteAILinkView(linkView);
                    uiPart.Add(viewItem);
                }

                if (view.viewArea.xMin < referencePos.x)
                {
                    referencePos.x = view.viewArea.xMin;
                }

                if (view.viewArea.yMin < referencePos.y)
                {
                    referencePos.y = view.viewArea.yMin;
                }
            }

            uiPart.AddValue(ElementName.ReferencePosition, referencePos);

            return SerializationMaster.Serialize(root);
        }

        internal static string Serialize(QualifierView qualifier)
        {
            var aiPart = new StageElement(ElementName.AIPart);
            var uiPart = new StageElement(ElementName.UIPart);
            var root = new StageElement(ElementName.QualifierSnippet, aiPart, uiPart);
            root.AddTextAttribute(AttributeName.SnippetType, ElementName.QualifierSnippet);

            var aiItem = SerializationMaster.Stage(ElementName.Qualifier, qualifier.qualifier);
            aiPart.Add(aiItem);

            var viewItem = WriteQualifierView(qualifier);
            uiPart.Add(viewItem);

            return SerializationMaster.Serialize(root);
        }

        internal static string Serialize(ActionView action)
        {
            var aiPart = new StageElement(ElementName.AIPart);
            var uiPart = new StageElement(ElementName.UIPart);
            var root = new StageElement(ElementName.ActionSnippet, aiPart, uiPart);
            root.AddTextAttribute(AttributeName.SnippetType, ElementName.ActionSnippet);

            var aiItem = SerializationMaster.Stage(ElementName.Action, action.action);
            aiPart.Add(aiItem);

            var viewItem = WriteActionView(action);
            uiPart.Add(viewItem);

            return SerializationMaster.Serialize(root);
        }

        internal static AICanvas Deserialize(AIUI host, string data)
        {
            var canvasElement = SerializationMaster.Deserialize(data);

            var canvas = new AICanvas
            {
                offset = canvasElement.ValueOrDefault<Vector2>(ElementName.Offset),
                zoom = canvasElement.ValueOrDefault(ElementName.Zoom, 1f)
            };

            var selectorViews = canvasElement.Elements(ElementName.SelectorView);
            foreach (var sve in selectorViews)
            {
                var sv = ReadSelectorView(sve, host);
                canvas.views.Add(sv);
            }

            var linkViews = canvasElement.Elements(ElementName.AILinkView);
            foreach (var lve in linkViews)
            {
                var lv = ReadAILinkView(lve, host);
                canvas.views.Add(lv);
            }

            return canvas;
        }

        internal static void DeserializeSnippet(string data, AIUI targetUI, Vector2 mousePos, bool regenIds)
        {
            var root = SerializationMaster.Deserialize(data);
            var snippetType = root.AttributeValue<string>(AttributeName.SnippetType);
            var aiElements = root.Element(ElementName.AIPart).Elements().ToArray();

            if (snippetType == ElementName.ViewSnippet)
            {
                var uiPart = root.Element(ElementName.UIPart);
                var referencePos = uiPart.ValueOrDefault(ElementName.ReferencePosition, Vector2.zero);

                var newViews = new List<TopLevelView>(uiPart.Items().Count());

                using (targetUI.undoRedo.bulkOperation)
                {
                    //Deserialize links first so that connections are not lost
                    var linkElements = uiPart.Elements(ElementName.AILinkView).ToArray();
                    DeserializeAILinkViews(linkElements, targetUI, referencePos, mousePos, newViews);

                    var selectorElements = uiPart.Elements(ElementName.SelectorView).ToArray();
                    DeserializeSelectorViews(aiElements, selectorElements, targetUI, referencePos, mousePos, newViews, regenIds);
                }

                targetUI.MultiSelectViews(newViews);
                return;
            }

            var viewElement = root.Element(ElementName.UIPart).Elements().First();
            if (snippetType == ElementName.QualifierSnippet)
            {
                DeserializeQualifier(aiElements[0], viewElement, targetUI);
            }
            else if (snippetType == ElementName.ActionSnippet)
            {
                DeserializeAction(aiElements[0], viewElement, targetUI);
            }
        }

        private static AILinkView ReadAILinkView(StageElement lve, AIUI parent)
        {
            var lv = new AILinkView
            {
                name = lve.ValueOrDefault<string>(ElementName.Name),
                description = lve.ValueOrDefault<string>(ElementName.Description),
                viewArea = lve.ValueOrDefault<Rect>(ElementName.ViewArea),
                aiId = lve.AttributeValue<Guid>(AttributeName.Id),
                parent = parent
            };

            return lv;
        }

        private static SelectorView ReadSelectorView(StageElement sve, AIUI parent)
        {
            var sv = new SelectorView
            {
                name = sve.ValueOrDefault<string>(ElementName.Name),
                description = sve.ValueOrDefault<string>(ElementName.Description),
                viewArea = sve.ValueOrDefault<Rect>(ElementName.ViewArea),
                parent = parent
            };

            var defQv = sve.Element(ElementName.DefaultQualifier);
            sv.defaultQualifierView = defQv != null ? ReadQualifierView(defQv, sv) : new QualifierView { parent = sv };

            var qualifierViews = sve.Elements(ElementName.QualifierView);
            foreach (var qve in qualifierViews)
            {
                var qv = ReadQualifierView(qve, sv);
                sv.qualifierViews.Add(qv);
            }

            return sv;
        }

        private static QualifierView ReadQualifierView(StageElement qve, SelectorView parent)
        {
            var qv = new QualifierView
            {
                name = qve.ValueOrDefault<string>(ElementName.Name, null),
                description = qve.ValueOrDefault<string>(ElementName.Description, null),
                parent = parent
            };

            var ave = qve.Element(ElementName.ActionView);
            if (ave != null)
            {
                qv.actionView = ReadActionView(ave, qv);
            }

            return qv;
        }

        private static ActionView ReadActionView(StageElement ave, QualifierView parent)
        {
            var connectorType = ave.AttributeValueOrDefault<string>(AttributeName.ConnectorType, null);

            ActionView av;
            if (connectorType == ConnectorType.Selector)
            {
                av = new SelectorActionView();
            }
            else if (connectorType == ConnectorType.AILink)
            {
                av = new AILinkActionView();
            }
            else if (connectorType == ConnectorType.Composite)
            {
                av = new CompositeActionView();
            }
            else
            {
                av = new ActionView();
            }

            av.name = ave.ValueOrDefault<string>(ElementName.Name, null);
            av.description = ave.ValueOrDefault<string>(ElementName.Description, null);
            av.parent = parent;

            return av;
        }

        private static StageElement WriteAILinkView(AILinkView lv)
        {
            var lve = new StageElement(
                ElementName.AILinkView,
                SerializationMaster.Stage(ElementName.ViewArea, lv.viewArea),
                SerializationMaster.ToStageAttribute(AttributeName.Id, lv.aiId));

            lve.AddTextValue(ElementName.Name, lv.name);
            lve.AddTextValue(ElementName.Description, lv.description);

            return lve;
        }

        private static StageElement WriteSelectorView(SelectorView sv)
        {
            var sve = new StageElement(
                ElementName.SelectorView,
                SerializationMaster.Stage(ElementName.ViewArea, sv.viewArea));

            sve.AddTextValue(ElementName.Name, sv.name);
            sve.AddTextValue(ElementName.Description, sv.description);

            var qualifierViews = sv.qualifierViews;
            int qvCount = qualifierViews.Count;
            for (int i = 0; i < qvCount; i++)
            {
                var qv = WriteQualifierView(qualifierViews[i]);

                sve.Add(qv);
            }

            var defQv = WriteQualifierView(sv.defaultQualifierView);
            sve.Add(defQv);

            return sve;
        }

        private static StageElement WriteQualifierView(QualifierView qv)
        {
            var name = qv.isDefault ? ElementName.DefaultQualifier : ElementName.QualifierView;
            var qve = new StageElement(name);
            qve.AddTextValue(ElementName.Name, qv.name);
            qve.AddTextValue(ElementName.Description, qv.description);

            var av = qv.actionView;
            if (av != null)
            {
                var ae = WriteActionView(av);
                qve.Add(ae);
            }

            return qve;
        }

        private static StageElement WriteActionView(ActionView av)
        {
            var ae = new StageElement(ElementName.ActionView);
            ae.AddTextValue(ElementName.Name, av.name);
            ae.AddTextValue(ElementName.Description, av.description);

            if (av is SelectorActionView)
            {
                ae.AddAttribute(AttributeName.ConnectorType, ConnectorType.Selector);
            }
            else if (av is AILinkActionView)
            {
                ae.AddAttribute(AttributeName.ConnectorType, ConnectorType.AILink);
            }
            else if (av is CompositeActionView)
            {
                ae.AddAttribute(AttributeName.ConnectorType, ConnectorType.Composite);
            }

            return ae;
        }

        private static void DeserializeAILinkViews(StageElement[] viewElements, AIUI targetUI, Vector2 referencePos, Vector2 mousePos, List<TopLevelView> newViews)
        {
            for (int i = 0; i < viewElements.Length; i++)
            {
                var lv = ReadAILinkView(viewElements[i], targetUI);

                //We only add an AI link if it does not already exist. Having two links to the same AI does not make sense.
                var existing = targetUI.FindAILink(other => other.aiId == lv.aiId);
                if (existing != null)
                {
                    continue;
                }

                // first treat the top-left-most selector as the origin point (reference pos)
                lv.viewArea.position -= referencePos;

                // secondly add the mouse offset to the position so as to paste it in where the mouse is at
                lv.viewArea.position += mousePos;

                targetUI.canvas.views.Add(lv);
                newViews.Add(lv);

                targetUI.undoRedo.Do(new AddAILinkOperation(targetUI, lv));
            }
        }

        private static void DeserializeSelectorViews(StageElement[] aiElements, StageElement[] viewElements, AIUI targetUI, Vector2 referencePos, Vector2 mousePos, List<TopLevelView> newViews, bool regenIds)
        {
            var requiresInit = new List<IInitializeAfterDeserialization>();

            var selectorViews = new List<SelectorView>(aiElements.Length);

            for (int i = 0; i < aiElements.Length; i++)
            {
                var selectorView = ReadSelectorView(viewElements[i], targetUI);
                var selector = SerializationMaster.Unstage<Selector>(aiElements[i], requiresInit);

                targetUI.canvas.views.Add(selectorView);
                targetUI.ai.AddSelector(selector);

                selectorView.Reconnect(selector);

                selectorViews.Add(selectorView);
                newViews.Add(selectorView);

                targetUI.undoRedo.Do(new AddSelectorOperation(targetUI, selectorView));
            }

            //Set the root if pasting to a blank canvas
            if (targetUI.ai.rootSelector == null)
            {
                targetUI.ai.rootSelector = selectorViews[0].selector;
                targetUI.undoRedo.Do(new SetRootOperation(targetUI, null, selectorViews[0].selector));
            }

            //Do the post deserialize initialization here
            var count = requiresInit.Count;
            for (int i = 0; i < count; i++)
            {
                requiresInit[i].Initialize(targetUI.ai);
            }

            //Finally adjust the positioning of pasted elements and prune broken connections, i.e. SelectorAction that no longer points to a selector
            for (int i = 0; i < aiElements.Length; i++)
            {
                var selectorView = selectorViews[i];

                selectorView.PruneBrokenConnections();

                if (regenIds)
                {
                    //We also regen the id since this allows for pasting the same copy multiple times
                    selectorView.selector.RegenerateId();
                }

                // first treat the top-left-most selector as the origin point (reference pos)
                selectorView.viewArea.position -= referencePos;

                // secondly add the mouse offset to the position so as to paste it in where the mouse is at
                selectorView.viewArea.position += mousePos;
            }
        }

        private static void DeserializeQualifier(StageElement aiElement, StageElement viewElement, AIUI targetUI)
        {
            var parent = targetUI.currentSelector;
            if (parent == null)
            {
                EditorUtility.DisplayDialog("Invalid Action", "You must select a Selector before pasting a Qualifier.", "Ok");
            }

            var requiresInit = new List<IInitializeAfterDeserialization>();

            var qualifierView = ReadQualifierView(viewElement, parent);
            var qualifier = SerializationMaster.Unstage<IQualifier>(aiElement, requiresInit);

            qualifierView.Reconnect(qualifier);

            //Do the post deserialize initialization here
            if (requiresInit.Count == 1)
            {
                requiresInit[0].Initialize(targetUI.ai);
            }

            qualifierView.PruneBrokenConnections();

            if (qualifierView.isDefault)
            {
                if (EditorUtility.DisplayDialog("Confirm Replacement", "Do you wish to replace the current Default Qualifier?", "Yes", "No"))
                {
                    targetUI.ReplaceDefaultQualifier(qualifierView, parent);
                }
            }
            else
            {
                targetUI.AddQualifier(qualifierView, parent);
            }
        }

        private static void DeserializeAction(StageElement aiElement, StageElement viewElement, AIUI targetUI)
        {
            var parent = targetUI.currentQualifier;
            if (parent == null)
            {
                EditorUtility.DisplayDialog("Invalid Action", "You must select a Qualifier before pasting an Action.", "Ok");
            }
            else if (parent.actionView != null)
            {
                if (!EditorUtility.DisplayDialog("Confirm Replace", "The selected Qualifier already has an action, do you wish to replace it?", "Yes", "No"))
                {
                    return;
                }
            }

            var requiresInit = new List<IInitializeAfterDeserialization>();

            var actionView = ReadActionView(viewElement, parent);
            var action = SerializationMaster.Unstage<IAction>(aiElement, requiresInit);

            actionView.action = action;

            //Do the post deserialize initialization here
            if (requiresInit.Count == 1)
            {
                requiresInit[0].Initialize(targetUI.ai);
            }

            targetUI.SetAction(actionView, parent);
            parent.PruneBrokenConnections();
        }

        internal class ElementName
        {
            public const string Canvas = "Canvas";
            public const string AILinkView = "AILinkView";
            public const string SelectorView = "SelectorView";
            public const string QualifierView = "QualifierView";
            public const string ActionView = "ActionView";

            public const string ViewArea = "ViewArea";
            public const string Name = "Name";
            public const string Description = "Description";
            public const string DefaultQualifier = "DefaultQualifier";
            public const string Offset = "offset";
            public const string Zoom = "zoom";

            public const string AIPart = "ai_part";
            public const string UIPart = "ui_part";
            public const string ViewSnippet = "viewssnippet";
            public const string QualifierSnippet = "qualifiersnippet";
            public const string ActionSnippet = "actionsnippet";
            public const string Qualifier = "Qualifier";
            public const string Action = "Action";
            public const string ReferencePosition = "ReferencePosition";
        }

        private class AttributeName
        {
            public const string Name = "name";
            public const string Id = "id";
            public const string SnippetType = "snippetType";
            public const string ConnectorType = "connectorType";
        }

        private class ConnectorType
        {
            public const string Selector = "Selector";
            public const string AILink = "AILink";
            public const string Composite = "Composite";
        }
    }
}
