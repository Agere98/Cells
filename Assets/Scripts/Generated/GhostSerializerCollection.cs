using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Networking.Transport;
using Unity.NetCode;

public struct CellsGhostSerializerCollection : IGhostSerializerCollection
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
    public static int FindGhostType<T>()
        where T : struct, ISnapshotData<T>
    {
        if (typeof(T) == typeof(NetPlayerParticleSnapshotData))
            return 0;
        if (typeof(T) == typeof(NetSpawnedParticleSnapshotData))
            return 1;
        return -1;
    }
    public int FindSerializer(EntityArchetype arch)
    {
        if (m_NetPlayerParticleGhostSerializer.CanSerialize(arch))
            return 0;
        if (m_NetSpawnedParticleGhostSerializer.CanSerialize(arch))
            return 1;
        throw new ArgumentException("Invalid serializer type");
    }

    public void BeginSerialize(ComponentSystemBase system)
    {
        m_NetPlayerParticleGhostSerializer.BeginSerialize(system);
        m_NetSpawnedParticleGhostSerializer.BeginSerialize(system);
    }

    public int CalculateImportance(int serializer, ArchetypeChunk chunk)
    {
        switch (serializer)
        {
            case 0:
                return m_NetPlayerParticleGhostSerializer.CalculateImportance(chunk);
            case 1:
                return m_NetSpawnedParticleGhostSerializer.CalculateImportance(chunk);
        }

        throw new ArgumentException("Invalid serializer type");
    }

    public bool WantsPredictionDelta(int serializer)
    {
        switch (serializer)
        {
            case 0:
                return m_NetPlayerParticleGhostSerializer.WantsPredictionDelta;
            case 1:
                return m_NetSpawnedParticleGhostSerializer.WantsPredictionDelta;
        }

        throw new ArgumentException("Invalid serializer type");
    }

    public int GetSnapshotSize(int serializer)
    {
        switch (serializer)
        {
            case 0:
                return m_NetPlayerParticleGhostSerializer.SnapshotSize;
            case 1:
                return m_NetSpawnedParticleGhostSerializer.SnapshotSize;
        }

        throw new ArgumentException("Invalid serializer type");
    }

    public int Serialize(SerializeData data)
    {
        switch (data.ghostType)
        {
            case 0:
            {
                return GhostSendSystem<CellsGhostSerializerCollection>.InvokeSerialize<NetPlayerParticleGhostSerializer, NetPlayerParticleSnapshotData>(m_NetPlayerParticleGhostSerializer, data);
            }
            case 1:
            {
                return GhostSendSystem<CellsGhostSerializerCollection>.InvokeSerialize<NetSpawnedParticleGhostSerializer, NetSpawnedParticleSnapshotData>(m_NetSpawnedParticleGhostSerializer, data);
            }
            default:
                throw new ArgumentException("Invalid serializer type");
        }
    }
    private NetPlayerParticleGhostSerializer m_NetPlayerParticleGhostSerializer;
    private NetSpawnedParticleGhostSerializer m_NetSpawnedParticleGhostSerializer;
}

public struct EnableCellsGhostSendSystemComponent : IComponentData
{}
public class CellsGhostSendSystem : GhostSendSystem<CellsGhostSerializerCollection>
{
    protected override void OnCreate()
    {
        base.OnCreate();
        RequireSingletonForUpdate<EnableCellsGhostSendSystemComponent>();
    }
}
