using BurnicleFramework.Runtime;
using DG.DOTweenEditor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace BurnicleFramework.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(DOTweenSequence))]
    public class DOTweenSequenceInspector : UnityEditor.Editor
    {
        SerializedProperty m_Sequence;
        ReorderableList m_SequenceList;

        GUIContent m_PlayBtnContent;
        GUIContent m_RewindBtnContent;
        GUIContent m_ResetBtnContent;
        private GUILayoutOption m_btnHeight;

        private void OnEnable()
        {
            m_PlayBtnContent = EditorGUIUtility.TrIconContent("d_PlayButton@2x", "播放");
            m_RewindBtnContent = EditorGUIUtility.TrIconContent("d_preAudioAutoPlayOff@2x", "倒放");
            m_ResetBtnContent = EditorGUIUtility.TrIconContent("d_preAudioLoopOff@2x", "重置");
            m_btnHeight = GUILayout.Height(35);
            m_Sequence = serializedObject.FindProperty("m_Sequence");
            m_SequenceList = new ReorderableList(serializedObject, m_Sequence);
            m_SequenceList.drawElementCallback = OnDrawSequenceItem;
            m_SequenceList.elementHeightCallback = index =>
            {
                var item = m_Sequence.GetArrayElementAtIndex(index);
                return EditorGUI.GetPropertyHeight(item);
            };
            m_SequenceList.drawHeaderCallback = OnDrawSequenceHeader;
        }

        public override void OnInspectorGUI()
        {
            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button(m_PlayBtnContent, m_btnHeight))
                    {
                        if (DOTweenEditorPreview.isPreviewing)
                        {
                            DOTweenEditorPreview.Stop(true, true);
                            (target as DOTweenSequence).DOKill();
                        }
                        DOTweenEditorPreview.PrepareTweenForPreview((target as DOTweenSequence).DOPlay());
                        DOTweenEditorPreview.Start();
                    }

                    if (GUILayout.Button(m_RewindBtnContent, m_btnHeight))
                    {
                        if (DOTweenEditorPreview.isPreviewing)
                        {
                            DOTweenEditorPreview.Stop(true, true);
                            (target as DOTweenSequence).DOKill();
                        }

                        DOTweenEditorPreview.PrepareTweenForPreview((target as DOTweenSequence).DORewind());
                        DOTweenEditorPreview.Start();
                    }

                    if (GUILayout.Button(m_ResetBtnContent, m_btnHeight))
                    {
                        DOTweenEditorPreview.Stop(true, true);
                        (target as DOTweenSequence).DOKill();
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }

            serializedObject.Update();
            m_SequenceList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }

        private void OnDrawSequenceHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Animation Sequences");
        }

        private void OnDrawSequenceItem(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = m_Sequence.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(rect, element, true);
        }
    }
}