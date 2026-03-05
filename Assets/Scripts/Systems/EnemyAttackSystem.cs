using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;

[UpdateInGroup(typeof(PhysicsSystemGroup))]
[UpdateAfter(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(AfterPhysicsSystemGroup))]
public partial struct EnemyAttackSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SimulationSingleton>();
        state.RequireForUpdate<DamageThisFrame>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        double elapsedTime = SystemAPI.Time.ElapsedTime;

        foreach (var (expirationTimestamp, cooldownEnabled) in SystemAPI.Query<RefRO<EnemyCooldownExpirationTimestamp>, EnabledRefRW<EnemyCooldownExpirationTimestamp>>())
        {
            if (expirationTimestamp.ValueRO.Value > elapsedTime)
            {
                continue;
            }

            cooldownEnabled.ValueRW = false;
        }

        var attackJob = new EnemyAttackJob
        {
            PlayerLookup = SystemAPI.GetComponentLookup<PlayerTag>(true),
            EnemyAttackLookup = SystemAPI.GetComponentLookup<EnemyAttack>(true),
            EnemyCooldownLookup = SystemAPI.GetComponentLookup<EnemyCooldownExpirationTimestamp>(),
            DamageBufferLookup = SystemAPI.GetBufferLookup<DamageThisFrame>(),
            ElapsedTime = elapsedTime
        };

        var simulationSingleton = SystemAPI.GetSingleton<SimulationSingleton>();

        state.Dependency = attackJob.Schedule(simulationSingleton, state.Dependency);
    }
}

public partial struct EnemyAttackJob : ICollisionEventsJob
{
    [ReadOnly] public ComponentLookup<PlayerTag> PlayerLookup;
    [ReadOnly] public ComponentLookup<EnemyAttack> EnemyAttackLookup;
    public ComponentLookup<EnemyCooldownExpirationTimestamp> EnemyCooldownLookup;
    public BufferLookup<DamageThisFrame> DamageBufferLookup;

    public double ElapsedTime;

    public void Execute(CollisionEvent collisionEvent)
    {
        Entity playerEntity;
        Entity enemyEntity;

        if (PlayerLookup.HasComponent(collisionEvent.EntityA) && EnemyAttackLookup.HasComponent(collisionEvent.EntityB))
        {
            playerEntity = collisionEvent.EntityA;
            enemyEntity = collisionEvent.EntityB;
        }
        else if (PlayerLookup.HasComponent(collisionEvent.EntityB) && EnemyAttackLookup.HasComponent(collisionEvent.EntityA))
        {
            playerEntity = collisionEvent.EntityB;
            enemyEntity = collisionEvent.EntityA;
        }
        else
        {
            return;
        }

        if (EnemyCooldownLookup.IsComponentEnabled(enemyEntity))
        {
            return;
        }

        var enemyAttack = EnemyAttackLookup[enemyEntity];
        EnemyCooldownLookup[enemyEntity] = new EnemyCooldownExpirationTimestamp { Value = ElapsedTime + enemyAttack.CooldownTime };
        EnemyCooldownLookup.SetComponentEnabled(enemyEntity, true);

        var playerDamageBuffer = DamageBufferLookup[playerEntity];
        playerDamageBuffer.Add(new DamageThisFrame
        {
            Amount = enemyAttack.AttackDamage
        });
    }
}
