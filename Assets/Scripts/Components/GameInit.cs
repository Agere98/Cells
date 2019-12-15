using Unity.Entities;

[GenerateAuthoringComponent]
public struct GameInit : IComponentData {
    public Entity ParticlePrefab;
    public int NumberOfParticles;
}
