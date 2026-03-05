using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;

public struct ThrowableUninitilizedTag : IComponentData, IEnableableComponent
{
    public float3 StartingForce;
}

public class ThrowableAuthoring : MonoBehaviour
{
    private class Baker : Baker<ThrowableAuthoring>
    {
        public override void Bake(ThrowableAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

            // Set the StartingForce and enable from the spawning entity
            AddComponent<ThrowableUninitilizedTag>(entity);
            SetComponentEnabled<ThrowableUninitilizedTag>(entity, false);
        }
    }
}

[BurstCompile]
public partial struct ThrowableInitializationSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (initializationData, enabledTag, physicsVelocity) in SystemAPI.Query<RefRO<ThrowableUninitilizedTag>, EnabledRefRW<ThrowableUninitilizedTag>, RefRW<PhysicsVelocity>>())
        {
            physicsVelocity.ValueRW.Linear = initializationData.ValueRO.StartingForce;
            enabledTag.ValueRW = false;
        }
    }
}