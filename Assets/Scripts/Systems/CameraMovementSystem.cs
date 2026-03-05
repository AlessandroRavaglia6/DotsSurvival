using System.Diagnostics.Contracts;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public struct CameraTarget : IComponentData
{
    public UnityObjectRef<Transform> CameraTaregtTransform;
}

public struct CameraTargetUninitializedTag : IComponentData, IEnableableComponent
{ }

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct CameraMovementSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (cameraTarget, playerTransform) in SystemAPI.Query<
                RefRW<CameraTarget>,
                RefRO<LocalTransform>>()
                .WithAll<PlayerTag>()
                .WithNone<CameraTargetUninitializedTag>())
        {
            cameraTarget.ValueRW.CameraTaregtTransform.Value.position = playerTransform.ValueRO.Position;
        }
    }
}

[BurstCompile]
[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct CameraTargetInitializationSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<CameraTargetUninitializedTag>();
    }

    public void OnUpdate(ref SystemState state)
    {
        if (CameraTargetSingleton.Instance == null)
        {
            return;
        }

        var cameraTargetTransform = CameraTargetSingleton.Instance.transform;

        foreach (var (uninitializedTag, cameraTarget) in SystemAPI.Query<EnabledRefRW<CameraTargetUninitializedTag>, RefRW<CameraTarget>>().WithAll<PlayerTag>())
        {
            cameraTarget.ValueRW.CameraTaregtTransform = cameraTargetTransform;
            uninitializedTag.ValueRW = false;
        }
    }
}
