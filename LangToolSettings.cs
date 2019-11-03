using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Net;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace JoeCH.LangTool
{
	[System.Serializable]
    public struct LanguageFont {
        public Font font;
        public float sizeChange;
    }
	
    [CreateAssetMenu(fileName = "LangTool Settings", menuName = "LangTool/Settings")]
    public class LangToolSettings : ScriptableObject {
        [System.Serializable]
        public class StringList {
            public List<string> list = new List<string>();

            public string this[int i] {
                get { return list[i]; }
                set { list[i] = value; }
            }

            public int Count {
                get { return list.Count; }
            }

            public void Add(string elm) {
                list.Add(elm);
            }
        }

        public string googleSheetID;
        public int currentLanguageColumn = 0;
        public float textRolloutPause;
        public bool pauseOnFirstChar;
        public bool pauseOnSpaces;
        public bool forceUpdate;
        public List<LanguageFont> languageFonts = new List<LanguageFont>();
        public List<StringList> localizationTable = new List<StringList>();

        private const string defaultText = "TEXT_UNDEFINED";
        private readonly Regex newLine = new Regex(@"\r\n?|\n");

        public event System.Action onLanguageChange;

        void Awake() {
            currentLanguageColumn = PlayerPrefs.GetInt("JOECH_LANGTOOL_CURRENT_LANGUAGE_COLUMN_" + GetInstanceID().ToString(), 0);
        }

        void OnDestroy() {
            PlayerPrefs.SetInt("JOECH_LANGTOOL_CURRENT_LANGUAGE_COLUMN_" + GetInstanceID().ToString(), currentLanguageColumn);
        }

        //changes language column and notifies subscribed language controllers.
        public void ChangeLanguage(int newColumn) {
            if (localizationTable.Count == 0 || newColumn >= localizationTable[0].Count) {
                Debug.LogError("LangTool: Tried to update text, but the new language is outside the bounds of the table.");
                return;
            }

            if ((newColumn != currentLanguageColumn || forceUpdate) && onLanguageChange != null) {
                currentLanguageColumn = newColumn;
                onLanguageChange();
            } 
        }

        public void UpdateText() {
            onLanguageChange();
        }

        public string getText(int textRow) {
            return localizationTable[textRow - 1][currentLanguageColumn];
        }

        //Imports a TSV file and converts it into the localization table, overriding any previous values.
        void ImportTSV(string tsv) {
            //load tsv and seperate it by the newlines
            string[] rows = newLine.Split(tsv);

            if (rows.Length == 0) {
                Debug.LogError("LangTool: The TSV file obtained was empty or invalid. Please check the google doc or file that you attempted to import.");
                return;
            }

            int maxLen = -1;
            localizationTable.Clear();

            for (int i = 0; i < rows.Length; i++) {
                string[] columns = rows[i].Split('\t');
                for (int j = 0; j < columns.Length; j++) {
                    if (columns[j].Length == 0) columns[i] = defaultText;
                }

                localizationTable.Add(new StringList() { list = new List<string>(columns) });

                maxLen = System.Math.Max(maxLen, columns.Length);
            }

            for (int i = 0; i < rows.Length; i++) {
                while (localizationTable[i].Count < maxLen) localizationTable[i].Add(defaultText);
            }

            while (languageFonts.Count < maxLen) languageFonts.Add(new LanguageFont());
            while (languageFonts.Count > maxLen) languageFonts.RemoveAt(languageFonts.Count - 1);
        }

        //retrieves TSV from URL and overrides localization tables
        void ImportGoogleSheet() {
            string url = "https://docs.google.com/spreadsheets/d/" + googleSheetID + "/export?format=tsv";
            using (WebClient client = new WebClient()) {
                string downloaded = client.DownloadString(url);

                if (downloaded.Length > 0) ImportTSV(downloaded);
                else {
                    Debug.LogError("LangTool: Google Sheet downloaded is invalid. please check your sheet id or try again.");
                    return;
                }
            }
        }

		#if UNITY_EDITOR
        //retrieves TSV from path and overrides localization table
        void ImportLocalFile() {
            string path = EditorUtility.OpenFilePanel("Select TSV File", "./", "tsv");
            if (path.Length == 0) return;

            string file = System.IO.File.ReadAllText(path);

            if (file.Length > 0) ImportTSV(file);
            else {
                Debug.LogError("LangTool: File provided is blank. Cannot use blank file for localization table.");
                return;
            }
            ImportTSV(file);
        }

        [ContextMenu("Update Sheet")]
        void UpdateTextSheet() {
            int option = EditorUtility.DisplayDialogComplex("Choose Import Type", "How will you be importing your TSV file?", "Google Sheet", "Cancel", "From A Local File");

            switch (option) {
                case 2:
                    ImportLocalFile();
                    break;
                case 0:
                    ImportGoogleSheet();
                    break;
                default:
                    return;
            }
        }
		#endif
    }
}
