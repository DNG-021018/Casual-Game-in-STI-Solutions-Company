using UnityEngine;

namespace NightEscape
{
    public class NE_DoorGoalTrigger : MonoBehaviour
    {
        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(NE_SafetyKey.KEY_TAG_PLAYER)) return;
            other.transform.TryGetComponent(out NE_PlayerController player);
            // NE_CameraManager.Instance.SetTarget(player.targetCamera.transform, NE_CameraManager.Instance.GetWinGameCamera());
            NE_CameraManager.Instance.BlendToWinCamera(0.1f);
        }
    }
}
