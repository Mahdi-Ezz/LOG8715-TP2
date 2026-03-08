using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class Ex4SpawnerAuthoring : MonoBehaviour
{
    public Ex3Config config;
    public GameObject predatorPrefab;
    public GameObject preyPrefab;
    public GameObject plantPrefab;

    class Baker : Baker<Ex4SpawnerAuthoring>
    {
        public override void Bake(Ex4SpawnerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);

            var plantEntity = GetEntity(authoring.plantPrefab, TransformUsageFlags.Dynamic);
            var preyEntity = GetEntity(authoring.preyPrefab, TransformUsageFlags.Dynamic);
            var predatorEntity = GetEntity(authoring.predatorPrefab, TransformUsageFlags.Dynamic);
            var size = (float)authoring.config.gridSize;
            var ratio = 2.088;

            int halfHeight = (int)Math.Round(Math.Sqrt(size / ratio)) / 2;
            int halfWidth = (int)Math.Round(size / (halfHeight * 2f)) / 2;

            AddComponent(entity, new Ex4SpawnerData
            {
                PlantPrefab = plantEntity,
                PreyPrefab = preyEntity,
                PredatorPrefab = predatorEntity,
                PlantCount = authoring.config.plantCount,
                PreyCount = authoring.config.preyCount,
                PredatorCount = authoring.config.predatorCount,
                HalfWidth = halfWidth,
                HalfHeight = halfHeight,
                Spawned = 0
            });
            AddComponent(entity, new GameConfig
            {
                HalfHeight = halfHeight,
                HalfWidth = halfWidth,
                PreySpeed = Ex3Config.PreySpeed,
                PredatorSpeed = Ex3Config.PredatorSpeed,
                TouchingDistance = Ex3Config.TouchingDistance,
            });
        }
    }
}

public struct Ex4SpawnerData : IComponentData
{
    public Entity PlantPrefab;
    public Entity PreyPrefab;
    public Entity PredatorPrefab;

    public int PlantCount;
    public int PreyCount;
    public int PredatorCount;

    public int HalfWidth;
    public int HalfHeight;

    public byte Spawned;
}

[BurstCompile]
[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct Ex4SpawnSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Ex4SpawnerData>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var spawner in SystemAPI.Query<RefRW<Ex4SpawnerData>>())
        {
            if (spawner.ValueRO.Spawned != 0)
                continue;

            uint plantSeedBase = 1u;
            uint preySeedBase = plantSeedBase + (uint)spawner.ValueRO.PlantCount;
            uint predatorSeedBase = preySeedBase + (uint)spawner.ValueRO.PreyCount;

            SpawnMany(
                ref ecb,
                spawner.ValueRO.PlantPrefab,
                spawner.ValueRO.PlantCount,
                spawner.ValueRO.HalfWidth,
                spawner.ValueRO.HalfHeight,
                plantSeedBase
            );

            SpawnMany(
                ref ecb,
                spawner.ValueRO.PreyPrefab,
                spawner.ValueRO.PreyCount,
                spawner.ValueRO.HalfWidth,
                spawner.ValueRO.HalfHeight,
                preySeedBase
            );

            SpawnMany(
                ref ecb,
                spawner.ValueRO.PredatorPrefab,
                spawner.ValueRO.PredatorCount,
                spawner.ValueRO.HalfWidth,
                spawner.ValueRO.HalfHeight,
                predatorSeedBase
            );

            spawner.ValueRW.Spawned = 1;
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    private static void SpawnMany(
        ref EntityCommandBuffer ecb,
        Entity prefab,
        int count,
        int halfWidth,
        int halfHeight,
        uint seedBase)
    {
        for (int i = 0; i < count; i++)
        {

            Entity entity = ecb.Instantiate(prefab);

            var random = Unity.Mathematics.Random.CreateFromIndex(seedBase + (uint)(i + 1));

            float x = random.NextInt(-halfWidth, halfWidth);
            float y = random.NextInt(-halfHeight, halfHeight);

            ecb.SetComponent(entity, LocalTransform.FromPosition(new float3(x, y, 0f)));
        }
    }
}