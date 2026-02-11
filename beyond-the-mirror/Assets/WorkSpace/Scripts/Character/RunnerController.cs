using UnityEngine;

public class RunnerController : MonoBehaviour
{
    [Header("Characters")]
    public CharacterController mainCharacter;
    public CharacterController mirroredCharacter;

    [HideInInspector] public int currentLane = 0;

    public void MoveLeft()
    {
        currentLane = Mathf.Clamp(currentLane - 1, -1, 1);
        UpdateCharacters();
    }

    public void MoveRight()
    {
        currentLane = Mathf.Clamp(currentLane + 1, -1, 1);
        UpdateCharacters();
    }

    public void Jump()
    {
        mainCharacter.Jump();
        mirroredCharacter.Jump();
    }

    public void Crouch()
    {
        mainCharacter.CrouchSlide();
        mirroredCharacter.CrouchSlide();
    }

    private void UpdateCharacters()
    {
        int mirrorLane = -currentLane;

        if (mainCharacter.CanSlideTo(currentLane) && mirroredCharacter.CanSlideTo(mirrorLane))
        {
            mainCharacter.SlideToLane(currentLane);
            mirroredCharacter.SlideToLane(mirrorLane);
        }
    }
}
