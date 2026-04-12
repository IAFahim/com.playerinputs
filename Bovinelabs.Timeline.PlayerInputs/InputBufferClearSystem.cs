// InputBufferClearSystem.cs
using BovineLabs.Core.Extensions;
using BovineLabs.Core.Iterators;
using BovineLabs.Timeline;
using BovineLabs.Timeline.Data;
using Bovinelabs.Timeline.PlayerInputs.Data;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Bovinelabs.Timeline.PlayerInputs
{
    [UpdateInGroup(typeof(TimelineComponentAnimationGroup))]
    public partial struct InputBufferClearSystem : ISystem
    {
        private UnsafeComponentLookup<InputSource> sources;
        private UnsafeBufferLookup<InputHistory> histories;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            this.sources = state.GetUnsafeComponentLookup<InputSource>(true);
            this.histories = state.GetUnsafeBufferLookup<InputHistory>(false);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            this.sources.Update(ref state);
            this.histories.Update(ref state);
            state.Dependency = new ClearBufferTransition
            {
                Sources = this.sources,
                Histories = this.histories,
            }.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        [WithAll(typeof(ClipActive), typeof(InputBufferClearTrigger))]
        [WithNone(typeof(ClipActivePrevious))]
        private partial struct ClearBufferTransition : IJobEntity
        {
            [ReadOnly] public UnsafeComponentLookup<InputSource> Sources;
            [NativeDisableParallelForRestriction] public UnsafeBufferLookup<InputHistory> Histories;

            private void Execute(in InputBufferClearTrigger config, in TrackBinding binding)
            {
                var consumer = binding.Value;
                if (!this.Sources.TryGetComponent(consumer, out var source) || source.Provider == Entity.Null) return;
                if (!this.Histories.TryGetBuffer(source.Provider, out var history)) return;

                ref var actionIds = ref config.ActionIds.Value;

                // Empty blob = clear all
                if (actionIds.Length == 0)
                {
                    history.Clear();
                    return;
                }

                for (var i = history.Length - 1; i >= 0; i--)
                {
                    var historyActionId = history[i].ActionId;
                    for (var j = 0; j < actionIds.Length; j++)
                    {
                        if (historyActionId == actionIds[j])
                        {
                            history.RemoveAt(i);
                            break;
                        }
                    }
                }
            }
        }
    }
}