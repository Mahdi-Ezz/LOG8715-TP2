using Unity.Entities;
using Unity.Mathematics;

public struct PlantTag : IComponentData { }
public struct PreyTag : IComponentData { }
public struct PredatorTag : IComponentData { }
public struct RespawnTag : IComponentData { }

public struct VelocityComponent : IComponentData
{
    public float3 Value;
}

// Composant lifetime
public struct LifetimeComponent : IComponentData
{
    public float Current;
    public float Starting;
    public float DecreasingFactor;
    public bool Reproduced;
    public bool AlwaysReproduce;

    public float GetProgression() => Current / Starting;
}

public struct GameConfig : IComponentData
{
    public float HalfHeight;
    public float HalfWidth;
    //[SerializeField] public int plantCount = 200;
    //[SerializeField] public int preyCount = 200;
    //[SerializeField] public int predatorCount = 200;
    //[SerializeField] public int gridSize = 600;

    public float PreySpeed;
    public float PredatorSpeed;
    public float TouchingDistance;
}


