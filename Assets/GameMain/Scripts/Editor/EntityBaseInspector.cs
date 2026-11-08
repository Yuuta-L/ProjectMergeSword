using MergeSword;
using UnityEditor;

[CustomEditor(typeof(EntityBase), true)]
public class EntityBaseInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (!EditorApplication.isPlaying) return;

        EditorGUILayout.SelectableLabel($"EntityId: {(target as EntityBase).SerialId}");
    }
}
