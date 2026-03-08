using Unity.Entities;
using UnityEngine;

public class PredatorAuthoring : MonoBehaviour
{
    class Baker : Baker<PredatorAuthoring>
    {
        public override void Bake(PredatorAuthoring authoring)
        {
            // The entity will be moved
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<PredatorTag>(entity);
            var starting = UnityEngine.Random.Range(5, 15);
            AddComponent(entity, new LifetimeComponent
            {
                Starting = starting,
                Current = starting,
                DecreasingFactor = 1f,
                AlwaysReproduce = false,
                Reproduced = false
            });
            AddComponent(entity, new VelocityComponent
            {
                Value = new Unity.Mathematics.float3(0f, 0f, 0f),
            });
        }
    }
}
