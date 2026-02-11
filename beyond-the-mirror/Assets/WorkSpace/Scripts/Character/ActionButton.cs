using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ActionButton : MonoBehaviour
{
    public UnityEvent onClick;

    void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() => onClick.Invoke());
    }
}
