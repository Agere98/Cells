using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.NetCode;

[UpdateInGroup(typeof(ServerSimulationSystemGroup))]
public class ParticleSpawnSystem : JobComponentSystem {

    private EndSimulationEntityCommandBufferSystem commandBufferSystem;

    protected override void OnCreate() {
        commandBufferSystem = World
            .GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps) {
        EntityCommandBuffer.Concurrent commandBuffer =
            commandBufferSystem.CreateCommandBuffer().ToConcurrent();
        var time = Time.ElapsedTime;
        var seed = (uint)(time * 1000000d);
        var levelBounds = GetSingleton<GameSettings>().levelBounds;

        var primerJobHandle = Entities
            .ForEach((Entity entity, int entityInQueryIndex, ref ParticleSpawner particle, in Primer primer) => {
                var rng = new Random(seed + (uint)(entityInQueryIndex * 1000));
                particle.RespawnTime = time + rng.NextDouble(primer.TimerLB, primer.TimerUB);
                if (particle.RespawnTime <= time) {
                    var spawned = commandBuffer.Instantiate(entityInQueryIndex, particle.Prefab);
                    Translation spawnedOffset = new Translation() {
                        Value = math.float3(
                            rng.NextFloat(levelBounds.Left, levelBounds.Right),
                            rng.NextFloat(levelBounds.Bottom, levelBounds.Top),
                            -1f)
                    };
                    commandBuffer.SetComponent(entityInQueryIndex, spawned, spawnedOffset);
                    commandBuffer.DestroyEntity(entityInQueryIndex, entity);
                }
                else {
                    commandBuffer.RemoveComponent<Primer>(entityInQueryIndex, entity);
                }
            }).Schedule(inputDeps);

        var spawnerJobHandle = Entities
            .WithNone<Primer>()
            .ForEach((Entity entity, int entityInQueryIndex, in ParticleSpawner particle) => {
                var rng = new Random(seed + (uint)(entityInQueryIndex * 1000));
                if (particle.RespawnTime <= time && particle.RespawnTime > 0d) {
                    var spawned = commandBuffer.Instantiate(entityInQueryIndex, particle.Prefab);
                    Translation spawnedOffset = new Translation() {
                        Value = math.float3(
                            rng.NextFloat(levelBounds.Left, levelBounds.Right),
                            rng.NextFloat(levelBounds.Bottom, levelBounds.Top),
                            -1f)
                    };
                    commandBuffer.SetComponent(entityInQueryIndex, spawned, spawnedOffset);
                    commandBuffer.DestroyEntity(entityInQueryIndex, entity);
                }
            }).Schedule(primerJobHandle);

        commandBufferSystem.AddJobHandleForProducer(spawnerJobHandle);
        return spawnerJobHandle;
    }
}
