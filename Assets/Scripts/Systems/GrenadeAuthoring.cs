using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

public struct GrenadeLifespan : IComponentData
{
    public float Value;
}

public struct ExpirationTimestamp : IComponentData
{
    public double Value;
}

public struct GrenadeUninitilizedTag : IComponentData, IEnableableComponent
{ }

public struct ExplosionPrefab : IComponentData
{
    public Entity Value;
}

public struct ExplosionData : IComponentData
{
    public float Radius;
    public float Damage;
    public float AnimationDuration;
}

public struct GrenadeCollisionFilter : IComponentData
{
    public CollisionFilter Value;
}

[RequireComponent(typeof(ThrowableAuthoring))]
public class GrenadeAuthoring : MonoBehaviour
{
    public GameObject ExplosionPrefab;

    public float Lifespan = 3.5f;

    public float ExplosionRadius = 6.0f;
    public float ExplosionDamage = 150.0f;
    public float ExplosionAnimationDuration = 0.3f;

    private class Baker : Baker<GrenadeAuthoring>
    {
        public override void Bake(GrenadeAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

            AddComponent(entity, new GrenadeLifespan
            {
                Value = authoring.Lifespan
            });
            AddComponent<ExpirationTimestamp>(entity);
            AddComponent<GrenadeUninitilizedTag>(entity);
            SetComponentEnabled<GrenadeUninitilizedTag>(entity, false);
            AddComponent<DestroyEntityTag>(entity);
            SetComponentEnabled<DestroyEntityTag>(entity, false);
            AddComponent(entity, new PhysicsGravityFactor
            {
                Value = 5.0f
            });
            AddComponent(entity, new ExplosionPrefab
            {
                Value = GetEntity(authoring.ExplosionPrefab, TransformUsageFlags.Dynamic),
            });
            AddComponent(entity, new ExplosionData
            {
                Radius = authoring.ExplosionRadius,
                Damage = authoring.ExplosionDamage,
                AnimationDuration = authoring.ExplosionAnimationDuration
            });

            var enemyLayer = LayerMask.NameToLayer("Enemy");
            var playerLayer = LayerMask.NameToLayer("Player");
            var grenadeLayerMask = (uint)math.pow(2, enemyLayer) | (uint)math.pow(2, playerLayer);
            var grenadeCollisionFiler = new CollisionFilter
            {
                BelongsTo = uint.MaxValue,
                CollidesWith = grenadeLayerMask
            };

            AddComponent(entity, new GrenadeCollisionFilter
            {
                Value = grenadeCollisionFiler,
            });
        }
    }
}

[BurstCompile]
public partial struct GrenadeInitializationSystem : ISystem
{
    public void OnUpdate(ref SystemState satate)
    {
        double elapsedTime = SystemAPI.Time.ElapsedTime;

        foreach (var (lifespan, expirationTimestamp, enableInitialization) in SystemAPI.Query<
            RefRO<GrenadeLifespan>,
            RefRW<ExpirationTimestamp>,
            EnabledRefRW<GrenadeUninitilizedTag>>().WithAll<GrenadeUninitilizedTag>())
        {
            expirationTimestamp.ValueRW.Value = elapsedTime + lifespan.ValueRO.Value;
            enableInitialization.ValueRW = false;
        }
    }
}
