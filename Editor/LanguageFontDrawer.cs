using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(JoeCH.LangTool.LanguageFont))]
public class LanguageFontDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        // Using BeginProperty / EndProperty on the parent property means that
        // prefab override logic works on the entire property.
        EditorGUI.BeginProperty(position, label, property);

        // Draw label
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        // Don't make child fields be indented
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        // Calculate rects
        var fontRect = new Rect(position.x, position.y, position.width * 0.5f, position.height);
        var sizeLabelRect = new Rect(position.x + (position.width * 0.52f), position.y, position.width * 0.26f, position.height);
        var sizeRect = new Rect(position.x + (position.width * 0.8f), position.y, position.width * 0.2f, position.height);

        // Draw fields - passs GUIContent.none to each so they are drawn without labels
        EditorGUI.PropertyField(fontRect, property.FindPropertyRelative("font"), GUIContent.none);
        EditorGUI.LabelField(sizeLabelRect, "Size Scale");
        EditorGUI.PropertyField(sizeRect, property.FindPropertyRelative("sizeChange"), GUIContent.none);

        // Set indent back to what it was
        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();
    }
}   