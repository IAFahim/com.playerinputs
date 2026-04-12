using System.Collections.Generic;
using BovineLabs.Core.Authoring.Settings;
using BovineLabs.Core.Collections;
using BovineLabs.Timeline.Authoring;
using Bovinelabs.Timeline.PlayerInputs.Data;
using Unity.Entities;
using UnityEngine.Timeline;

namespace Bovinelabs.Timeline.PlayerInputs.Authoring
{
    public sealed class PlayerInputCancelWindowClip : DOTSClip, ITimelineClipAsset
    {
        public List<InputSettings.InputMapping> AllowedActions = new();
        public override double duration => 1;

        public ClipCaps clipCaps => ClipCaps.None;

        public override void Bake(Entity clipEntity, BakingContext context)
        {
            var mask = new BitArray256();
            var settings = AuthoringSettingsUtility.GetSettings<InputSettings>();

            foreach (var mapping in AllowedActions)
            {
                for (byte i = 0; i < settings.Mappings.Count; i++)
                {
                    if (settings.Mappings[i].Action != mapping.Action) continue;
                    mask[i] = true;
                    break;
                }
            }

            context.Baker.AddComponent(clipEntity, new InputCancelWindowConfig
            {
                AllowedMask = mask
            });

            base.Bake(clipEntity, context);
        }
    }
}