﻿using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;

// Control system updating in the default world
[UpdateInWorld(UpdateInWorld.TargetWorld.Default)]
public class Game : ComponentSystem {
    // Singleton component to trigger connections once from a control system
    struct InitGameComponent : IComponentData { }

    protected override void OnCreate() {
        RequireSingletonForUpdate<InitGameComponent>();
        // Create singleton, require singleton for update so system runs once
        EntityManager.CreateEntity(typeof(InitGameComponent));
    }

    protected override void OnUpdate() {
        // Destroy singleton to prevent system from running again
        EntityManager.DestroyEntity(GetSingletonEntity<InitGameComponent>());
        foreach (var world in World.AllWorlds) {
            var network = world.GetExistingSystem<NetworkStreamReceiveSystem>();
            if (world.GetExistingSystem<ClientSimulationSystemGroup>() != null) {
                // Client worlds automatically connect to localhost
                NetworkEndPoint ep = NetworkEndPoint.LoopbackIpv4;
                ep.Port = 7979;
                network.Connect(ep);
            }
            else if (world.GetExistingSystem<ServerSimulationSystemGroup>() != null) {
                // Server world automatically listens for connections from any host
                NetworkEndPoint ep = NetworkEndPoint.AnyIpv4;
                ep.Port = 7979;
                network.Listen(ep);
            }
        }
    }
}

[BurstCompile]
public struct GoInGameRequest : IRpcCommand {
    public void Deserialize(DataStreamReader reader, ref DataStreamReader.Context ctx) { }

    public void Serialize(DataStreamWriter writer) { }

    [BurstCompile]
    private static void InvokeExecute(ref RpcExecutor.Parameters parameters) {
        RpcExecutor.ExecuteCreateRequestComponent<GoInGameRequest>(ref parameters);
    }

    public PortableFunctionPointer<RpcExecutor.ExecuteDelegate> CompileExecute() {
        return new PortableFunctionPointer<RpcExecutor.ExecuteDelegate>(InvokeExecute);
    }
}

// The system that makes the RPC request component transfer
public class GoInGameRequestSystem : RpcCommandRequestSystem<GoInGameRequest> { }

// When client has a connection with network id, go in game and tell server to also go in game
[UpdateInGroup(typeof(ClientSimulationSystemGroup))]
public class GoInGameClientSystem : ComponentSystem {

    protected override void OnCreate() { }

    protected override void OnUpdate() {
        Entities.WithNone<NetworkStreamInGame>().ForEach((Entity ent, ref NetworkIdComponent id) => {
            PostUpdateCommands.AddComponent<NetworkStreamInGame>(ent);
            var req = PostUpdateCommands.CreateEntity();
            PostUpdateCommands.AddComponent<GoInGameRequest>(req);
            PostUpdateCommands.AddComponent(req, new SendRpcCommandRequestComponent { TargetConnection = ent });
        });
    }
}

// When server receives go in game request, go in game and delete request
[UpdateInGroup(typeof(ServerSimulationSystemGroup))]
public class GoInGameServerSystem : ComponentSystem {
    protected override void OnUpdate() {
        var seed = (uint)(Time.ElapsedTime * 1000000);
        Entities.WithNone<SendRpcCommandRequestComponent>().ForEach((Entity reqEnt, ref GoInGameRequest req, ref ReceiveRpcCommandRequestComponent reqSrc) => {
            PostUpdateCommands.AddComponent<NetworkStreamInGame>(reqSrc.SourceConnection);
            UnityEngine.Debug.Log(string.Format("Server setting connection {0} to in game", EntityManager.GetComponentData<NetworkIdComponent>(reqSrc.SourceConnection).Value));
            var ghostCollection = GetSingleton<GhostPrefabCollectionComponent>();
            var ghostId = CellsGhostSerializerCollection.FindGhostType<NetPlayerParticleSnapshotData>();
            var prefab = EntityManager.GetBuffer<GhostPrefabBuffer>(ghostCollection.serverPrefabs)[ghostId].Value;
            var player = EntityManager.Instantiate(prefab);

            EntityManager.AddComponent<Primer>(player);
            EntityManager.SetComponentData(player, new NetworkPlayer { PlayerId = EntityManager.GetComponentData<NetworkIdComponent>(reqSrc.SourceConnection).Value });
            PostUpdateCommands.AddBuffer<PlayerInput>(player);

            PostUpdateCommands.SetComponent(reqSrc.SourceConnection, new CommandTargetComponent { targetEntity = player });

            PostUpdateCommands.DestroyEntity(reqEnt);
        });
    }
}

[UpdateInGroup(typeof(ServerSimulationSystemGroup))]
[UpdateAfter(typeof(CollisionSystem))]
public class DisconnectSystem : JobComponentSystem {

    private BeginSimulationEntityCommandBufferSystem m_Barrier;

    [RequireComponentTag(typeof(NetworkStreamDisconnected))]
    struct DisconnectJob : IJobForEach<CommandTargetComponent, NetworkIdComponent> {
        public EntityCommandBuffer commandBuffer;

        public void Execute(ref CommandTargetComponent state, [ReadOnly]ref NetworkIdComponent id) {
            if (state.targetEntity != Entity.Null) {
                commandBuffer.DestroyEntity(state.targetEntity);
                state.targetEntity = Entity.Null;
                UnityEngine.Debug.Log($"Player {id.Value} has been disconnected from the server");
            }
        }
    }

    protected override void OnCreate() {
        m_Barrier = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps) {
        var job = new DisconnectJob { commandBuffer = m_Barrier.CreateCommandBuffer() };
        var handle = job.ScheduleSingle(this, inputDeps);
        m_Barrier.AddJobHandleForProducer(handle);
        return handle;
    }
}
