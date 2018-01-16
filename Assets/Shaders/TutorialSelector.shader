Shader "Spellbinder/TutorialSelector"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_RingColor ("Ring color", Color) = (1.0, 1.0, 1.0, 1.0)
	}

	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="False"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		ZTest [unity_GUIZTestMode]
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma shader_feature ETC1_EXTERNAL_ALPHA
			#include "UnityCG.cginc"
			
			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color;
				return OUT;
			}

			sampler2D _MainTex;
			fixed4 _RingColor;

			fixed4 frag(v2f IN) : SV_Target
			{
				float k = cos(_Time.w) * 0.05 + 0.05;
				float2 uv = IN.texcoord - float2(0.5, 0.5);
				float r = uv.x * uv.x + uv.y * uv.y;
				fixed4 c = lerp(fixed4(_RingColor.rgb, 0.0), fixed4(_RingColor.rgb, 1.0), smoothstep(0.15 - k, 0.2 - k, r));
				c.a *= (1.0 - smoothstep(0.15 - k, 0.25 - k, r));
				return c;
			}
		ENDCG
		}
	}
}
