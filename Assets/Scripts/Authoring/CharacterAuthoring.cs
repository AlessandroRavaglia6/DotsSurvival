using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;

public class CharacterAuthoring : MonoBehaviour
{
    public float MovementSpeed = 1.0f;
    public float Health = 100.0f;

    private class Baker : Baker<CharacterAuthoring>
    {
        public override void Bake(CharacterAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

            AddComponent<CharacterUninitilizedTag>(entity);
            AddComponent<CharacterMovementDirection>(entity);
            AddComponent(entity, new CharacterMovementSpeed
            {
                Speed = authoring.MovementSpeed
            });
            AddComponent(entity, new MaxHealth
            {
                Value = authoring.Health
            });
            AddComponent(entity, new CurrentHealth
            {
                Value = authoring.Health
            });
            AddBuffer<DamageThisFrame>(entity);

            AddComponent<DestroyEntityTag>(entity);
            SetComponentEnabled<DestroyEntityTag>(entity, false);
            AddComponent(entity, new PhysicsGravityFactor
            {
                Value = 5.0f
            });
        }
    }
}

public struct MaxHealth : IComponentData
{
    public float Value;
}

public struct CurrentHealth : IComponentData
{
    public float Value;
}

public struct DamageThisFrame : IBufferElementData
{
    public float Amount;
}

public struct CharacterUninitilizedTag : IComponentData, IEnableableComponent
{ }

public partial struct CharacterInitializationSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (physicsMass, uninitializedTag) in SystemAPI.Query<RefRW<PhysicsMass>, EnabledRefRW<CharacterUninitilizedTag>>())
        {
            physicsMass.ValueRW.InverseInertia = float3.zero;
            uninitializedTag.ValueRW = false;
        }
    }
}