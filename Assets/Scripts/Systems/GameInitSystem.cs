using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.NetCode;

[UpdateInGroup(typeof(ServerSimulationSystemGroup))]
public class GameInitSystem : ComponentSystem {

    [BurstCompile]
    struct SpawnJob : IJobParallelFor {
        [ReadOnly]
        public Entity spawn;
        public EntityCommandBuffer.Concurrent commandBuffer;

        public void Execute(int index) {
            commandBuffer.Instantiate(index, spawn);
        }
    }

    private EndSimulationEntityCommandBufferSystem commandBufferSystem;

    protected override void OnCreate() {
        RequireSingletonForUpdate<GameInit>();
        commandBufferSystem = World
            .GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate() {
        EntityCommandBuffer.Concurrent commandBuffer =
            commandBufferSystem.CreateCommandBuffer().ToConcurrent();
        var init = GetSingleton<GameInit>();
        var job = new SpawnJob {
            spawn = init.ParticlePrefab,
            commandBuffer = commandBuffer
        };
        var jobHandle = job.Schedule(init.NumberOfParticles, 16);
        commandBufferSystem.AddJobHandleForProducer(jobHandle);
        EntityManager.DestroyEntity(GetSingletonEntity<GameInit>());
    }
}
