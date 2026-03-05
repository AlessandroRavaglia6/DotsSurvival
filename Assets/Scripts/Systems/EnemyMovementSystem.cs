using System.ComponentModel;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct EnemyMovementSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerTag>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
        float3 playerPosition = SystemAPI.GetComponentLookup<LocalTransform>()[playerEntity].Position;

        state.Dependency = new EnemyMovementJob
        {
            PlayerPosition = playerPosition
        }.ScheduleParallel(state.Dependency);
    }
}

[BurstCompile]
[WithAll(typeof(EnemyTag))]
public partial struct EnemyMovementJob : IJobEntity
{
    public float3 PlayerPosition;

    public void Execute(ref CharacterMovementDirection MovementDirection, in LocalTransform Transform)
    {
        MovementDirection.Direction = PlayerPosition - Transform.Position;
    }
}