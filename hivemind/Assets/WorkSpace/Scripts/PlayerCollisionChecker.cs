using UnityEngine;

public class PlayerCollisionChecker
{
    private Transform playerTransform;
    private float currentPosY;

    private readonly float raycastDistance = 0.2f;
    private readonly float raycastOffset = 0.1f;

    private readonly Vector3 boxHalfExtents = new Vector3(0.6f, 0.5f, 0.4f);

    private LayerMask groundMask;
    private LayerMask iceGroundMask;
    private LayerMask wallMask;
    private LayerMask gateMask;
    private LayerMask teleportMask;

    public PlayerCollisionChecker(Transform playerTransform, float currentPosY)
    {
        this.playerTransform = playerTransform;
        this.currentPosY = currentPosY;

        groundMask = LayerMask.GetMask("Ground");
        iceGroundMask = LayerMask.GetMask("IceGround");
        wallMask = LayerMask.GetMask("Wall");
        gateMask = LayerMask.GetMask("Gate");
        teleportMask = LayerMask.GetMask("Teleport");
    }

    public bool IsGroundBelow() => CheckRaycastBelow(groundMask);
    public bool IsIceGroundBelow() => CheckRaycastBelow(iceGroundMask);

    public bool IsWallAheadBox(Vector2 input) => CheckBoxAhead(input, wallMask, Color.red);
    public bool IsGateAheadBox(Vector2 input) => CheckBoxAhead(input, gateMask, Color.green);
    public bool IsTeleportAheadBox(Vector2 input) => CheckBoxAhead(input, teleportMask, Color.blue);

    public void UpdateCurrentPosY(float newY) => currentPosY = newY;

    // === Private Common Methods ===

    private bool CheckRaycastBelow(LayerMask mask)
    {
        Vector3 origin = playerTransform.position + Vector3.up * raycastOffset;
        return Physics.Raycast(origin, Vector3.down, raycastDistance, mask);
    }

    private bool CheckBoxAhead(Vector2 input, LayerMask mask, Color debugColor)
    {
        Vector3 direction = new Vector3(input.x, 0f, input.y).normalized;
        Vector3 checkPos = playerTransform.position + direction;
        checkPos.y = currentPosY + boxHalfExtents.y;

        Collider[] colliders = Physics.OverlapBox(checkPos, boxHalfExtents, Quaternion.identity, mask);
        Debug.DrawLine(checkPos + Vector3.up * 1f, checkPos + Vector3.down * 0.5f, debugColor, 0.3f);

        return colliders.Length > 0;
    }
}
