using System.Collections.Generic;
using UnityEngine;

namespace HexaStack
{
    public class HS_HexStack : MonoBehaviour
    {
        public List<HS_Hexagon> Hexagons { get; private set; }

        public void Add(HS_Hexagon hexagon)
        {
            Hexagons ??= new List<HS_Hexagon>();
            Hexagons.Add(hexagon);
            hexagon.SetParent(transform);
        }
        public void Place()
        {
            foreach (HS_Hexagon hexagon in Hexagons)
            {
                hexagon.DisableCollider();
            }
        }

        public void Remove(HS_Hexagon hexagon)
        {
            Hexagons.Remove(hexagon);

            if (Hexagons.Count <= 0)
            {
                DestroyImmediate(gameObject);
            }
        }

        public bool Contains(HS_Hexagon hexagon) => Hexagons.Contains(hexagon);
        public Color GetTopHexagonColor() => Hexagons[^1].Color;
    }
}
