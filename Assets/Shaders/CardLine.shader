Shader "Spellbinder/CardLine"
{
	Properties
	{
		_MainColor("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off
		Cull Off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 texcoord : TEXCOORD0;
			};

			fixed4 _MainColor;
			sampler2D _MainTex;
			
			v2f vert (appdata v)
			{
			  v2f o;
			  o.vertex = UnityObjectToClipPos(v.vertex);
			  o.texcoord = v.texcoord;
			  return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
			  fixed4 c = tex2D(_MainTex, i.texcoord) * _MainColor;
			  return c;
			}
			ENDCG
		}
	}
}

