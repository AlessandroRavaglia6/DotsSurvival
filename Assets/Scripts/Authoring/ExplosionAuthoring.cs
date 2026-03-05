using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using UnityEngine;

public struct ExplosionTag : IComponentData
{ }

public class ExplosionAuthoring : MonoBehaviour
{
    private class Baker : Baker<ExplosionAuthoring>
    {
        public override void Bake(ExplosionAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.None);

            AddComponent<ExplosionTag>(entity);
            AddComponent<ExpirationTimestamp>(entity);
            AddComponent<DestroyEntityTag>(entity);
            SetComponentEnabled<DestroyEntityTag>(entity, false);
        }
    }
}

[BurstCompile]
public partial struct ExplosionExpirationSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        double elapsedTime = SystemAPI.Time.ElapsedTime;

        foreach (var (expirationTimestamp, enableDestruction) in SystemAPI.Query<
            RefRO<ExpirationTimestamp>,
            EnabledRefRW<DestroyEntityTag>>().WithNone<DestroyEntityTag>().WithAll<ExplosionTag>())
        {
            if (elapsedTime >= expirationTimestamp.ValueRO.Value)
            {
                enableDestruction.ValueRW = true;
            }
        }
    }
}
