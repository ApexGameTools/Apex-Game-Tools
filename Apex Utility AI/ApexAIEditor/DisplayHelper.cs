namespace Apex.AI.Editor
{
    using System;
    using Apex.Utilities;
    using UnityEditor;

    internal static class DisplayHelper
    {
        internal static string GetFriendlyName(Type t, bool prettify = true)
        {
            var element = t.GetAttribute<FriendlyNameAttribute>(false);
            if (element != null)
            {
                return element.name;
            }

            return prettify ? t.PrettyName() : t.Name;
        }

        internal static bool ConfirmDelete(string itemType, bool forceConfirm = false)
        {
            if (!UserSettings.instance.confirmDeletes && !forceConfirm)
            {
                return true;
            }

            string msg = string.Format("Are you sure you wish to delete this {0}?", itemType);
            return EditorUtility.DisplayDialog("Confirm Delete", msg, "Yes", "No");
        }
    }
}