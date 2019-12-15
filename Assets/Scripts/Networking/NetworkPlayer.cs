using Unity.Entities;
using Unity.NetCode;

[GenerateAuthoringComponent]
public struct NetworkPlayer : IComponentData {
    [GhostDefaultField]
    public int PlayerId;
}
