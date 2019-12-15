using Unity.Entities;

[GenerateAuthoringComponent]
public struct ParticleSpawner : IComponentData {

    public Entity Prefab;
    public double RespawnTime;

    public ParticleSpawner(Entity prefab, double respawnTime) {
        Prefab = prefab;
        RespawnTime = respawnTime;
    }
}
