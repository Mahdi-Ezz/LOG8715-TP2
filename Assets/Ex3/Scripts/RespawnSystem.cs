using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateAfter(typeof(LifetimeSystem))]
public partial struct RespawnSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GameConfig>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var settings = SystemAPI.GetSingleton<GameConfig>();

        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

        var respawnJob = new RespawnJob
        {
            Seed = (uint)(SystemAPI.Time.ElapsedTime * 1000) + 1,
            HalfHeight = settings.HalfHeight,
            HalfWidth = settings.HalfWidth,
            Ecb = ecb
        };

        state.Dependency = respawnJob.ScheduleParallel(state.Dependency);
    }
}

[BurstCompile]
partial struct RespawnJob : IJobEntity
{
    public uint Seed;
    public float HalfHeight;
    public float HalfWidth;
    public EntityCommandBuffer.ParallelWriter Ecb;

    void Execute([EntityIndexInQuery] int entityIndex, Entity entity, ref LocalTransform transform, in RespawnTag _)
    {
        var random = Unity.Mathematics.Random.CreateFromIndex((uint) (entityIndex + Seed));
        float x = random.NextFloat(-HalfWidth, HalfWidth); ;
        float y = random.NextFloat(-HalfHeight, HalfHeight);

        transform.Position = new float3(x, y, 0f);

        Ecb.RemoveComponent<RespawnTag>(entityIndex, entity);
    }
}