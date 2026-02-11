using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance;

    public Transform target;

    private void Awake()
    {
        Instance = this;
    }
}
