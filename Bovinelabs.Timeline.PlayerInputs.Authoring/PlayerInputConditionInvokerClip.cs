using BovineLabs.Core.Authoring.Settings;
using BovineLabs.Reaction.Authoring.Conditions;
using BovineLabs.Reaction.Data.Conditions;
using BovineLabs.Timeline.Authoring;
using Bovinelabs.Timeline.PlayerInputs.Data;
using Unity.Entities;
using UnityEngine.Timeline;

namespace Bovinelabs.Timeline.PlayerInputs.Authoring
{
    public sealed class PlayerInputConditionInvokerClip : DOTSClip, ITimelineClipAsset
    {
        public InputSettings.InputMapping Action;
        public InputPhase Phase;
        public ConditionEventObject Condition;
        public int Value = 1;
        public override double duration => 1;

        public ClipCaps clipCaps => ClipCaps.None;

        public override void Bake(Entity clipEntity, BakingContext context)
        {
            var settings = AuthoringSettingsUtility.GetSettings<InputSettings>();
            byte actionId = 0;
            
            for (byte i = 0; i < settings.Mappings.Count; i++)
            {
                if (settings.Mappings[i].Action == Action.Action)
                {
                    actionId = i;
                    break;
                }
            }

            context.Baker.AddComponent(clipEntity, new InputInvokerConfig
            {
                ActionId = actionId,
                Phase = Phase,
                Condition = Condition ? Condition.Key : ConditionKey.Null,
                Value = Value
            });

            base.Bake(clipEntity, context);
        }
    }
}