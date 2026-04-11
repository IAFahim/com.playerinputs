using BovineLabs.Timeline;
using BovineLabs.Timeline.Data;
using Bovinelabs.Timeline.PlayerInputs.Data;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Bovinelabs.Timeline.PlayerInputs
{
    [UpdateInGroup(typeof(TimelineComponentAnimationGroup))]
    public partial struct InputCancelWindowSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new EvaluateCancelTransition
            {
                Sources = SystemAPI.GetComponentLookup<InputSource>(true),
                States = SystemAPI.GetComponentLookup<InputState>(true),
                Timelines = SystemAPI.GetComponentLookup<TimelineActive>(false)
            }.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        [WithAll(typeof(ClipActive))]
        private partial struct EvaluateCancelTransition : IJobEntity
        {
            [ReadOnly] public ComponentLookup<InputSource> Sources;
            [ReadOnly] public ComponentLookup<InputState> States;
            [NativeDisableParallelForRestriction] public ComponentLookup<TimelineActive> Timelines;

            private void Execute(in InputCancelWindowConfig config, in TrackBinding binding, in DirectorRoot director)
            {
                var consumer = binding.Value;

                if (!this.Sources.TryGetComponent(consumer, out var source) || source.Provider == Entity.Null)
                {
                    return;
                }

                if (!this.States.TryGetComponent(source.Provider, out var state))
                {
                    return;
                }

                if (state.Down.Overlaps(config.AllowedMask))
                {
                    if (this.Timelines.HasComponent(director.Director))
                    {
                        this.Timelines.SetComponentEnabled(director.Director, false);
                    }
                }
            }
        }
    }
}