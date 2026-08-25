using System.Collections.Generic;
using System.Linq;
using PhoenixFlame.Monobehaviour;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace PhoenixFlame.Editor
{
    internal static class PhoenixFlameAnimatorColorReader
    {
        private const string ColorIndexParameter = "ColorIndex";
        private const string BaseColorPrefix = "material._BaseColor.";
        private const string EmissionColorPrefix = "material._EmissionColor.";

        public static List<PhoenixFlameColorOption> ReadColorOptions(AnimatorController controller)
        {
            var result = new List<PhoenixFlameColorOption>();
            if (controller == null || controller.layers.Length == 0)
                return result;

            // Keyed by each transition's ColorIndex condition, not state array order -
            // the condition is the only place a state's actual index is defined.
            var statesByColorIndex = new SortedDictionary<int, AnimatorState>();
            foreach (var transition in controller.layers[0].stateMachine.anyStateTransitions)
            {
                var condition = transition.conditions.FirstOrDefault(c => c.parameter == ColorIndexParameter);
                if (condition.parameter != ColorIndexParameter)
                    continue;

                statesByColorIndex[(int)condition.threshold] = transition.destinationState;
            }

            foreach (var state in statesByColorIndex.Values)
            {
                if (state.motion is not AnimationClip clip)
                    continue;

                result.Add(new PhoenixFlameColorOption(state.name, ReadColor(clip, BaseColorPrefix), ReadColor(clip, EmissionColorPrefix)));
            }

            return result;
        }

        private static Color ReadColor(AnimationClip clip, string propertyPrefix)
        {
            return new Color(
                ReadChannel(clip, propertyPrefix + "r"),
                ReadChannel(clip, propertyPrefix + "g"),
                ReadChannel(clip, propertyPrefix + "b"),
                ReadChannel(clip, propertyPrefix + "a"));
        }

        private static float ReadChannel(AnimationClip clip, string propertyName)
        {
            foreach (var binding in AnimationUtility.GetCurveBindings(clip))
            {
                if (binding.propertyName != propertyName)
                    continue;

                return AnimationUtility.GetEditorCurve(clip, binding).Evaluate(0f);
            }

            return 0f;
        }
    }
}
