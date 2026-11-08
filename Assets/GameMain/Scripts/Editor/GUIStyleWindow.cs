using UnityEngine;
using UnityEditor;
 
namespace MergeSword
{
    public sealed class GUIStyleWindow : EditorWindow
    {
        [MenuItem("Tools/GUI Style")]
        private static void OpenGUIStyle()
        {
            GetWindow<GUIStyleWindow>().Show();
        }
 
        private UnityEngine.GUIStyle[] styles;
        private Vector2 scroll = Vector2.zero;
        private string searchContent = "";
 
        private void OnGUI()
        {
            if (styles == null)
            {
                styles = GUI.skin.customStyles;
            }
            GUILayout.BeginHorizontal("Toolbar");
            {
                GUILayout.Label("Search:", GUILayout.Width(50));
                searchContent = GUILayout.TextField(searchContent, "SearchTextField");
            }
            GUILayout.EndHorizontal();
 
            scroll = GUILayout.BeginScrollView(scroll);
            {
                foreach (var style in styles)
                {
                    if (style.name.ToLower().Contains(searchContent.ToLower()))
                    {
                        GUILayout.BeginHorizontal("Badge");
                        {
                            if (GUILayout.Button("拷贝", "LargeButton", GUILayout.Width(40f)))
                            {
                                EditorGUIUtility.systemCopyBuffer = style.name;
                                Debug.Log($"拷贝名称: {style.name}");
                            }
                            EditorGUILayout.SelectableLabel(style.name, GUILayout.Width((position.width - 40f) * 0.3f));
                            GUILayout.Button(style.name, style, GUILayout.Width((position.width - 40f) * 0.6f));
                        }
                        GUILayout.EndHorizontal();
                    }
                }
            }
            GUILayout.EndScrollView();
        }
    }
}