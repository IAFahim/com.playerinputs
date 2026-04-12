using BovineLabs.Core.Authoring.Settings;
using BovineLabs.Reaction.Authoring.Conditions;
using BovineLabs.Reaction.Data.Conditions;
using BovineLabs.Timeline.Authoring;
using Bovinelabs.Timeline.PlayerInputs.Data;
using Unity.Entities;
using UnityEngine.InputSystem;
using UnityEngine.Timeline;
using InputSettings = Bovinelabs.Timeline.PlayerInputs.Data.InputSettings;

namespace Bovinelabs.Timeline.PlayerInputs.Authoring
{
    public sealed class PlayerInputConditionInvokerClip : DOTSClip, ITimelineClipAsset
    {
        public InputActionReference action;
        public InputPhase phase;
        public ConditionEventObject condition;
        public int value = 1;
        public override double duration => 1;

        public ClipCaps clipCaps => ClipCaps.None;

        public override void Bake(Entity clipEntity, BakingContext context)
        {
            var settings = AuthoringSettingsUtility.GetSettings<InputSettings>();
            byte actionId = 0;
            
            for (byte i = 0; i < settings.Mappings.Count; i++)
            {
                if (settings.Mappings[i].Action != action) continue;
                actionId = i;
                break;
            }

            context.Baker.AddComponent(clipEntity, new InputInvokerConfig
            {
                ActionId = actionId,
                Phase = phase,
                Condition = condition ? condition.Key : ConditionKey.Null,
                Value = value
            });

            base.Bake(clipEntity, context);
        }
    }
}