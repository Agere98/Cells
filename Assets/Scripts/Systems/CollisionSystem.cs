using Unity.Entities;
using Unity.Burst;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using Unity.NetCode;

[UpdateInGroup(typeof(ServerSimulationSystemGroup))]
[UpdateAfter(typeof(ParticleSpawnSystem))]
public class CollisionSystem : JobComponentSystem {

    [BurstCompile]
    struct CollisionCheckJob : IJobChunk {
        public EntityCommandBuffer.Concurrent commandBuffer;
        [ReadOnly] public NativeArray<ArchetypeChunk> dynamicEntityChunks;
        [ReadOnly] public ArchetypeChunkEntityType entityType;
        [ReadOnly] public ArchetypeChunkComponentType<Translation> translationType;
        [ReadOnly] public ArchetypeChunkComponentType<CollisionSphere> sphereType;
        public ArchetypeChunkComponentType<Score> scoreType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
            var colliderTranslation = chunk.GetNativeArray(translationType);
            var colliderSphere = chunk.GetNativeArray(sphereType);
            var colliderScore = chunk.GetNativeArray(scoreType);
            var colliderEntity = chunk.GetNativeArray(entityType);

            for (int collider = 0; collider < colliderTranslation.Length; collider++) {
                var colliderPosition = colliderTranslation[collider].Value.xy;
                var colliderRadius = colliderSphere[collider].Radius;

                for (int dec = 0; dec < dynamicEntityChunks.Length; dec++) {
                    var dynamicTranslation = dynamicEntityChunks[dec].GetNativeArray(translationType);
                    var dynamicSphere = dynamicEntityChunks[dec].GetNativeArray(sphereType);
                    var dynamicScore = dynamicEntityChunks[dec].GetNativeArray(scoreType);

                    for (int dynamic = 0; dynamic < dynamicTranslation.Length; dynamic++) {
                        var dynamicPosition = dynamicTranslation[dynamic].Value.xy;
                        var dynamicRadius = dynamicSphere[dynamic].Radius;

                        if (Enclose(dynamicPosition, dynamicRadius, colliderPosition, colliderRadius)) {
                            commandBuffer.AddComponent<Primer>(chunkIndex, colliderEntity[collider]);
                            var score = dynamicScore[dynamic];
                            score.Value += colliderScore[collider].Value;
                            dynamicScore[dynamic] = score;
                        }
                    }
                }
            }
        }
    }

    struct ChunkCleanupJob : IJob {
        [ReadOnly]
        [DeallocateOnJobCompletion]
        public NativeArray<ArchetypeChunk> chunks;
        public void Execute() { }
    }

    private BeginSimulationEntityCommandBufferSystem commandBufferSystem;
    private EntityQuery collisionEntityQuery;
    private EntityQuery dynamicEntityQuery;

    protected override void OnCreate() {
        commandBufferSystem = World
            .GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
        var collision = new EntityQueryDesc {
            All = new ComponentType[] {
                ComponentType.ReadOnly<Translation>(),
                ComponentType.ReadOnly<CollisionSphere>(),
                ComponentType.ReadWrite<Score>()
            },
            None = new ComponentType[] {
                ComponentType.ReadOnly<Primer>()
            }
        };
        var dynamic = new EntityQueryDesc {
            All = new ComponentType[] {
                ComponentType.ReadOnly<Translation>(),
                ComponentType.ReadOnly<CollisionSphere>(),
                ComponentType.ReadWrite<Score>()
            },
            None = new ComponentType[] {
                ComponentType.ReadOnly<CollisionStatic>(),
                ComponentType.ReadOnly<Primer>()
            }
        };
        collisionEntityQuery = GetEntityQuery(collision);
        dynamicEntityQuery = GetEntityQuery(dynamic);
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps) {
        EntityCommandBuffer.Concurrent commandBuffer =
            commandBufferSystem.CreateCommandBuffer().ToConcurrent();
        JobHandle dynamicEntityHandle;
        var job = new CollisionCheckJob {
            commandBuffer = commandBuffer,
            dynamicEntityChunks = dynamicEntityQuery.CreateArchetypeChunkArray(Allocator.TempJob,
                out dynamicEntityHandle),
            translationType = GetArchetypeChunkComponentType<Translation>(true),
            sphereType = GetArchetypeChunkComponentType<CollisionSphere>(true),
            scoreType = GetArchetypeChunkComponentType<Score>(false),
            entityType = GetArchetypeChunkEntityType()
        };
        var jobDeps = JobHandle.CombineDependencies(inputDeps, dynamicEntityHandle);
        var handle = job.Schedule(collisionEntityQuery, jobDeps);
        commandBufferSystem.AddJobHandleForProducer(handle);
        var cleanupJob = new ChunkCleanupJob {
            chunks = job.dynamicEntityChunks
        };
        return cleanupJob.Schedule(handle);
    }

    public static bool Enclose(float2 enclosingPosition, float enclosingRadius,
        float2 enclosedPosition, float enclosedRadius, float tolerance = 0.5f) {
        return enclosingRadius > enclosedRadius &&
            distance(enclosingPosition, enclosedPosition) <
            (enclosingRadius - enclosedRadius * (1f - tolerance));
    }
}
