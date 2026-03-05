using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

using Plane = UnityEngine.Plane;
using Ray = UnityEngine.Ray;
using RaycastHit = Unity.Physics.RaycastHit;

[InternalBufferCapacity(1)] // Update this when adding new attacks
public struct AttackData : IBufferElementData
{
    public float AttackDamage;
    public float CooldownTime;
}

[InternalBufferCapacity(1)] // Update this when adding new attacks
public struct AttackCooldownExpirationTimestamp : IBufferElementData
{
    public double Value;
}

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct LaserAttackSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PhysicsWorldSingleton>();
        state.RequireForUpdate<PlayerTag>();
        state.RequireForUpdate<InputActionShoot>();
        state.RequireForUpdate<InputActionAim>();
    }

    public void OnUpdate(ref SystemState state)
    {
        double elapsedTime = SystemAPI.Time.ElapsedTime;
        int laserAttackIndex = (int)PlayerAttackType.Laser;
        
        var shootInput = SystemAPI.GetSingleton<InputActionShoot>();
        var aimInput = SystemAPI.GetSingleton<InputActionAim>();

        var damageBufferLookup = SystemAPI.GetBufferLookup<DamageThisFrame>();
        var physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();

        if (shootInput.IsPressed)
        {
            // Compute the direction the player is looking toward
            Vector2 aimScreenCoord = aimInput.ScreenCoord;
            Ray ray = Camera.main.ScreenPointToRay(aimScreenCoord);

            foreach (var (playerTransform, cooldownBuffer, attackDataBuffer, attackConfiguration, laserAttackConfigs) in SystemAPI.Query<
                RefRO<LocalTransform>,
                DynamicBuffer<AttackCooldownExpirationTimestamp>,
                DynamicBuffer<AttackData>,
                RefRO<PlayerAttackConfigs>,
                RefRO<LaserAttackConfigs>>().WithAll<PlayerTag>())
            {
                if (cooldownBuffer[laserAttackIndex].Value > elapsedTime)
                {
                    continue;
                }

                Plane groundPlane = new Plane(Vector3.up, playerTransform.ValueRO.Position);
                Vector3 aimPosition = Vector3.zero;

                if (groundPlane.Raycast(ray, out float distance))
                {
                    aimPosition = ray.GetPoint(distance);
                }

                float3 lookAtTarget = new float3(aimPosition);

                float3 lookAtDirection = lookAtTarget - playerTransform.ValueRO.Position;

                RaycastHit enemyHit;
                RaycastInput raycastInput = new RaycastInput
                {
                    Start = playerTransform.ValueRO.Position,
                    End = playerTransform.ValueRO.Position + (math.normalizesafe(lookAtDirection) * laserAttackConfigs.ValueRO.AttackRange),
                    Filter = attackConfiguration.ValueRO.AttackCollisionFilter
                };
                
                if (physicsWorldSingleton.CastRay(raycastInput, out enemyHit)) 
                {
                    if (SystemAPI.HasComponent<DestroyEntityTag>(enemyHit.Entity) &&
                        !SystemAPI.IsComponentEnabled<DestroyEntityTag>(enemyHit.Entity))
                    {
                        var enemyDamageBuffer = damageBufferLookup[enemyHit.Entity];
                        enemyDamageBuffer.Add(new DamageThisFrame
                        {
                            Amount = attackDataBuffer[laserAttackIndex].AttackDamage
                        });
                    }
                }

                cooldownBuffer.ElementAt(laserAttackIndex).Value = elapsedTime + attackDataBuffer[laserAttackIndex].CooldownTime;
            }
        }
    }
}
