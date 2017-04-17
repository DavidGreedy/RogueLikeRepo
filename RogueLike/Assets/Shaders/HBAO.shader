// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Tutorial/Display Normals"
{
	Properties
	{

	}
	SubShader
	{
		Pass
		{
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"

				struct VertexIn
				{
					float4 Position : POSITION;
					float4 UV : TEXCOORD;
				};

				VertexIn vert(appdata_base v)
				{
					VertexIn o;
					o.Position = UnityObjectToClipPos(v.vertex);
					o.UV = float4(v.texcoord.xy, 0, 0);
					return o;
				}

				fixed4 frag(VertexIn i) : SV_Target
				{
					return fixed4(1,1,1,1);
				}
			ENDCG
		}
	}
}