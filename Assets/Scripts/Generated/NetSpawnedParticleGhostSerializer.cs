using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Collections;
using Unity.NetCode;
using Unity.Transforms;

public struct NetSpawnedParticleGhostSerializer : IGhostSerializer<NetSpawnedParticleSnapshotData>
{
    private ComponentType componentTypeCollisionSphere;
    private ComponentType componentTypeCollisionStatic;
    private ComponentType componentTypeParticleSpawner;
    private ComponentType componentTypeScore;
    private ComponentType componentTypeLocalToWorld;
    private ComponentType componentTypeRotation;
    private ComponentType componentTypeTranslation;
    // FIXME: These disable safety since all serializers have an instance of the same type - causing aliasing. Should be fixed in a cleaner way
    [NativeDisableContainerSafetyRestriction][ReadOnly] private ArchetypeChunkComponentType<Rotation> ghostRotationType;
    [NativeDisableContainerSafetyRestriction][ReadOnly] private ArchetypeChunkComponentType<Translation> ghostTranslationType;


    public int CalculateImportance(ArchetypeChunk chunk)
    {
        return 1;
    }

    public bool WantsPredictionDelta => true;

    public int SnapshotSize => UnsafeUtility.SizeOf<NetSpawnedParticleSnapshotData>();
    public void BeginSerialize(ComponentSystemBase system)
    {
        componentTypeCollisionSphere = ComponentType.ReadWrite<CollisionSphere>();
        componentTypeCollisionStatic = ComponentType.ReadWrite<CollisionStatic>();
        componentTypeParticleSpawner = ComponentType.ReadWrite<ParticleSpawner>();
        componentTypeScore = ComponentType.ReadWrite<Score>();
        componentTypeLocalToWorld = ComponentType.ReadWrite<LocalToWorld>();
        componentTypeRotation = ComponentType.ReadWrite<Rotation>();
        componentTypeTranslation = ComponentType.ReadWrite<Translation>();
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
            if (components[i] == componentTypeCollisionStatic)
                ++matches;
            if (components[i] == componentTypeParticleSpawner)
                ++matches;
            if (components[i] == componentTypeScore)
                ++matches;
            if (components[i] == componentTypeLocalToWorld)
                ++matches;
            if (components[i] == componentTypeRotation)
                ++matches;
            if (components[i] == componentTypeTranslation)
                ++matches;
        }
        return (matches == 7);
    }

    public void CopyToSnapshot(ArchetypeChunk chunk, int ent, uint tick, ref NetSpawnedParticleSnapshotData snapshot, GhostSerializerState serializerState)
    {
        snapshot.tick = tick;
        var chunkDataRotation = chunk.GetNativeArray(ghostRotationType);
        var chunkDataTranslation = chunk.GetNativeArray(ghostTranslationType);
        snapshot.SetRotationValue(chunkDataRotation[ent].Value, serializerState);
        snapshot.SetTranslationValue(chunkDataTranslation[ent].Value, serializerState);
    }
}
