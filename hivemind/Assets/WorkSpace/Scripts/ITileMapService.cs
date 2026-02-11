using UnityEngine;

public interface ITileMapService
{
    Vector3Int WorldToCell(Vector3 worldPosition);
    Vector3 GetCellCenterWorld(Vector3Int cellPosition);
    bool HasTeleportAt(Vector3Int cellPosition);
    TileMapPointTeleport GetTeleportAt(Vector3Int cellPosition);
}
