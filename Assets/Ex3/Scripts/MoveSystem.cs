using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public partial struct MoveSystem : ISystem
{

    private EntityQuery _preyQuery;
    private EntityQuery _plantQuery;
    private EntityQuery _predatorQuery;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GameConfig>();
        _preyQuery = SystemAPI.QueryBuilder().WithAll<LocalTransform, PreyTag>().Build();
        _plantQuery = SystemAPI.QueryBuilder().WithAll<LocalTransform, PlantTag>().Build();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var settings = SystemAPI.GetSingleton<GameConfig>();

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



        var movePreyTowardPlantJob = new MovePreyTowardPlantJob
        {
            PlantPositions = plantPositions,
            PreySpeed = settings.PreySpeed,
        };
        var movePreyTowardPlantHandle = movePreyTowardPlantJob.ScheduleParallel(state.Dependency);

        var movePredatorTowardPreyJob = new MovePredatorTowardPreyJob
        {
            PreyPositions = preyPositions,
            PredatorSpeed = settings.PredatorSpeed,
        };
        var movePredatorTowardPreyHandle = movePredatorTowardPreyJob.ScheduleParallel(movePreyTowardPlantHandle);
        var combinedHandle = JobHandle.CombineDependencies(
            movePredatorTowardPreyHandle,
            movePreyTowardPlantHandle
            );

        var velocityJob = new VelocityJob
        {
            DeltaTime = SystemAPI.Time.DeltaTime
        };
        var velocityHandle = velocityJob.ScheduleParallel(combinedHandle);
        state.Dependency = velocityHandle;

    }
}


[BurstCompile]
partial struct MovePreyTowardPlantJob : IJobEntity
{
    [ReadOnly] public NativeArray<float3> PlantPositions;
    [ReadOnly] public float PreySpeed;
    void Execute(in LocalTransform preyTransform, ref VelocityComponent preyVelocity, in PreyTag _)
    {
        var closestDistance = float.MaxValue;
        var closestPosition = preyTransform.Position;
        foreach (var plant in PlantPositions)
        {
            var distance = math.distancesq(preyTransform.Position, plant);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPosition = plant;
            }
        }

        var direction = closestPosition - preyTransform.Position;
        preyVelocity.Value = math.lengthsq(direction) > 0.01f ? math.normalize(direction) * PreySpeed : float3.zero;
    }
}

[BurstCompile]
partial struct MovePredatorTowardPreyJob : IJobEntity
{
    [ReadOnly] public NativeArray<float3> PreyPositions;
    [ReadOnly] public float PredatorSpeed;
    void Execute(in LocalTransform predatorTransform, ref VelocityComponent predatorVelocity, in PredatorTag _)
    {
        var closestDistance = float.MaxValue;
        var closestPosition = predatorTransform.Position;
        foreach (var plant in PreyPositions)
        {
            var distance = math.distancesq(predatorTransform.Position, plant);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPosition = plant;
            }
        }

        var direction = closestPosition - predatorTransform.Position;
        predatorVelocity.Value = math.lengthsq(direction) > 0.01f ? math.normalize(direction) * PredatorSpeed : float3.zero;
    }
}

[BurstCompile]
partial struct VelocityJob : IJobEntity
{
    public float DeltaTime;
    void Execute(ref LocalTransform transform, in VelocityComponent velocity)
    {
        transform.Position += velocity.Value * DeltaTime;
    }
}