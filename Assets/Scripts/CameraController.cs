using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour {

    [SerializeField]
    private float minSize = 7f;
    [SerializeField]
    private float speed = 0.5f;
    private Camera cam;
    private Transform tr;

    private void Awake() {
        cam = GetComponent<Camera>();
        tr = transform;
    }

    private void Update() {
        cam.orthographicSize = Mathf.Clamp(
            -tr.position.z - 100f,
            minSize,
            cam.orthographicSize + Time.deltaTime * speed);
    }
}
