using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

public struct LaserVisibilityTag : IComponentData
{ }

public struct LaserVisibilityUninitializedTag : IComponentData, IEnableableComponent
{ }

public class LaserVisibilityAuthoring : MonoBehaviour
{
    private class Baker : Baker<LaserVisibilityAuthoring>
    {
        public override void Bake(LaserVisibilityAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.None);

            AddComponent<LaserVisibilityTag>(entity);
            AddComponent<LaserVisibilityUninitializedTag>(entity);
        }
    }
}

public partial struct LaserVisibilityInitializationSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (uninitializedTag, materialMeshInfo) in SystemAPI.Query<EnabledRefRW<LaserVisibilityUninitializedTag>, EnabledRefRW<MaterialMeshInfo>>())
        {
            materialMeshInfo.ValueRW = false;
            uninitializedTag.ValueRW = false;
        }
    }
}