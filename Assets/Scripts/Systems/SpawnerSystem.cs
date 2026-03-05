using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public partial struct SpawnerSystem : ISystem
{
    private float nextSpawn;

    private Random random;

    private int counter;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Spawner>();

        random = new Random((uint)System.DateTime.Now.Ticks);

        counter = 0;
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        Spawner spawner = SystemAPI.GetSingleton<Spawner>();

        if (nextSpawn < SystemAPI.Time.ElapsedTime)
        {
            Entity newEntity = state.EntityManager.Instantiate(spawner.Prefab);

            float3 randomOffset = (random.NextFloat3() - 0.5f) * 10f;
            randomOffset.y = 0;

            float3 newPosition = spawner.SpawnPosition + randomOffset;

            state.EntityManager.SetComponentData(newEntity, LocalTransform.FromPosition(newPosition));

            nextSpawn = (float)SystemAPI.Time.ElapsedTime + spawner.SpawnRate;

            //if (counter % 100 == 0)
            //{
            //    Debug.Log($"EnemyCount({counter})");
            //}
            //++counter;
        }
    }
}