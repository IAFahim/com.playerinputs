using PlayerInputs.Data;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace PlayerInputs.Systems
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.LocalSimulation | WorldSystemFilterFlags.ClientSimulation)]
    public partial struct PlayerInputSubscriptionSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BackingInputEntityTag>();
            state.RequireForUpdate<PlayerInputRegisteredTag>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var em = state.EntityManager;

            var backingQuery = SystemAPI.QueryBuilder()
                .WithAll<BackingInputEntityTag, ECSPlayerInputID, InputSubscribedEntity>()
                .Build();

            var subscriberQuery = SystemAPI.QueryBuilder()
                .WithAll<PlayerInputRegisteredTag, ECSPlayerInputID>()
                .WithNone<BackingInputEntityTag>()
                .Build();

            using var backingEntities = backingQuery.ToEntityArray(Allocator.Temp);
            using var subscriberEntities = subscriberQuery.ToEntityArray(Allocator.Temp);

            for (int i = 0; i < backingEntities.Length; i++)
            {
                var backingEntity = backingEntities[i];
                var backingId = em.GetComponentData<ECSPlayerInputID>(backingEntity);
                var subscriberBuffer = em.GetBuffer<InputSubscribedEntity>(backingEntity);

                for (int j = 0; j < subscriberEntities.Length; j++)
                {
                    var subEntity = subscriberEntities[j];
                    var subId = em.GetComponentData<ECSPlayerInputID>(subEntity);

                    if (subId.ID == backingId.ID)
                    {
                        subscriberBuffer.Add(new InputSubscribedEntity { Value = subEntity });
                        em.RemoveComponent<PlayerInputRegisteredTag>(subEntity);
                    }
                }
            }
        }
    }
}
