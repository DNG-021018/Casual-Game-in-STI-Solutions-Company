Shader "Mobile/Particles/Additive2_Stencil" {
    Properties {
        _MainTex ("Particle Texture", 2D) = "white" {}
    }

    Category {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        Blend SrcAlpha One
        Cull Off Lighting Off ZWrite Off
        Fog { Color (0,0,0,0) }

        BindChannels {
            Bind "Color", color
            Bind "Vertex", vertex
            Bind "TexCoord", texcoord
        }

        SubShader {
            Pass {
                Stencil {
                    Ref 1
                    Comp Equal
                    Pass Keep
                }

                SetTexture [_MainTex] {
                    combine texture * primary
                }
            }
        }
    }
}
