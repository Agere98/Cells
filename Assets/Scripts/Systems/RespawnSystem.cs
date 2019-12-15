using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.NetCode;

[UpdateInGroup(typeof(ServerSimulationSystemGroup))]
public class RespawnSystem : ComponentSystem {

    protected override void OnUpdate() {
        var levelBounds = GetSingleton<GameSettings>().levelBounds;
        var rng = new Random((uint)(Time.ElapsedTime * 100000));
        Entities
            .WithAll<Player, Primer>()
            .ForEach((Entity entity, ref Translation translation, ref Score score) => {
                float spawnX = rng.NextFloat(levelBounds.Left, levelBounds.Right);
                float spawnY = rng.NextFloat(levelBounds.Bottom, levelBounds.Top);
                translation.Value = new float3(spawnX, spawnY, 0f);
                score.Value = 4;
                EntityManager.RemoveComponent<Primer>(entity);
            });
    }
}
