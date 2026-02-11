using UnityEngine;

namespace HexaStack
{
    public class HS_GridCell : MonoBehaviour
    {
        public HS_HexStack Stack { get; private set; }
        public Color hightLightColor;

        private Color originColor;
        private MeshRenderer r;

        void Start()
        {
            r = GetComponentInChildren<MeshRenderer>();
            originColor = r.material.color;
        }

        public bool IsOccupied
        {
            get => Stack != null;
            private set { }
        }

        public void AssignStack(HS_HexStack stack)
        {
            this.Stack = stack;
        }

        public void HightLight(bool value)
        {
            if (value)
            {
                r.material.color = hightLightColor;
            }
            else
            {
                r.material.color = originColor;
            }
        }
    }
}
