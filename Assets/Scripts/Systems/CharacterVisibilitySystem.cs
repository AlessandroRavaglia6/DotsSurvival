using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public struct CharacterVisibilityRotation : IComponentData
{
    public quaternion Rotation;
}

[BurstCompile]
[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial struct CharacterVisibilitySystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        state.Dependency = new CharacterVisibilityJob
        { }.ScheduleParallel(state.Dependency);
    }
}

[BurstCompile]
public partial struct CharacterVisibilityJob : IJobEntity
{
    public void Execute(ref LocalTransform Transform, in CharacterVisibilityRotation visibilityRotation)
    {
        Transform.Rotation = visibilityRotation.Rotation;
    }
}
