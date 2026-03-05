using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct GrenadeExplosionSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PhysicsWorldSingleton>();
        state.RequireForUpdate<DamageThisFrame>();
    }

    public void OnUpdate(ref SystemState state)
    {
        double elapsedTime = SystemAPI.Time.ElapsedTime;

        var damageBufferLookup = SystemAPI.GetBufferLookup<DamageThisFrame>();

        var physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();

        foreach (var (expirationTimestamp, enableDestruction, transform, explosionPrefab, explosionData, collisionFilter) in SystemAPI.Query<
            RefRO<ExpirationTimestamp>,
            EnabledRefRW<DestroyEntityTag>,
            RefRO<LocalTransform>,
            RefRO<ExplosionPrefab>,
            RefRO<ExplosionData>,
            RefRO<GrenadeCollisionFilter>>().WithNone<DestroyEntityTag>())
        {
            if (elapsedTime >= expirationTimestamp.ValueRO.Value)
            {
                enableDestruction.ValueRW = true;

                Entity explosionEntity = state.EntityManager.Instantiate(explosionPrefab.ValueRO.Value);

                SystemAPI.SetComponent(explosionEntity, LocalTransform.FromPositionRotationScale(transform.ValueRO.Position, quaternion.identity, explosionData.ValueRO.Radius * 2.0f));
                SystemAPI.SetComponent(explosionEntity, new ExpirationTimestamp
                {
                    Value = elapsedTime + explosionData.ValueRO.AnimationDuration
                });

                // Damage the enemies and players in radius
                NativeList<DistanceHit> distanceHits = new NativeList<DistanceHit>(Allocator.Temp);
                if (physicsWorldSingleton.OverlapSphere(transform.ValueRO.Position, explosionData.ValueRO.Radius, ref distanceHits, collisionFilter.ValueRO.Value))
                {
                    foreach (var hit in distanceHits)
                    {
                        if (SystemAPI.HasComponent<DestroyEntityTag>(hit.Entity) &&
                        !SystemAPI.IsComponentEnabled<DestroyEntityTag>(hit.Entity))
                        {
                            var damageBuffer = damageBufferLookup[hit.Entity];
                            damageBuffer.Add(new DamageThisFrame
                            {
                                Amount = explosionData.ValueRO.Damage
                            });
                        }
                    }
                }
            }
        }
    }
}