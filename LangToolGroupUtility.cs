using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace JoeCH.LangTool
{
    [CreateAssetMenu(fileName = "LangTool Group Utility", menuName = "LangTool/Group Utility")]
    public class LangToolGroupUtility : ScriptableObject {
        public List<LangToolSettings> settingsGroup = new List<LangToolSettings>();

        public void GroupTextUpdate() {
            foreach (LangToolSettings settings in settingsGroup) settings.UpdateText();
        }

        public void GroupLanguageChange(int newColumn) {
            foreach (LangToolSettings settings in settingsGroup) settings.ChangeLanguage(newColumn);
        }

        public void GroupLanguageChange(IList<int> newColumns) {
            if (settingsGroup.Count != newColumns.Count) {
                Debug.LogError(string.Format("LangTool: This Group Utility is controlling {0} settings, but the column list provided has {1} indices.", settingsGroup.Count, newColumns.Count));
                return;
            }
            for (int i = 0; i < settingsGroup.Count; i++) settingsGroup[i].ChangeLanguage(newColumns[i]);
        }
    }
}