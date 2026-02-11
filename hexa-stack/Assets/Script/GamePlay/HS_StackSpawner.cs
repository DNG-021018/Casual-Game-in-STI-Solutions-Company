using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;

namespace HexaStack
{
    public class HS_StackSpawner : MonoBehaviour
    {
        [Header("Elements")]
        [SerializeField] private Transform stackPositionParent;
        [SerializeField] private HS_Hexagon hexagonPrefab;
        [SerializeField] private HS_HexStack hexagonStackPrefab;

        [Header("Settings")]
        [MinMaxSlider(3, 7)]
        [SerializeField] private Vector2Int minMaxHexCount;
        [SerializeField] private Material[] materials;

        private Color[] colors;
        private int stackConter;

        void Awake()
        {
            Application.targetFrameRate = 60;

            HS_StackController.OnStackPlaced += StackPlacedCallBack;

            colors = new Color[materials.Length];
            for (int i = 0; i < materials.Length; i++)
            {
                colors[i] = materials[i].color;
            }
        }

        void OnDestroy()
        {
            HS_StackController.OnStackPlaced -= StackPlacedCallBack;
        }

        private void StackPlacedCallBack(HS_GridCell cell)
        {
            stackConter++;

            if (stackConter >= 3)
            {
                stackConter = 0;
                GeneratedStacks();
            }
        }

        public void ClearAllStacks()
        {
            HS_HexStack[] allStacks = FindObjectsByType<HS_HexStack>(FindObjectsSortMode.None);
            foreach (var stack in allStacks)
            {
                Destroy(stack.gameObject);
            }

            HS_GridCell[] cells = HS_GridGenerator.Instance.GetAllGridCell();
            if (cells != null)
            {
                foreach (var cell in cells)
                {
                    if (cell.IsOccupied)
                    {
                        cell.AssignStack(null);
                    }
                }
            }

            stackConter = 0;
        }

        public void SpawnInitialStacks()
        {
            GeneratedStacks();
        }

        private void GeneratedStacks()
        {
            for (int i = 0; i < stackPositionParent.childCount; i++)
            {
                GeneratedStack(stackPositionParent.GetChild(i));
            }
        }

        private void GeneratedStack(Transform parent)
        {
            HS_HexStack hexStack =
                Instantiate(hexagonStackPrefab, parent.position, Quaternion.identity, parent);
            hexStack.name = $"Stack {parent.GetSiblingIndex()}";

            int amount = Random.Range(minMaxHexCount.x, minMaxHexCount.y);
            int firstColorHexagonCount = Random.Range(0, amount);

            Color[] colorArray = GetRandomColors();

            for (int i = 0; i < amount; i++)
            {
                Vector3 hexagonLocalPos = Vector3.up * i * 0.2f;
                Vector3 spawnPosition = hexStack.transform.TransformPoint(hexagonLocalPos);

                HS_Hexagon hexagonInstance =
                    Instantiate(hexagonPrefab, spawnPosition, Quaternion.Euler(0, 90, 0), hexStack.transform);
                hexagonInstance.Color = i < firstColorHexagonCount ? colorArray[0] : colorArray[1];
                hexagonInstance.Configure(hexStack);
                hexStack.Add(hexagonInstance);
            }
        }

        private Color[] GetRandomColors()
        {
            List<Color> colorList = new List<Color>();
            colorList.AddRange(colors);

            if (colorList.Count <= 0)
            {
                Debug.LogError("No Color was found");
                return null;
            }

            Color firstColor = colorList.OrderBy(x => Random.value).First();
            colorList.Remove(firstColor);

            if (colorList.Count <= 0)
            {
                Debug.LogError("Only one color was found");
                return null;
            }

            Color secondColor = colorList.OrderBy(x => Random.value).First();
            return new Color[] { firstColor, secondColor };
        }
    }
}
