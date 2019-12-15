using Unity.Entities;

[GenerateAuthoringComponent]
public struct Player : IComponentData {
    public float Speed;

    public Player(float speed) {
        Speed = speed;
    }
}
