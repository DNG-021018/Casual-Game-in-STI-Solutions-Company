using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HexaStack
{
    public class HS_MergeManager : MonoBehaviour
    {
        List<HS_GridCell> updatedCells = new();
        HS_GridCell[] allGridCells;

        private Queue<HS_GridCell> mergeQueue = new();
        private bool isMerging = false;


        void Awake()
        {
            HS_StackController.OnStackPlaced += StackPlacedCallback;
        }

        void Start()
        {
            allGridCells = HS_GridGenerator.Instance.GetAllGridCell();
        }

        void OnDestroy()
        {
            HS_StackController.OnStackPlaced -= StackPlacedCallback;
        }

        public void ResetMerge()
        {
            StopAllCoroutines();
            updatedCells.Clear();
            mergeQueue.Clear();
            isMerging = false;
        }


        private void StackPlacedCallback(HS_GridCell gridCell)
        {
            mergeQueue.Enqueue(gridCell);

            if (!isMerging)
            {
                StartCoroutine(ProcessMergeQueue());
            }
        }

        IEnumerator ProcessMergeQueue()
        {
            while (mergeQueue.Count > 0)
            {
                isMerging = true;
                HS_GridCell gridCell = mergeQueue.Dequeue();
                yield return StackPlacedCoroutine(gridCell);
            }

            isMerging = false;
        }

        IEnumerator StackPlacedCoroutine(HS_GridCell gridCell)
        {
            updatedCells.Add(gridCell);

            while (updatedCells.Count > 0)
            {
                yield return CheckForMerge(updatedCells[0]);
            }

            if (CheckGameOver())
            {
                HS_GameManager.Instance?.SetState(GameState.Lose);
                yield break;
            }
        }

        private bool CheckGameOver()
        {
            if (!HasEmptyCell())
            {
                if (!HasPossibleMerge())
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasEmptyCell()
        {
            if (allGridCells == null)
            {
                Debug.LogError("AllGridCells is null");
                return false;
            }

            foreach (HS_GridCell cell in allGridCells)
            {
                if (!cell.IsOccupied)
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasPossibleMerge()
        {
            if (allGridCells == null)
            {
                Debug.LogError("AllGridCells is null");
                return false;
            }

            foreach (HS_GridCell gridCell in allGridCells)
            {
                if (!gridCell.IsOccupied)
                    continue;

                List<HS_GridCell> neighborGridCells = GetNeighBorGridCells(gridCell);

                if (neighborGridCells.Count <= 0)
                    continue;

                Color gridCellTopHexagonColor = gridCell.Stack.GetTopHexagonColor();
                List<HS_GridCell> similarNeighborGridCells = GetSimilarNeighBorGridCells(gridCellTopHexagonColor, neighborGridCells.ToArray());

                if (similarNeighborGridCells.Count > 0)
                {
                    return true;
                }
            }

            return false;
        }

        IEnumerator CheckForMerge(HS_GridCell gridCell)
        {
            updatedCells.Remove(gridCell);

            if (!gridCell.IsOccupied)
            {
                yield break;
            }

            List<HS_GridCell> neighborGridCells = GetNeighBorGridCells(gridCell);

            if (neighborGridCells.Count <= 0)
            {
                yield break;
            }

            Color gridCellTopHexagonColor = gridCell.Stack.GetTopHexagonColor();

            List<HS_GridCell> similarNeighborGridCells = GetSimilarNeighBorGridCells(gridCellTopHexagonColor, neighborGridCells.ToArray());

            if (similarNeighborGridCells.Count <= 0)
            {
                yield break;
            }

            updatedCells.AddRange(similarNeighborGridCells);

            List<HS_Hexagon> hexagonsToAdd = GetHexagonsToAdd(gridCellTopHexagonColor, similarNeighborGridCells.ToArray());

            RemoveHexagonFromStack(hexagonsToAdd, similarNeighborGridCells.ToArray());

            yield return MoveHexagon(gridCell, hexagonsToAdd);
            yield return CheckForCompleteStack(gridCell, gridCellTopHexagonColor);
        }

        private IEnumerator CheckForCompleteStack(HS_GridCell gridCell, Color topColor)
        {
            if (gridCell.Stack.Hexagons.Count < 10)
            {
                yield break;
            }

            List<HS_Hexagon> similarHexagons = new();

            for (int i = gridCell.Stack.Hexagons.Count - 1; i >= 0; i--)
            {
                HS_Hexagon hexagon = gridCell.Stack.Hexagons[i];

                if (hexagon.Color != topColor)
                {
                    break;
                }

                similarHexagons.Add(hexagon);
            }

            int similarHexagonCount = similarHexagons.Count;

            if (similarHexagons.Count < 10)
            {
                yield break;
            }

            float delay = 0;

            while (similarHexagons.Count > 0)
            {
                similarHexagons[0].SetParent(null);
                similarHexagons[0].Vanish(delay);

                delay += 0.01f;

                gridCell.Stack.Remove(similarHexagons[0]);
                similarHexagons.RemoveAt(0);
            }

            updatedCells.Add(gridCell);
            HS_GameManager.Instance.AddScore(10);
            yield return new WaitForSeconds(0.2f + (similarHexagonCount + 1) * 0.01f);
        }

        private IEnumerator MoveHexagon(HS_GridCell gridCell, List<HS_Hexagon> hexagonsToAdd)
        {
            float initialY = gridCell.Stack.Hexagons.Count * 0.2f;

            for (int i = 0; i < hexagonsToAdd.Count; i++)
            {
                HS_Hexagon hexagon = hexagonsToAdd[i];

                float targetY = initialY + i * 0.2f;
                Vector3 targetLocalPosition = Vector3.up * targetY;

                gridCell.Stack.Add(hexagon);
                hexagon.MoveToLocal(targetLocalPosition);
            }

            yield return new WaitForSeconds(0.5f + (hexagonsToAdd.Count + 1) * 0.1f);
        }

        private List<HS_Hexagon> GetHexagonsToAdd(Color gridCellTopHexagonColor, HS_GridCell[] similarNeighborGridCells)
        {
            List<HS_Hexagon> hexagonsToAdd = new();

            foreach (HS_GridCell neighborCell in similarNeighborGridCells)
            {
                HS_HexStack neighborCellHexStack = neighborCell.Stack;

                for (int i = neighborCellHexStack.Hexagons.Count - 1; i >= 0; i--)
                {
                    HS_Hexagon hexagon = neighborCellHexStack.Hexagons[i];

                    if (hexagon.Color != gridCellTopHexagonColor)
                    {
                        break;
                    }

                    hexagonsToAdd.Add(hexagon);
                    hexagon.SetParent(null);
                }
            }

            return hexagonsToAdd;
        }

        private List<HS_GridCell> GetSimilarNeighBorGridCells(Color gridCellTopHexagonColor, HS_GridCell[] neighborGridCells)
        {
            List<HS_GridCell> similarNeighborGridCells = new();

            foreach (HS_GridCell neighborGridCell in neighborGridCells)
            {
                Color neighborGridCellTopHexagonColor = neighborGridCell.Stack.GetTopHexagonColor();

                if (gridCellTopHexagonColor == neighborGridCellTopHexagonColor)
                {
                    similarNeighborGridCells.Add(neighborGridCell);
                }
            }

            return similarNeighborGridCells;
        }

        private List<HS_GridCell> GetNeighBorGridCells(HS_GridCell gridCell)
        {
            LayerMask gridCellMask = 1 << gridCell.gameObject.layer;
            List<HS_GridCell> neighborGridCells = new();
            Collider[] neighborGridCellColliders = Physics.OverlapSphere(gridCell.transform.position, 2, gridCellMask);

            foreach (Collider gridCellCollider in neighborGridCellColliders)
            {
                HS_GridCell neighborGridCell = gridCellCollider.GetComponent<HS_GridCell>();

                if (!neighborGridCell.IsOccupied)
                {
                    continue;
                }

                if (neighborGridCell == gridCell)
                {
                    continue;
                }

                neighborGridCells.Add(neighborGridCell);
            }

            return neighborGridCells;
        }

        private void RemoveHexagonFromStack(List<HS_Hexagon> hexagonsToAdd, HS_GridCell[] similarNeighborGridCells)
        {
            foreach (HS_GridCell neighborCell in similarNeighborGridCells)
            {
                HS_HexStack stack = neighborCell.Stack;

                foreach (HS_Hexagon hexagon in hexagonsToAdd)
                {
                    if (stack.Contains(hexagon))
                    {
                        stack.Remove(hexagon);
                    }
                }
            }
        }
    }
}
