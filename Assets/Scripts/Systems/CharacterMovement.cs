using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

public struct CharacterMovementDirection : IComponentData
{
    public float3 Direction;
}

public struct CharacterMovementSpeed : IComponentData
{
    public float Speed;
}

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(TransformSystemGroup))]
public partial struct CharacterMovementSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        state.Dependency = new CharacterMovementJob
        { }.ScheduleParallel(state.Dependency);
    }
}

[BurstCompile]
public partial struct CharacterMovementJob : IJobEntity
{
    public void Execute(in CharacterMovementDirection MovementDirection, in CharacterMovementSpeed MovementSpeed, ref PhysicsVelocity Velocity, in LocalTransform Transform)
    {
        float verticalVelocity = Velocity.Linear.y;
        Velocity.Linear = math.normalizesafe(MovementDirection.Direction) * MovementSpeed.Speed;
        Velocity.Linear.y = verticalVelocity;
    }
}