using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Collections;
using Unity.NetCode;
using Unity.Transforms;

public struct NetPlayerParticleGhostSerializer : IGhostSerializer<NetPlayerParticleSnapshotData>
{
    private ComponentType componentTypeCollisionSphere;
    private ComponentType componentTypeNetworkPlayer;
    private ComponentType componentTypePlayer;
    private ComponentType componentTypeScore;
    private ComponentType componentTypeLocalToWorld;
    private ComponentType componentTypeNonUniformScale;
    private ComponentType componentTypeRotation;
    private ComponentType componentTypeTranslation;
    // FIXME: These disable safety since all serializers have an instance of the same type - causing aliasing. Should be fixed in a cleaner way
    [NativeDisableContainerSafetyRestriction][ReadOnly] private ArchetypeChunkComponentType<NetworkPlayer> ghostNetworkPlayerType;
    [NativeDisableContainerSafetyRestriction][ReadOnly] private ArchetypeChunkComponentType<Score> ghostScoreType;
    [NativeDisableContainerSafetyRestriction][ReadOnly] private ArchetypeChunkComponentType<Rotation> ghostRotationType;
    [NativeDisableContainerSafetyRestriction][ReadOnly] private ArchetypeChunkComponentType<Translation> ghostTranslationType;


    public int CalculateImportance(ArchetypeChunk chunk)
    {
        return 1;
    }

    public bool WantsPredictionDelta => true;

    public int SnapshotSize => UnsafeUtility.SizeOf<NetPlayerParticleSnapshotData>();
    public void BeginSerialize(ComponentSystemBase system)
    {
        componentTypeCollisionSphere = ComponentType.ReadWrite<CollisionSphere>();
        componentTypeNetworkPlayer = ComponentType.ReadWrite<NetworkPlayer>();
        componentTypePlayer = ComponentType.ReadWrite<Player>();
        componentTypeScore = ComponentType.ReadWrite<Score>();
        componentTypeLocalToWorld = ComponentType.ReadWrite<LocalToWorld>();
        componentTypeNonUniformScale = ComponentType.ReadWrite<NonUniformScale>();
        componentTypeRotation = ComponentType.ReadWrite<Rotation>();
        componentTypeTranslation = ComponentType.ReadWrite<Translation>();
        ghostNetworkPlayerType = system.GetArchetypeChunkComponentType<NetworkPlayer>(true);
        ghostScoreType = system.GetArchetypeChunkComponentType<Score>(true);
        ghostRotationType = system.GetArchetypeChunkComponentType<Rotation>(true);
        ghostTranslationType = system.GetArchetypeChunkComponentType<Translation>(true);
    }

    public bool CanSerialize(EntityArchetype arch)
    {
        var components = arch.GetComponentTypes();
        int matches = 0;
        for (int i = 0; i < components.Length; ++i)
        {
            if (components[i] == componentTypeCollisionSphere)
                ++matches;
            if (components[i] == componentTypeNetworkPlayer)
                ++matches;
            if (components[i] == componentTypePlayer)
                ++matches;
            if (components[i] == componentTypeScore)
                ++matches;
            if (components[i] == componentTypeLocalToWorld)
                ++matches;
            if (components[i] == componentTypeNonUniformScale)
                ++matches;
            if (components[i] == componentTypeRotation)
                ++matches;
            if (components[i] == componentTypeTranslation)
                ++matches;
        }
        return (matches == 8);
    }

    public void CopyToSnapshot(ArchetypeChunk chunk, int ent, uint tick, ref NetPlayerParticleSnapshotData snapshot, GhostSerializerState serializerState)
    {
        snapshot.tick = tick;
        var chunkDataNetworkPlayer = chunk.GetNativeArray(ghostNetworkPlayerType);
        var chunkDataScore = chunk.GetNativeArray(ghostScoreType);
        var chunkDataRotation = chunk.GetNativeArray(ghostRotationType);
        var chunkDataTranslation = chunk.GetNativeArray(ghostTranslationType);
        snapshot.SetNetworkPlayerPlayerId(chunkDataNetworkPlayer[ent].PlayerId, serializerState);
        snapshot.SetScoreValue(chunkDataScore[ent].Value, serializerState);
        snapshot.SetRotationValue(chunkDataRotation[ent].Value, serializerState);
        snapshot.SetTranslationValue(chunkDataTranslation[ent].Value, serializerState);
    }
}
