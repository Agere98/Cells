using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using Unity.NetCode;

[UpdateInGroup(typeof(ClientSimulationSystemGroup))]
public class PlayerInputSystem : ComponentSystem {

    protected override void OnCreate() {
        RequireSingletonForUpdate<NetworkIdComponent>();
        RequireSingletonForUpdate<EnableCellsGhostReceiveSystemComponent>();
    }

    protected override void OnUpdate() {
        var localInput = GetSingleton<CommandTargetComponent>().targetEntity;
        if (localInput == Entity.Null) {
            var localPlayerId = GetSingleton<NetworkIdComponent>().Value;
            Entities.WithNone<PlayerInput>().ForEach((Entity entity, ref NetworkPlayer player) => {
                if (player.PlayerId == localPlayerId) {
                    PostUpdateCommands.AddBuffer<PlayerInput>(entity);
                    PostUpdateCommands.SetComponent(GetSingletonEntity<CommandTargetComponent>(), new CommandTargetComponent { targetEntity = entity });
                }
            });
            return;
        }
        var input = default(PlayerInput);
        input.tick = World.GetExistingSystem<ClientSimulationSystemGroup>().ServerTick;
        input.horizontal = (int)Input.GetAxisRaw("Horizontal");
        input.vertical = (int)Input.GetAxisRaw("Vertical");
        var inputBuffer = EntityManager.GetBuffer<PlayerInput>(localInput);
        inputBuffer.AddCommandData(input);
    }
}

[UpdateInGroup(typeof(GhostPredictionSystemGroup))]
public class ProcessInputSystem : ComponentSystem {
    protected override void OnUpdate() {
        var group = World.GetExistingSystem<GhostPredictionSystemGroup>();
        var tick = group.PredictingTick;
        var deltaTime = Time.DeltaTime;
        var levelBounds = GetSingleton<GameSettings>().levelBounds;
        Entities.ForEach((DynamicBuffer<PlayerInput> inputBuffer, ref Translation translation, ref Player player, ref PredictedGhostComponent prediction) => {
            if (!GhostPredictionSystemGroup.ShouldPredict(tick, prediction))
                return;
            PlayerInput input;
            inputBuffer.GetDataAtTick(tick, out input);
            float2 direction = float2(input.horizontal, input.vertical);
            if (lengthsq(direction) > 1f) direction = normalize(direction);
            translation.Value += float3(direction, 0f) * player.Speed * deltaTime;
            if (translation.Value.y < levelBounds.Bottom) translation.Value.y = levelBounds.Bottom;
            if (translation.Value.y > levelBounds.Top) translation.Value.y = levelBounds.Top;
            if (translation.Value.x < levelBounds.Left) translation.Value.x = levelBounds.Left;
            if (translation.Value.x > levelBounds.Right) translation.Value.x = levelBounds.Right;
        });
    }
}
