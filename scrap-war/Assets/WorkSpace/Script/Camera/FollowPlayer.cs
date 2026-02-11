using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public GameObject player;

    void LateUpdate()
    {
        transform.position = new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z);
    }
}
