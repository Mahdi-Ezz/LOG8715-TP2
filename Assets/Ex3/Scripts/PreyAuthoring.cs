using Unity.Entities;
using UnityEngine;

public class PreyAuthoring : MonoBehaviour
{
    class Baker : Baker<PreyAuthoring>
    {
        public override void Bake(PreyAuthoring authoring)
        {
            // The entity will be moved
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<PreyTag>(entity);
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
                Value = new Unity.Mathematics.float3(0f,0f,0f),
            });
        }
    }
}
