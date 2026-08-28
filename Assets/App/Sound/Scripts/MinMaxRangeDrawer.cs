#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Sound
{
    [CustomPropertyDrawer(typeof(MinMaxRangeAttribute))]
    public class MinMaxRangeDrawer : PropertyDrawer
    {
        private const float FieldWidth = 40f;
        private const float Spacing = 4f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var range = (MinMaxRangeAttribute)attribute;

            EditorGUI.BeginProperty(position, label, property);

            var labelRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, position.height);
            EditorGUI.LabelField(labelRect, label);

            var minFieldRect = new Rect(labelRect.xMax, position.y, FieldWidth, position.height);
            var maxFieldRect = new Rect(position.xMax - FieldWidth, position.y, FieldWidth, position.height);
            var sliderRect = new Rect(minFieldRect.xMax + Spacing, position.y,
                maxFieldRect.x - minFieldRect.xMax - Spacing * 2f, position.height);

            if (property.propertyType == SerializedPropertyType.Vector2)
            {
                var value = property.vector2Value;
                float min = value.x, max = value.y;

                min = EditorGUI.FloatField(minFieldRect, min);
                EditorGUI.MinMaxSlider(sliderRect, ref min, ref max, range.Min, range.Max);
                max = EditorGUI.FloatField(maxFieldRect, max);

                min = Mathf.Clamp(min, range.Min, max);
                max = Mathf.Clamp(max, min, range.Max);

                property.vector2Value = new Vector2(min, max);
            }
            else if (property.propertyType == SerializedPropertyType.Vector2Int)
            {
                var value = property.vector2IntValue;
                float min = value.x, max = value.y;

                min = EditorGUI.FloatField(minFieldRect, min);
                EditorGUI.MinMaxSlider(sliderRect, ref min, ref max, range.Min, range.Max);
                max = EditorGUI.FloatField(maxFieldRect, max);

                min = Mathf.Clamp(min, range.Min, max);
                max = Mathf.Clamp(max, min, range.Max);

                property.vector2IntValue = new Vector2Int(Mathf.RoundToInt(min), Mathf.RoundToInt(max));
            }
            else
            {
                EditorGUI.LabelField(position, label, "MinMaxRange needs a Vector2 or Vector2Int field.");
            }

            EditorGUI.EndProperty();
        }
    }
}
#endif
