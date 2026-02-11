using NaughtyAttributes;
using UnityEngine;

namespace HexaStack
{
    public class HS_GridGenerator : MonoBehaviour
    {
        public static HS_GridGenerator Instance { get; private set; }

        [Header("Elements")]
        [SerializeField] private Grid grid;
        [SerializeField] private GameObject hexagonPrefab;

        [Header("Settings")]
        [OnValueChanged("GenerateGrid")]
        [SerializeField] private int gridSize;
        HS_GridCell[] gridCellsArray;

        private void GenerateGrid()
        {
            transform.Clear();

            for (int x = -gridSize; x <= gridSize; x++)
            {
                for (int y = -gridSize; y <= gridSize; y++)
                {
                    Vector3 spawnPos = grid.CellToWorld(new Vector3Int(x, y, 0));

                    if (spawnPos.magnitude > grid.CellToWorld(new Vector3Int(1, 0, 0)).magnitude * gridSize)
                    {
                        continue;
                    }

                    Instantiate(hexagonPrefab, spawnPos, Quaternion.identity, transform);
                }
            }
        }

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }

            gridCellsArray = GetComponentsInChildren<HS_GridCell>();
        }

        public HS_GridCell[] GetAllGridCell()
        {
            if (gridCellsArray.Length <= 0)
            {
                return null;
            }

            return gridCellsArray;
        }
    }
}
