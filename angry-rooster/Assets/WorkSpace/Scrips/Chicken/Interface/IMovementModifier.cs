using UnityEngine;

public interface IMovementModifier
{
    void ApplySpeedModifier(float multiplier);
    void RemoveSpeedModifier();
}
