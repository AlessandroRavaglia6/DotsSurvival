using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;

public enum PlayerAttackType : byte // Update the buffer capacity for player attacks data when updating this enum
{
    Laser
}

[System.Serializable]
public struct PlayerAttackInitializationData
{
    public PlayerAttackType AttackType;
    public float AttackDamage;
    public float CooldownTime;
}

public struct PlayerAttackConfigs : IComponentData
{
    public CollisionFilter AttackCollisionFilter;
}

public struct LaserAttackConfigs : IComponentData
{
    public float AttackRange;
}

public struct PlayerTag : IComponentData
{ }

public struct PlayerJumpForce : IComponentData
{
    public float Value;
}

public struct PlayerThrowForce : IComponentData
{
    public float Value;
}

public struct GrenadePrefab : IComponentData
{
    public Entity Value;
}

[RequireComponent(typeof(CharacterAuthoring))]
public class PlayerAuthoring : MonoBehaviour
{
    public GameObject GrenadePrefab;

    public float JumpForce = 70.0f;
    public float ThrowForce = 20.0f;

    public List<PlayerAttackInitializationData> PlayerAttackInitializationList = new List<PlayerAttackInitializationData>
    {
        new PlayerAttackInitializationData
        {
            AttackType = PlayerAttackType.Laser,
            AttackDamage = 50.0f,
            CooldownTime = 0.05f
        }
    };

    public float LaserAttackRange = 12.0f;

    private class Baker : Baker<PlayerAuthoring>
    {
        public override void Bake(PlayerAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

            AddComponent<PlayerTag>(entity);
            AddComponent(entity, new PlayerJumpForce
            {
                Value = authoring.JumpForce
            });
            AddComponent(entity, new PlayerThrowForce
            {
                Value = authoring.ThrowForce
            });
            AddComponent<CameraTarget>(entity);
            AddComponent<CameraTargetUninitializedTag>(entity);

            var attackDataBuffer = AddBuffer<AttackData>(entity);
            foreach (var attackInitData in authoring.PlayerAttackInitializationList)
            {
                attackDataBuffer.Insert((byte)attackInitData.AttackType, new AttackData
                {
                    AttackDamage = attackInitData.AttackDamage,
                    CooldownTime = attackInitData.CooldownTime
                });
            }

            var attackCooldownBuffer = AddBuffer<AttackCooldownExpirationTimestamp>(entity);

            int playerAttackTypeCount = Enum.GetValues(typeof(PlayerAttackType)).Length;
            for (int i = 0; i < playerAttackTypeCount; ++i)
            {
                attackCooldownBuffer.Insert(i, new AttackCooldownExpirationTimestamp { Value = 0.0 });
            }

            var enemyLayer = LayerMask.NameToLayer("Enemy");
            var enemyLayerMask = (uint)math.pow(2, enemyLayer);
            var attackCollisionFiler = new CollisionFilter
            {
                BelongsTo = uint.MaxValue,
                CollidesWith  = enemyLayerMask
            };

            AddComponent(entity, new PlayerAttackConfigs
            {
                AttackCollisionFilter = attackCollisionFiler,
            });

            AddComponent(entity, new LaserAttackConfigs { AttackRange = authoring.LaserAttackRange });

            AddComponent(entity, new GrenadePrefab
            {
                Value = GetEntity(authoring.GrenadePrefab, TransformUsageFlags.Dynamic),
            });
        }
    }
}
