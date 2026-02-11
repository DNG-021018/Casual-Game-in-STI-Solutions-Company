using UnityEngine;
using System;

public enum PlayerStateAnimator
{
    Idle,
    Moving,
    Lost,
    Win
}

public class PlayerAnimator : MonoBehaviour
{
    public event Action<PlayerAnimator, PlayerStateAnimator> OnStateChanged;

    [Header("Components")]
    [SerializeField] private Animator animator;
    
        // Nếu muốn dùng const, bạn chỉ có thể dùng cho các chuỗi tên tham số:
    private const string IsMoveParam = "isMove";
    private const string IsLostParam = "isLost";
    private const string IsWinParam = "isWin";
    public PlayerStateAnimator CurrentState { get; private set; }

    public void SetState(PlayerStateAnimator newState)
    {
        if (CurrentState == newState || IsFinalState(CurrentState))
            return;

        CurrentState = newState;
        UpdateAnimator();
        OnStateChanged?.Invoke(this, CurrentState);
    }

    private bool IsFinalState(PlayerStateAnimator state) =>
        state == PlayerStateAnimator.Lost || state == PlayerStateAnimator.Win;

    private void UpdateAnimator()
    {
        animator.SetBool(IsMoveParam, false);

        switch (CurrentState)
        {
            case PlayerStateAnimator.Moving:
                animator.SetBool(IsMoveParam, true);
                break;
            case PlayerStateAnimator.Lost:
                animator.SetTrigger(IsLostParam);
                break;
            case PlayerStateAnimator.Win:
                animator.SetTrigger(IsWinParam);
                break;
        }
    }

    public void Die() => SetState(PlayerStateAnimator.Lost);
    public void Win() => SetState(PlayerStateAnimator.Win);
    public void Idle() => SetState(PlayerStateAnimator.Idle);
    public void Moving() => SetState(PlayerStateAnimator.Moving);
}
