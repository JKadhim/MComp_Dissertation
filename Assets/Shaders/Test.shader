Shader "Custom/Test"
{
    Properties
    {
        _Color ("Base Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // Include necessary Unity shader libraries
            #include "UnityCG.cginc"

            // Uniforms
            float4 _Color;

            // Vertex input structure
            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR; // Vertex color from the mesh
            };

            // Vertex output structure
            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 color : COLOR;
            };

            // Vertex shader
            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex); // Transform vertex to clip space
                o.color = v.color * _Color; // Combine vertex color with base color
                return o;
            }

            // Fragment shader
            float4 frag(v2f i) : SV_Target
            {
                return i.color; // Output the final color
            }
            ENDCG
        }
    }
}
