using UnityEngine;

public class Settings : MonoBehaviour {

    [SerializeField]
    private Transform board;
    [SerializeField]
    private float boardWidth;
    [SerializeField]
    private float boardHeight;

    [ContextMenu("Apply settings")]
    void Apply() {
        if (board) {
            board.localScale = new Vector3(boardWidth, boardHeight, 128f);
            var renderer = board.GetComponent<Renderer>();
            if (renderer) {
                renderer.sharedMaterial.mainTextureScale = new Vector2(boardWidth / 2, boardHeight / 2);
            }
        }
    }
}
