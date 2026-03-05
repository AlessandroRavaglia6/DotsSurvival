using Unity.Entities;
using Unity.Rendering;

public partial struct LaserVisibilitySystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<InputActionShoot>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var shootInput = SystemAPI.GetSingleton<InputActionShoot>();

        if (shootInput.WasPressedThisFrame || shootInput.WasReleasedThisFrame)
        {
            foreach (var materialMeshInfo in SystemAPI.Query<EnabledRefRW<MaterialMeshInfo>>().WithPresent<MaterialMeshInfo>().WithAll<LaserVisibilityTag>())
            {
                    materialMeshInfo.ValueRW = shootInput.IsPressed;
            }
        }
    }
}
