using Unity.Entities;
using UnityEngine;

public struct PlayerVisibilityTag : IComponentData
{ }

[RequireComponent(typeof(CharacterVisibilityAuthoring))]
public class PlayerVisibilityAuthoring : MonoBehaviour
{
    private class Baker : Baker<PlayerVisibilityAuthoring>
    {
        public override void Bake(PlayerVisibilityAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

            AddComponent<PlayerVisibilityTag>(entity);
        }
    }
}
