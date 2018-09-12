using UnityEngine;
using UnityEditor;

namespace Battlehub.UIControls
{
    [CustomEditor(typeof(VirtualizingScrollRect), true)]
    public class VirtualizingScrollRectEditor : UnityEditor.UI.ScrollRectEditor
    {
        private SerializedProperty m_virtualContentProp;
        private SerializedProperty m_containerPrefabProp;
        private SerializedProperty m_modeProp;
        private SerializedProperty m_useGrid;
        private SerializedProperty m_gridSpacing;
        protected override void OnEnable()
        {
            base.OnEnable();

            m_virtualContentProp = serializedObject.FindProperty("m_virtualContent");
            m_containerPrefabProp = serializedObject.FindProperty("ContainerPrefab");
            m_modeProp = serializedObject.FindProperty("m_mode");
            m_useGrid = serializedObject.FindProperty("m_useGrid");
            m_gridSpacing = serializedObject.FindProperty("m_gridSpacing");
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_virtualContentProp);
            EditorGUILayout.PropertyField(m_containerPrefabProp);
            EditorGUILayout.PropertyField(m_modeProp);
            EditorGUILayout.PropertyField(m_useGrid);
            EditorGUILayout.PropertyField(m_gridSpacing);

            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }

    }

}
