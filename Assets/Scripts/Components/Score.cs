using Unity.Entities;

[GenerateAuthoringComponent]
public struct Score : IComponentData {
    public int Value;

    public Score(int points) {
        Value = points;
    }
}
