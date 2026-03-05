using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial struct PlayerVisibilitySystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<InputActionAim>();
    }

    public void OnUpdate(ref SystemState state)
    {
        Vector2 aimInput = SystemAPI.GetSingleton<InputActionAim>().ScreenCoord;

        Ray ray = Camera.main.ScreenPointToRay(aimInput);

        foreach (var (visibilityRotation, transform) in SystemAPI.Query<RefRW<CharacterVisibilityRotation>, RefRO<LocalToWorld>>().WithAll<Parent>().WithAll<PlayerVisibilityTag>())
        {
            Plane groundPlane = new Plane(Vector3.up, transform.ValueRO.Position);
            Vector3 aimPosition = Vector3.zero;

            if (groundPlane.Raycast(ray, out float distance))
            {
                aimPosition = ray.GetPoint(distance);
            }

            float3 lookAtTarget = new float3(aimPosition);

            float3 lookAtDirection = lookAtTarget - transform.ValueRO.Position;
            lookAtDirection.y = 0.0f;

            visibilityRotation.ValueRW.Rotation = quaternion.LookRotation(lookAtDirection, new float3(0.0f, 1.0f, 0.0f));
        }
    }
}
