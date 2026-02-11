using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonJumpAction : MonoBehaviour, IPointerDownHandler
{
    private static ButtonJumpAction _instance;
    public static ButtonJumpAction Instance => _instance;

    public event Action OnJumpButtonClicked;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log(">>> ON POINTER DOWN <<<");

        OnJumpButtonClicked?.Invoke();
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
}