using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.NetCode;

[UpdateInWorld(UpdateInWorld.TargetWorld.Client)]
[UpdateInGroup(typeof(GhostPredictionSystemGroup))]
public class CameraFollowSystem : JobComponentSystem {

    [BurstCompile]
    [RequireComponentTag(typeof(CameraProxy))]
    struct CameraFollowJob : IJobForEach<Translation> {
        [ReadOnly]
        [DeallocateOnJobCompletion]
        public NativeArray<Translation> target;
        [ReadOnly]
        [DeallocateOnJobCompletion]
        public NativeArray<NonUniformScale> scale;

        public void Execute(ref Translation cameraPosition) {
            if (target.Length == 0) return;
            cameraPosition.Value.xy = target[0].Value.xy;
            cameraPosition.Value.z = -scale[0].Value.x - 105f;
        }
    }

    private EntityQuery followTargetQuery;

    protected override void OnCreate() {
        followTargetQuery = GetEntityQuery(
            ComponentType.ReadOnly<Translation>(),
            ComponentType.ReadOnly<NonUniformScale>(),
            ComponentType.ReadOnly<CameraTarget>());
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps) {
        var job = new CameraFollowJob {
            target = followTargetQuery.ToComponentDataArray<Translation>(Allocator.TempJob),
            scale = followTargetQuery.ToComponentDataArray<NonUniformScale>(Allocator.TempJob)
        };
        return job.Schedule(this, inputDeps);
    }
}
