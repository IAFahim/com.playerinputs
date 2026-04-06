using PlayerInputs.Data;
using Unity.Collections;
using Unity.Entities;

namespace PlayerInputs.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct PlayerInputPropagationSystem : ISystem
    {
        private EntityQuery _backingQuery;

        public void OnCreate(ref SystemState state)
        {
            _backingQuery = state.GetEntityQuery(
                ComponentType.ReadWrite<InputSubscribedEntity>(),
                ComponentType.ReadWrite<InputButtonDownBuffer>(),
                ComponentType.ReadWrite<InputButtonHeldBuffer>(),
                ComponentType.ReadWrite<InputButtonUpBuffer>(),
                ComponentType.ReadWrite<InputAxisBuffer>(),
                typeof(BackingInputEntityTag));

            state.RequireForUpdate<BackingInputEntityTag>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var em = state.EntityManager;
            using var backingEntities = _backingQuery.ToEntityArray(Allocator.Temp);

            for (int i = 0; i < backingEntities.Length; i++)
            {
                var backing = backingEntities[i];
                var subscribers = em.GetBuffer<InputSubscribedEntity>(backing);

                if (subscribers.Length == 0) continue;

                var masterDowns = em.GetBuffer<InputButtonDownBuffer>(backing).ToNativeArray(Allocator.Temp);
                var masterHelds = em.GetBuffer<InputButtonHeldBuffer>(backing).ToNativeArray(Allocator.Temp);
                var masterUps = em.GetBuffer<InputButtonUpBuffer>(backing).ToNativeArray(Allocator.Temp);
                var masterAxes = em.GetBuffer<InputAxisBuffer>(backing).ToNativeArray(Allocator.Temp);

                for (int j = 0; j < subscribers.Length; j++)
                {
                    var target = subscribers[j].Value;
                    if (!em.HasBuffer<InputButtonDownBuffer>(target)) continue;

                    var targetDowns = em.GetBuffer<InputButtonDownBuffer>(target);
                    var targetHelds = em.GetBuffer<InputButtonHeldBuffer>(target);
                    var targetUps = em.GetBuffer<InputButtonUpBuffer>(target);
                    var targetAxes = em.GetBuffer<InputAxisBuffer>(target);

                    targetDowns.CopyFrom(masterDowns);
                    targetHelds.CopyFrom(masterHelds);
                    targetUps.CopyFrom(masterUps);
                    targetAxes.CopyFrom(masterAxes);
                }

                masterDowns.Dispose();
                masterHelds.Dispose();
                masterUps.Dispose();
                masterAxes.Dispose();
            }
        }
    }
}
