using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public partial struct LifetimeSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var targetQuery = SystemAPI.QueryBuilder().WithAll<LocalTransform,PreyTag>().Build();
        var targetTransforms = targetQuery.ToComponentDataArray<LocalTransform>(state.WorldUpdateAllocator);
        var preyPositions = CollectionHelper.CreateNativeArray<float3>(targetTransforms.Length,state.WorldUpdateAllocator);

        for (int i = 0; i < preyPositions.Length; i += 1)
        {
            preyPositions[i] = targetTransforms[i].Position;
        }
        var job = new PlantDecreasingFactorJob
        {
            PreyPositions = preyPositions
        };
        job.ScheduleParallel();
    }
}

[BurstCompile]
partial struct PlantDecreasingFactorJob : IJobEntity
{
    [ReadOnly] public NativeArray<float3> PreyPositions;

    // In source generation, a query is created from the parameters of Execute().
    // Here, the query will match all entities having a LocalTransform, PostTransformMatrix, and RotationSpeed component.
    // (In the scene, the root cube has a non-uniform scale, so it is given a PostTransformMatrix component in baking.)
    void Execute(in LocalTransform plantTransform, ref LifetimeComponent plantLifetime, in PlantTag _)
    {
        plantLifetime.DecreasingFactor = 1.0f;
        foreach (var prey in PreyPositions)
        {
            if (Vector3.SqrMagnitude(prey - plantTransform.Position) < Ex3Config.TouchingDistance * Ex3Config.TouchingDistance)
            {
                plantLifetime.DecreasingFactor *= 2f;
                break;
            }
        }
    }
}

