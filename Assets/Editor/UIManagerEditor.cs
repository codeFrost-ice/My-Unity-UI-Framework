using UIFramework;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UIManager))]
public class UIManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        UIManager manager = (UIManager)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("🧱 当前UI栈信息", EditorStyles.boldLabel);

        if (Application.isPlaying)
        {
            var stackField = typeof(UIManager).GetField("_panelStack", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var stack = stackField.GetValue(manager) as System.Collections.IEnumerable;

            if (stack != null)
            {
                foreach (var panel in stack)
                {
                    EditorGUILayout.LabelField("→ " + (panel != null ? panel.ToString() : "(空引用)"));
                }
            }
        }
        else
        {
            EditorGUILayout.HelpBox("运行时才能查看堆栈内容。", MessageType.Info);
        }

        Repaint(); // 实时刷新 Inspector
    }
}
