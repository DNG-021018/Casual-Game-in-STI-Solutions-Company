Shader "Hidden/InvisibleDepth"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        // Ghi depth, KHÔNG vẽ màu
        Pass
        {
            Name "DepthOnly"
            Cull Back
            ZWrite On
            ZTest LEqual
            ColorMask 0
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct appdata { float4 vertex:POSITION; };
            struct v2f { float4 pos:SV_POSITION; };
            v2f vert(appdata v){ v2f o; o.pos = UnityObjectToClipPos(v.vertex); return o; }
            fixed4 frag(v2f i):SV_Target { return 0; } // không xuất màu
            ENDCG
        }
    }
    FallBack Off
}
