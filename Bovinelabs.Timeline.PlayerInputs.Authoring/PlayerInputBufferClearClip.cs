using System.Collections.Generic;
using BovineLabs.Core.Authoring.Settings;
using BovineLabs.Timeline.Authoring;
using Bovinelabs.Timeline.PlayerInputs.Data;
using Unity.Collections;
using Unity.Entities;
using UnityEngine.Timeline;

namespace Bovinelabs.Timeline.PlayerInputs.Authoring
{
    public sealed class PlayerInputBufferClearClip : DOTSClip, ITimelineClipAsset
    {
        public List<InputSettings.InputMapping> ActionsToClear = new();

        public override double duration => 1;
        public ClipCaps clipCaps => ClipCaps.None;

        public override void Bake(Entity clipEntity, BakingContext context)
        {
            var settings = AuthoringSettingsUtility.GetSettings<InputSettings>();

            var builder = new BlobBuilder(Allocator.Temp);
            ref var root = ref builder.ConstructRoot<BlobArray<byte>>();

            if (ActionsToClear.Count > 0)
            {
                var resolved = new List<byte>(ActionsToClear.Count);
                foreach (var mapping in ActionsToClear)
                {
                    for (byte i = 0; i < settings.Mappings.Count; i++)
                    {
                        if (settings.Mappings[i].Action == mapping.Action)
                        {
                            resolved.Add(i);
                            break;
                        }
                    }
                }

                var array = builder.Allocate(ref root, resolved.Count);
                for (var i = 0; i < resolved.Count; i++)
                    array[i] = resolved[i];
            }

            var blobRef = builder.CreateBlobAssetReference<BlobArray<byte>>(Allocator.Persistent);
            builder.Dispose();

            context.Baker.AddBlobAsset(ref blobRef, out _);
            context.Baker.AddComponent(clipEntity, new InputBufferClearTrigger { ActionIds = blobRef });
            context.Baker.SetComponentEnabled<InputBufferClearTrigger>(clipEntity, false);
            base.Bake(clipEntity, context);
        }
    }
}