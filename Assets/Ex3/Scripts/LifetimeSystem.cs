using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateAfter(typeof(MoveSystem))]
public partial struct LifetimeSystem : ISystem
{
    private EntityQuery _preyQuery;
    private EntityQuery _plantQuery;
    private EntityQuery _predatorQuery;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GameConfig>();
        _preyQuery = SystemAPI.QueryBuilder().WithAll<LocalTransform, PreyTag>().Build();
        _plantQuery = SystemAPI.QueryBuilder().WithAll<LocalTransform, PlantTag>().Build();
        _predatorQuery = SystemAPI.QueryBuilder().WithAll<LocalTransform, PredatorTag>().Build();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
        var settings = SystemAPI.GetSingleton<GameConfig>();
        var touchingDistanceSq = settings.TouchingDistance * settings.TouchingDistance;

        var targetTransforms = _preyQuery.ToComponentDataArray<LocalTransform>(state.WorldUpdateAllocator);
        var preyPositions = CollectionHelper.CreateNativeArray<float3>(targetTransforms.Length, state.WorldUpdateAllocator);
        for (int i = 0; i < preyPositions.Length; i += 1)
        {
            preyPositions[i] = targetTransforms[i].Position;
        }

        targetTransforms = _plantQuery.ToComponentDataArray<LocalTransform>(state.WorldUpdateAllocator);
        var plantPositions = CollectionHelper.CreateNativeArray<float3>(targetTransforms.Length, state.WorldUpdateAllocator);
        for (int i = 0; i < plantPositions.Length; i += 1)
        {
            plantPositions[i] = targetTransforms[i].Position;
        }

        targetTransforms = _predatorQuery.ToComponentDataArray<LocalTransform>(state.WorldUpdateAllocator);
        var predatorPositions = CollectionHelper.CreateNativeArray<float3>(targetTransforms.Length, state.WorldUpdateAllocator);
        for (int i = 0; i < predatorPositions.Length; i += 1)
        {
            predatorPositions[i] = targetTransforms[i].Position;
        }

        var plantJob = new PlantDecreasingFactorJob
        {
            PreyPositions = preyPositions,
            TouchingDistanceSq = touchingDistanceSq
        };
        var plantHandle = plantJob.ScheduleParallel(state.Dependency);

        var preyJob = new PreyDecreasingFactorJob
        {
            PreyPositions = preyPositions,
            PlantPositions = plantPositions,
            PredatorPositions = predatorPositions,
            TouchingDistanceSq = touchingDistanceSq

        };
        var preyHandle = preyJob.ScheduleParallel(plantHandle);

        var predatortJob = new PredatorDecreasingFactorJob
        {
            PreyPositions = preyPositions,
            PredatorPositions = predatorPositions,
            TouchingDistanceSq = touchingDistanceSq
        };
        var predatorHandle = predatortJob.ScheduleParallel(preyHandle);

        var lifetimeJob = new LifetimeDecreaseJob
        {
            ECB = ecb,
            DeltaTime = SystemAPI.Time.DeltaTime,
        };
        var combinedHandle = JobHandle.CombineDependencies(
            plantHandle,
            preyHandle,
            predatorHandle
        );
        var lifetimeHandle = lifetimeJob.ScheduleParallel(combinedHandle);
        var plantScaleJob = new PlantScaleJob();
        state.Dependency = plantScaleJob.ScheduleParallel(lifetimeHandle);
    }
}

[BurstCompile]
partial struct PlantDecreasingFactorJob : IJobEntity
{
    [ReadOnly] public NativeArray<float3> PreyPositions;
    [ReadOnly] public float TouchingDistanceSq;

    // In source generation, a query is created from the parameters of Execute().
    // Here, the query will match all entities having a LocalTransform, PostTransformMatrix, and RotationSpeed component.
    // (In the scene, the root cube has a non-uniform scale, so it is given a PostTransformMatrix component in baking.)
    void Execute(in LocalTransform plantTransform, ref LifetimeComponent plantLifetime, in PlantTag _)
    {
        plantLifetime.DecreasingFactor = 1.0f;
        foreach (var prey in PreyPositions)
        {
            if (math.distancesq(prey, plantTransform.Position) < TouchingDistanceSq)
            {
                plantLifetime.DecreasingFactor *= 2f;
            }
        }
    }
}

[BurstCompile]
partial struct PreyDecreasingFactorJob : IJobEntity
{
    [ReadOnly] public NativeArray<float3> PredatorPositions;
    [ReadOnly] public NativeArray<float3> PlantPositions;
    [ReadOnly] public NativeArray<float3> PreyPositions;
    [ReadOnly] public float TouchingDistanceSq;

    void Execute(in LocalTransform preyTransform, ref LifetimeComponent preyLifetime, in PreyTag _)
    {
        preyLifetime.DecreasingFactor = 1.0f;
        foreach (var plant in PlantPositions)
        {
            if (math.distancesq(plant, preyTransform.Position) < TouchingDistanceSq)
            {
                preyLifetime.DecreasingFactor /= 2;
            }
        }

        foreach (var predator in PredatorPositions)
        {
            if (math.distancesq(predator, preyTransform.Position) < TouchingDistanceSq)
            {
                preyLifetime.DecreasingFactor *= 2f;
            }
        }

        if (!preyLifetime.Reproduced)
        {
            foreach (var prey in PreyPositions)
            {
                float distanceSq = math.distancesq(prey, preyTransform.Position);
                if (distanceSq > 0 && distanceSq < TouchingDistanceSq)
                {
                    preyLifetime.Reproduced = true;
                    break;

                }
            }
        }

    }
}

[BurstCompile]
partial struct PredatorDecreasingFactorJob : IJobEntity
{
    [ReadOnly] public NativeArray<float3> PredatorPositions;
    [ReadOnly] public NativeArray<float3> PreyPositions;
    [ReadOnly] public float TouchingDistanceSq;


    void Execute(in LocalTransform predatorTransform, ref LifetimeComponent predatorLifetime, in PredatorTag _)
    {
        predatorLifetime.DecreasingFactor = 1.0f;
        foreach (var prey in PreyPositions)
        {
            if (math.distancesq(prey, predatorTransform.Position) < TouchingDistanceSq)
            {
                predatorLifetime.DecreasingFactor /= 2;
            }
        }


        if (!predatorLifetime.Reproduced)
        {
            foreach (var predator in PredatorPositions)
            {
                float distanceSq = math.distancesq(predator, predatorTransform.Position);
                if (distanceSq > 0 && distanceSq < TouchingDistanceSq)
                {
                    predatorLifetime.Reproduced = true;
                    break;

                }
            }
        }

    }
}

[BurstCompile]
partial struct LifetimeDecreaseJob : IJobEntity

{
    public float DeltaTime;
    public EntityCommandBuffer.ParallelWriter ECB;
    void Execute([ChunkIndexInQuery] int sortKey, Entity entity, ref LifetimeComponent lifetime)
    {
        lifetime.Current -= DeltaTime * lifetime.DecreasingFactor;
        if (lifetime.Current > 0) return;

        if (lifetime.Reproduced || lifetime.AlwaysReproduce)
        {

            ECB.AddComponent<RespawnTag>(sortKey, entity);
        }
        else
        {
            ECB.SetEnabled(sortKey, entity, false);
        }
    }
}


[BurstCompile]
partial struct PlantScaleJob : IJobEntity
{
    void Execute(ref LocalTransform transform, in PlantTag _, in LifetimeComponent lifetime)
    {
        transform.Scale = math.clamp(lifetime.GetProgression() ,0 ,1);
    }
}