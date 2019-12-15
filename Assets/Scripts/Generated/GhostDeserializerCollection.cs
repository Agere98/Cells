using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Networking.Transport;
using Unity.NetCode;

public struct CellsGhostDeserializerCollection : IGhostDeserializerCollection
{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public string[] CreateSerializerNameList()
    {
        var arr = new string[]
        {
            "NetPlayerParticleGhostSerializer",
            "NetSpawnedParticleGhostSerializer",
        };
        return arr;
    }

    public int Length => 2;
#endif
    public void Initialize(World world)
    {
        var curNetPlayerParticleGhostSpawnSystem = world.GetOrCreateSystem<NetPlayerParticleGhostSpawnSystem>();
        m_NetPlayerParticleSnapshotDataNewGhostIds = curNetPlayerParticleGhostSpawnSystem.NewGhostIds;
        m_NetPlayerParticleSnapshotDataNewGhosts = curNetPlayerParticleGhostSpawnSystem.NewGhosts;
        curNetPlayerParticleGhostSpawnSystem.GhostType = 0;
        var curNetSpawnedParticleGhostSpawnSystem = world.GetOrCreateSystem<NetSpawnedParticleGhostSpawnSystem>();
        m_NetSpawnedParticleSnapshotDataNewGhostIds = curNetSpawnedParticleGhostSpawnSystem.NewGhostIds;
        m_NetSpawnedParticleSnapshotDataNewGhosts = curNetSpawnedParticleGhostSpawnSystem.NewGhosts;
        curNetSpawnedParticleGhostSpawnSystem.GhostType = 1;
    }

    public void BeginDeserialize(JobComponentSystem system)
    {
        m_NetPlayerParticleSnapshotDataFromEntity = system.GetBufferFromEntity<NetPlayerParticleSnapshotData>();
        m_NetSpawnedParticleSnapshotDataFromEntity = system.GetBufferFromEntity<NetSpawnedParticleSnapshotData>();
    }
    public bool Deserialize(int serializer, Entity entity, uint snapshot, uint baseline, uint baseline2, uint baseline3,
        DataStreamReader reader,
        ref DataStreamReader.Context ctx, NetworkCompressionModel compressionModel)
    {
        switch (serializer)
        {
            case 0:
                return GhostReceiveSystem<CellsGhostDeserializerCollection>.InvokeDeserialize(m_NetPlayerParticleSnapshotDataFromEntity, entity, snapshot, baseline, baseline2,
                baseline3, reader, ref ctx, compressionModel);
            case 1:
                return GhostReceiveSystem<CellsGhostDeserializerCollection>.InvokeDeserialize(m_NetSpawnedParticleSnapshotDataFromEntity, entity, snapshot, baseline, baseline2,
                baseline3, reader, ref ctx, compressionModel);
            default:
                throw new ArgumentException("Invalid serializer type");
        }
    }
    public void Spawn(int serializer, int ghostId, uint snapshot, DataStreamReader reader,
        ref DataStreamReader.Context ctx, NetworkCompressionModel compressionModel)
    {
        switch (serializer)
        {
            case 0:
                m_NetPlayerParticleSnapshotDataNewGhostIds.Add(ghostId);
                m_NetPlayerParticleSnapshotDataNewGhosts.Add(GhostReceiveSystem<CellsGhostDeserializerCollection>.InvokeSpawn<NetPlayerParticleSnapshotData>(snapshot, reader, ref ctx, compressionModel));
                break;
            case 1:
                m_NetSpawnedParticleSnapshotDataNewGhostIds.Add(ghostId);
                m_NetSpawnedParticleSnapshotDataNewGhosts.Add(GhostReceiveSystem<CellsGhostDeserializerCollection>.InvokeSpawn<NetSpawnedParticleSnapshotData>(snapshot, reader, ref ctx, compressionModel));
                break;
            default:
                throw new ArgumentException("Invalid serializer type");
        }
    }

    private BufferFromEntity<NetPlayerParticleSnapshotData> m_NetPlayerParticleSnapshotDataFromEntity;
    private NativeList<int> m_NetPlayerParticleSnapshotDataNewGhostIds;
    private NativeList<NetPlayerParticleSnapshotData> m_NetPlayerParticleSnapshotDataNewGhosts;
    private BufferFromEntity<NetSpawnedParticleSnapshotData> m_NetSpawnedParticleSnapshotDataFromEntity;
    private NativeList<int> m_NetSpawnedParticleSnapshotDataNewGhostIds;
    private NativeList<NetSpawnedParticleSnapshotData> m_NetSpawnedParticleSnapshotDataNewGhosts;
}
public struct EnableCellsGhostReceiveSystemComponent : IComponentData
{}
public class CellsGhostReceiveSystem : GhostReceiveSystem<CellsGhostDeserializerCollection>
{
    protected override void OnCreate()
    {
        base.OnCreate();
        RequireSingletonForUpdate<EnableCellsGhostReceiveSystemComponent>();
    }
}
