using Unity.Entities;
using UnityEngine;

public struct EnemyTag : IComponentData
{ }

public struct EnemyAttack : IComponentData
{
    public float AttackDamage;
    public float CooldownTime;
}

public struct EnemyCooldownExpirationTimestamp : IComponentData, IEnableableComponent
{
    public double Value;
}

public class EnemyAuthoring : MonoBehaviour
{
    public float AttackDamage = 10.0f;
    public float CooldownTime = 1.0f;

    private class Baker : Baker<EnemyAuthoring>
    {
        public override void Bake(EnemyAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

            AddComponent<EnemyTag>(entity);
            AddComponent(entity, new EnemyAttack
            {
                AttackDamage = authoring.AttackDamage,
                CooldownTime = authoring.CooldownTime
            });
            AddComponent<EnemyCooldownExpirationTimestamp>(entity);
            SetComponentEnabled<EnemyCooldownExpirationTimestamp>(entity, false);
        }
    }
}
