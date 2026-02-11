using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InputController : MonoBehaviour, IPointerDownHandler
{
    private static InputController _instance;
    public static InputController Instance => _instance;

    [Header("References")]
    [SerializeField] private Character _character;
    [SerializeField] private Button _jumpButton;
    [SerializeField] private Button _sprintButton;
    [SerializeField] private Image _sprintCoolDownImage;
    [SerializeField] public UltimateJoystick joystick;

    public event Action OnJumpButtonPressedEvent;
    public event Action OnSprintButtonPressedEvent;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Gán handler jump bằng cách thêm component nhận sự kiện
        if (_jumpButton != null)
        {
            JumpButtonHandler trigger = _jumpButton.gameObject.AddComponent<JumpButtonHandler>();
            trigger.OnPointerDownEvent += () =>
            {
                Debug.Log(">> JumpButton Down Triggered");
                OnJumpButtonPressedEvent?.Invoke();
            };
        }

        SceneManager.sceneUnloaded += OnSceneUnLoaded;
    }

    private void OnSprintPressed()
    {
        OnSprintButtonPressedEvent?.Invoke();
    }

    private void OnSceneUnLoaded(Scene arg0)
    {
        OnJumpButtonPressedEvent = null;
    }

    private void OnDestroy()
    {
        if (_instance != null)
        {
            _instance = null;
        }
        joystick = null;
    }

    // Không dùng hàm OnPointerDown này của Mono nếu bạn không gắn script lên Button
    public void OnPointerDown(PointerEventData eventData) { }
}
