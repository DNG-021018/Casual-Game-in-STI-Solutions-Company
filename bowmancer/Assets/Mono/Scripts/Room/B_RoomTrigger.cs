using UnityEngine;

namespace Bowmancer
{
    [RequireComponent(typeof(Collider))]
    public class B_RoomTrigger : MonoBehaviour
    {
        [Header("Previous Room Door")]
        [SerializeField] private B_Door previousRoomDoor;

        [Header("Next Room")]
        [SerializeField] private B_RoomManager nextRoom;

        [SerializeField] private bool triggerOnce = true;

        private bool triggered;

        void Awake()
        {
            var col = GetComponent<Collider>();
            col.isTrigger = true;
        }

        void OnTriggerEnter(Collider other)
        {
            if (triggered && triggerOnce) return;
            if (!other.CompareTag(B_SafetyKey.TAG_PLAYER)) return;

            if (previousRoomDoor != null && !previousRoomDoor.IsOpen())
                return;

            triggered = true;

            if (previousRoomDoor != null)
            {
                previousRoomDoor.CloseDoor();
            }

            if (nextRoom != null)
            {
                nextRoom.ActivateRoom();
            }

            if (triggerOnce)
            {
                GetComponent<Collider>().enabled = false;
            }
        }
    }
}
