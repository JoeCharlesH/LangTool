using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class TextController : MonoBehaviour {
    public JoeCH.LangTool.LangToolSettings settings;
    public int textRow;
    public bool adjustFontSize = true;
    public float rolloutTimeScale = 1;
    public bool enableRollout;

    Text uiText;
    TextMesh meshText;
    MeshRenderer meshRenderer;
    Font originalFont;
    float originalSize;
    bool isMesh;

    string targetText;
    string cachedText;

    Dictionary<int, float> pauses = new Dictionary<int, float>();
    Regex pauseCommand = new Regex(@"\<pz *(\d*\.?\d*)\>", RegexOptions.Compiled);

    Coroutine rollout;
    bool rollingOut = false;
    bool force = false;
	
    event System.Action OnAnimationComplete;

    //GROUP 0 = Command, Group 1 = Time

    void Start() {
        uiText = GetComponent<Text>();
        if (uiText) {
            isMesh = false;
            originalSize = uiText.fontSize;
            originalFont = uiText.font;
        }
        else {
            meshText = GetComponent<TextMesh>();
            if (meshText) {
                meshRenderer = GetComponent<MeshRenderer>();
                isMesh = true;
                originalSize = meshText.fontSize;
                originalFont = meshText.font;
            }
            else enabled = false;
        }

        if (enabled) settings.onLanguageChange += LanguageChanged;
        LanguageChanged();
    }

    void LanguageChanged() {
        if (textRow > settings.localizationTable.Count) {
            Debug.LogError("Localization Table does not have " + textRow.ToString() + " rows.");
            return;
        }

        if (settings.currentLanguageColumn >= settings.languageFonts.Count) {
            Debug.LogError("LangTool: Language selected has no font assigned to it.");
            return;
        }

        if (!enableRollout) force = true;
        cachedText = settings.getText(textRow);
        SetUpRollout();
        if (rollingOut) StopCoroutine(rollout);
        rollout = StartCoroutine(RolloutText());

        if (isMesh) {
            if (settings.languageFonts[settings.currentLanguageColumn].font != null)
                meshText.font = settings.languageFonts[settings.currentLanguageColumn].font;
            else if (originalFont != null)
                meshText.font = originalFont;
            meshRenderer.material = meshText.font.material;

            if (adjustFontSize) meshText.fontSize = Mathf.RoundToInt(originalSize * settings.languageFonts[settings.currentLanguageColumn].sizeChange);
        }
        else {
            if (settings.languageFonts[settings.currentLanguageColumn].font != null)
                uiText.font = settings.languageFonts[settings.currentLanguageColumn].font;
            else if (originalFont != null)
                uiText.font = originalFont;
                
            if (adjustFontSize) uiText.fontSize = Mathf.RoundToInt(originalSize * settings.languageFonts[settings.currentLanguageColumn].sizeChange);
        }
    }

    public void ChangeText(string input) {
        ScheduleTextChange(input);
        if (!enableRollout) force = true;
    }

    public void ChangeTextRow(int newRow) {
        if (newRow == textRow && targetText == settings.getText(textRow) && !settings.forceUpdate) return;

        textRow = newRow;
        ScheduleTextChange(settings.getText(textRow));
        if (!enableRollout) force = true;
    }

    void Update() {
        if (!rollingOut && cachedText != "") {
            SetUpRollout();
            rollout = StartCoroutine(RolloutText());
        }
    }

    IEnumerator RolloutText() {
        if (force == true) {
            force = false;
            if (meshText) meshText.text = targetText;
            else uiText.text = targetText;
            if (OnAnimationComplete != null) OnAnimationComplete();
            yield return null;
        }
        else {
            rollingOut = true;

            if (meshText) meshText.text = "";
            else uiText.text = "";

            if (settings.pauseOnFirstChar && !force) yield return new WaitForSeconds(settings.textRolloutPause * rolloutTimeScale);
            for (int i = 0; i < targetText.Length; i++) {
                if (force == true) {
                    force = false;
                    if (meshText) meshText.text = targetText;
                    else uiText.text = targetText;
                    break;
                }

                if (pauses.ContainsKey(i)) yield return new WaitForSeconds(settings.textRolloutPause * rolloutTimeScale * pauses[i]);
                if (meshText) meshText.text += targetText[i];
                else uiText.text += targetText[i];
                if (targetText[i] != ' ' || settings.pauseOnSpaces) yield return new WaitForSeconds(settings.textRolloutPause * rolloutTimeScale);
            }

            rollingOut = false;
			if (OnAnimationComplete != null) OnAnimationComplete();
        }
    }

    public void ForceRolloutFinish() {
        if (rollingOut && !force) force = true;
    }

    void SetUpRollout() {
        targetText = cachedText;
        cachedText = "";
        pauses.Clear();

        Match command = pauseCommand.Match(targetText);
        while (command.Success) {
            string floatVal = command.Groups[1].Value;
            if (floatVal.StartsWith(".")) floatVal = "0" + floatVal;
            if (floatVal.EndsWith(".")) floatVal += "0";

            if(!pauses.ContainsKey(command.Groups[0].Index)) pauses.Add(command.Groups[0].Index, float.Parse(floatVal));

            targetText = pauseCommand.Replace(targetText, "", 1);
            command = pauseCommand.Match(targetText);
        }
    }

    void ScheduleTextChange(string str) {
        cachedText = str;
    }

    void OnDestroy() {
        settings.onLanguageChange -= LanguageChanged;    
    }

    public void AddAnimationListener(System.Action subscriber) {
        OnAnimationComplete += subscriber;
    }

    public void RemoveAnimationListener(System.Action subscriber) {
        OnAnimationComplete -= subscriber;
    }
}
