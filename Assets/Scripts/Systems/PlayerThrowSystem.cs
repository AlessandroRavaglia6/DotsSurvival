using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

using Plane = UnityEngine.Plane;
using Ray = UnityEngine.Ray;

public partial struct PlayerThrowSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerTag>();
        state.RequireForUpdate<InputActionAim>();
        state.RequireForUpdate<InputActionThrow>();
    }

    public void OnUpdate(ref SystemState state)
    {
        double elapsedTime = SystemAPI.Time.ElapsedTime;

        var aimInput = SystemAPI.GetSingleton<InputActionAim>();
        var throwInput = SystemAPI.GetSingleton<InputActionThrow>();

        if (throwInput.WasPressedThisFrame)
        {
            // Compute the direction the player is looking toward
            Vector2 aimScreenCoord = aimInput.ScreenCoord;
            Ray ray = Camera.main.ScreenPointToRay(aimScreenCoord);

            foreach (var (transform, grenadePrefab, throwForce) in SystemAPI.Query<RefRO<LocalTransform>, RefRO<GrenadePrefab>, RefRO<PlayerThrowForce>>())
            {
                Plane groundPlane = new Plane(Vector3.up, transform.ValueRO.Position);
                Vector3 aimPosition = Vector3.zero;

                if (groundPlane.Raycast(ray, out float distance))
                {
                    aimPosition = ray.GetPoint(distance);
                }

                float3 lookAtTarget = new float3(aimPosition);

                Entity grenadeEntity = state.EntityManager.Instantiate(grenadePrefab.ValueRO.Value);

                float3 throwDirection = math.normalizesafe(lookAtTarget - transform.ValueRO.Position);
                throwDirection.y = 1.0f;

                SystemAPI.SetComponent(grenadeEntity, LocalTransform.FromPosition(transform.ValueRO.Position));
                SystemAPI.SetComponent(grenadeEntity, new ThrowableUninitilizedTag
                {
                    StartingForce = (throwDirection * throwForce.ValueRO.Value)
                });
                SystemAPI.SetComponentEnabled<ThrowableUninitilizedTag>(grenadeEntity, true);
                SystemAPI.SetComponentEnabled<GrenadeUninitilizedTag>(grenadeEntity, true);
            }
        }
    }
}
