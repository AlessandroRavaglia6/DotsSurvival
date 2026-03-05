using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class CharacterVisibilityAuthoring : MonoBehaviour
{
    private class Baker : Baker<CharacterVisibilityAuthoring>
    {
        public override void Bake(CharacterVisibilityAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

            AddComponent(entity, new CharacterVisibilityRotation
            {
                Rotation = quaternion.LookRotation(new float3(0.0f, 0.0f, 1.0f), new float3(0.0f, 1.0f, 0.0f))
            });
        }
    }
}
