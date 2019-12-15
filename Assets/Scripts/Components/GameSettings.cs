using Unity.Entities;

[GenerateAuthoringComponent]
public struct GameSettings : IComponentData {
    [System.Serializable]
    public struct LevelBounds {
        public float Bottom, Top, Left, Right;
    }
    public LevelBounds levelBounds {
        get {
            return new LevelBounds {
                Bottom = -BoardHeight / 2,
                Top = BoardHeight / 2,
                Left = -BoardWidth / 2,
                Right = BoardWidth / 2
            };
        }
    }

    public float BoardWidth;
    public float BoardHeight;

    public GameSettings(float boardWidth = 128f, float boardHeight = 128f) {
        BoardWidth = boardWidth;
        BoardHeight = boardHeight;
    }
}
