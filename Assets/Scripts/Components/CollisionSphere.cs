using Unity.Entities;

[GenerateAuthoringComponent]
public struct CollisionSphere : IComponentData {
    public float Radius;

    public CollisionSphere(float radius) {
        Radius = radius;
    }
}
