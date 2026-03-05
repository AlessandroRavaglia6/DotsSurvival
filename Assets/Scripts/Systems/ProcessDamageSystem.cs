using Unity.Burst;
using Unity.Entities;

[BurstCompile]
public partial struct ProcessDamageSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (currentHealth, damageThisFrame, entity) in SystemAPI.Query<RefRW<CurrentHealth>, DynamicBuffer<DamageThisFrame>>().WithPresent<DestroyEntityTag>().WithEntityAccess())
        {
            if (damageThisFrame.IsEmpty)
            {
                continue;
            }

            foreach (var damage in damageThisFrame)
            {
                currentHealth.ValueRW.Value -= damage.Amount;
            }

            damageThisFrame.Clear();

            if (currentHealth.ValueRO.Value <= 0.0f)
            {
                SystemAPI.SetComponentEnabled<DestroyEntityTag>(entity, true);
            }
        }
    }
}
