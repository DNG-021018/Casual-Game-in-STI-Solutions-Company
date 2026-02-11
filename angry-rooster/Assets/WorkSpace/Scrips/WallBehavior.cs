using UnityEngine;

public class WallBehavior : MonoBehaviour
{
    [SerializeField] private GameObject uiPanel;
     private Character playerController;
    [SerializeField] private float triggerDuration = 1f;

    public bool isPlayerInside = false;
    public float stayTime = 0f;


    private void Start() {
        playerController = GetComponent<Character>();
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("OnTriggerEnter called with: " + other.name);
        if (other.CompareTag("Wall"))
        {
            Debug.Log("Player entered the wall trigger.");
            isPlayerInside = true;
            stayTime = 0f;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Wall"))
        {
            isPlayerInside = false;
            stayTime = 0f;
            uiPanel.SetActive(false);
        }
    }

    private void FixedUpdate()
    {
        if (isPlayerInside && playerController.isMoving)
        {
            stayTime += Time.fixedDeltaTime;

            if (!uiPanel.activeSelf && stayTime >= triggerDuration) 
            {
                Debug.Log("Player is inside the wall trigger and has stayed long enough.");
                uiPanel.SetActive(true);
            }

            if (uiPanel.activeSelf)
            {
                stayTime = 0f;
            }
        }
    }
}
