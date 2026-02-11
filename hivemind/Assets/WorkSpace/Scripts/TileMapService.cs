using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileMapService : MonoBehaviour, ITileMapService
{
    [SerializeField] private TileMapController tileMapController;

    public Vector3Int WorldToCell(Vector3 worldPosition)
    {
        return tileMapController.tilemapGround.WorldToCell(worldPosition);
    }

    public Vector3 GetCellCenterWorld(Vector3Int cellPosition)
    {
        return tileMapController.tilemapGround.GetCellCenterWorld(cellPosition);
    }

    public bool HasTeleportAt(Vector3Int cellPosition)
    {
        return tileMapController.tileMapPointTeleport.ContainsKey(cellPosition);
    }

    public TileMapPointTeleport GetTeleportAt(Vector3Int cellPosition)
    {
        if (tileMapController.tileMapPointTeleport.TryGetValue(cellPosition, out var teleport))
            return teleport;

        return null;
    }
}
