using System.Collections.Generic;
using PhoenixFlame.Monobehaviour;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace PhoenixFlame.Editor
{
    [CustomEditor(typeof(PhoenixFlameConfig))]
    public class PhoenixFlameConfigEditor : UnityEditor.Editor
    {
        private SerializedProperty _flamePrefab;
        private SerializedProperty _baseMaterial;
        private SerializedProperty _animatorController;
        private SerializedProperty _colorOptions;

        private void OnEnable()
        {
            _flamePrefab = serializedObject.FindProperty("flamePrefab");
            _baseMaterial = serializedObject.FindProperty("baseMaterial");
            _animatorController = serializedObject.FindProperty("animatorController");
            _colorOptions = serializedObject.FindProperty("colorOptions");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_flamePrefab);
            EditorGUILayout.PropertyField(_baseMaterial);
            EditorGUILayout.PropertyField(_animatorController);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Colors (read from Animator Controller)", EditorStyles.boldLabel);

            var controller = _animatorController.objectReferenceValue as AnimatorController;
            var options = PhoenixFlameAnimatorColorReader.ReadColorOptions(controller);

            if (controller == null)
                EditorGUILayout.HelpBox("Assign an Animator Controller above to derive the color list from its states.", MessageType.Info);
            else if (options.Count == 0)
                EditorGUILayout.HelpBox("No \"Any State\" transition gated on a ColorIndex int condition was found.", MessageType.Warning);

            using (new EditorGUI.DisabledScope(true))
            {
                for (var i = 0; i < options.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"{i}: {options[i].DisplayName}", GUILayout.Width(160));
                    EditorGUILayout.ColorField(GUIContent.none, options[i].BaseColor, false, true, false, GUILayout.Width(60));
                    EditorGUILayout.ColorField(GUIContent.none, options[i].EmissionColor, false, true, true, GUILayout.Width(60));
                    EditorGUILayout.EndHorizontal();
                }
            }

            SyncColorOptions(options);

            serializedObject.ApplyModifiedProperties();
        }

        private void SyncColorOptions(IReadOnlyList<PhoenixFlameColorOption> options)
        {
            if (OptionsMatch(options))
                return;

            _colorOptions.arraySize = options.Count;
            for (var i = 0; i < options.Count; i++)
            {
                var element = _colorOptions.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("displayName").stringValue = options[i].DisplayName;
                element.FindPropertyRelative("baseColor").colorValue = options[i].BaseColor;
                element.FindPropertyRelative("emissionColor").colorValue = options[i].EmissionColor;
            }
        }

        private bool OptionsMatch(IReadOnlyList<PhoenixFlameColorOption> options)
        {
            if (_colorOptions.arraySize != options.Count)
                return false;

            for (var i = 0; i < options.Count; i++)
            {
                var element = _colorOptions.GetArrayElementAtIndex(i);
                if (element.FindPropertyRelative("displayName").stringValue != options[i].DisplayName)
                    return false;
                if (element.FindPropertyRelative("baseColor").colorValue != options[i].BaseColor)
                    return false;
                if (element.FindPropertyRelative("emissionColor").colorValue != options[i].EmissionColor)
                    return false;
            }

            return true;
        }
    }
}
