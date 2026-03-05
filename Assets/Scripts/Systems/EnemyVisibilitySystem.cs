using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial struct EnemyVisibilitySystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerTag>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
        float3 lookAtTarget = SystemAPI.GetComponent<LocalTransform>(playerEntity).Position;

        state.Dependency = new EnemyVisibilityJob
        {
            LookAtTarget = lookAtTarget
        }.ScheduleParallel(state.Dependency);
    }
}

[BurstCompile]
[WithAll(typeof(EnemyVisibilityTag))]
[WithAll(typeof(Parent))]
public partial struct EnemyVisibilityJob : IJobEntity
{
    public float3 LookAtTarget;

    public void Execute(ref CharacterVisibilityRotation VisibilityRotation, in LocalToWorld Transform)
    {
        float3 lookAtDirection = LookAtTarget - Transform.Position;
        lookAtDirection.y = 0.0f;

        VisibilityRotation.Rotation = quaternion.LookRotation(lookAtDirection, new float3(0.0f, 1.0f, 0.0f));
    }
}