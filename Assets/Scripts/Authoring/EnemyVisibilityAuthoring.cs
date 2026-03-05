using Unity.Entities;
using UnityEngine;

public struct EnemyVisibilityTag : IComponentData
{ }

[RequireComponent(typeof(CharacterVisibilityAuthoring))]
public class EnemyVisibilityAuthoring : MonoBehaviour
{
    private class Baker : Baker<EnemyVisibilityAuthoring>
    {
        public override void Bake(EnemyVisibilityAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

            AddComponent<EnemyVisibilityTag>(entity);
        }
    }
}
