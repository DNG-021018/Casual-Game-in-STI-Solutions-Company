using UnityEngine;


public class SwipeInput
{
    public float swipeThreshold = 50f;
    private Vector2 startTouchPos;
    private Vector2 endTouchPos;

    public void HandleSwipeInput()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetMouseButtonDown(0))
        {
            startTouchPos = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            endTouchPos = Input.mousePosition;
            TryDetectSwipe();
        }
#else
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    startTouchPos = touch.position;
                    break;
                case TouchPhase.Ended:
                    endTouchPos = touch.position;
                    TryDetectSwipe();
                    break;
            }
        }
#endif
    }
    private void TryDetectSwipe()
    {
        if (!PlayerManager.Instance.AreAllPlayersIdle()) return;

        if (UIManager.Instance != null && UIManager.Instance.IsEditModeActive()) return;

        Vector2 swipeDelta = endTouchPos - startTouchPos;
        if (swipeDelta.magnitude < swipeThreshold) return;

        Vector2 inputDirection = GetSwipeDirection(swipeDelta);
        PlayerManager.Instance.MoveAllPlayers(inputDirection);
        SoundManager.Instance?.PlaySFX(PlayerManager.Instance.swipeClip);
    }

    private Vector2 GetSwipeDirection(Vector2 delta)
    {
        float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360;

        return angle switch
        {
            >= 45 and < 135 => Vector2.up,
            >= 135 and < 225 => Vector2.left,
            >= 225 and < 315 => Vector2.down,
            _ => Vector2.right
        };
    }
}