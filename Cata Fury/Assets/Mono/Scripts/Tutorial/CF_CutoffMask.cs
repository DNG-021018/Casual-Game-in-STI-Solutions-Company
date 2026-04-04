using UnityEngine;
using UnityEngine.UI;

namespace CataFury
{
    public class CF_CutoffMask : Image
    {
        public override Material materialForRendering
        {
            get
            {
                Material material = new(base.materialForRendering);
                material.SetInt("_StencilComp", (int)UnityEngine.Rendering.CompareFunction.NotEqual);
                return material;
            }
        }
    }
}
