using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using static Unity.Mathematics.math;

public class ScoreSystem : JobComponentSystem {

    protected override JobHandle OnUpdate(JobHandle inputDeps) {
        var jobHandle = Entities
            .ForEach((ref CollisionSphere sphere, ref Translation translation, ref NonUniformScale scale,
            ref Player player, in Score score) => {
                var points = score.Value;
                var sqrtPoints = sqrt(points);
                sphere.Radius = 0.3f * sqrtPoints;
                scale.Value.xy = float2(sqrtPoints, sqrtPoints);
                player.Speed = 12f * pow(points, -0.15f);
                translation.Value.z = player.Speed - 12f;
            }).Schedule(inputDeps);
        return jobHandle;
    }
}
