Shader "Sprites/CustomOutline"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_TexelScale("TexelScale", Float) = 1
		_AlphaThreshold("Alpha Threshold", Range(0.01,1)) = 0.01
		_OutlineColor("[Outline] Tint", Color) = (1,1,1,1)
		_OutlineSize("[Outline] Size", Range(5,10)) = 5
		_OutlineMinAlpha("[Outline] Min Alpha", Range(0,1)) = 0.15
		_OutlinePulseFreq("[Outline] Pulse Freq", Float) = 1
		_OutlinePulseMaxAlphaOffset("[Outline] Pulse Max Alpha Offset", Range(0,1)) = 0.15
	}
	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Fade"
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas" = "True"
		}
		//LOD 100

		Cull Off
		Lighting Off
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#pragma target 4.0
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_TexelSize;
			fixed4 _OutlineColor;
			int _OutlineSize;
			float _AlphaThreshold;
			fixed _TexelScale;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			fixed _OutlineMinAlpha = 0.1;

			fixed4 ComputeOutlineColor_EdgePass(float2 texCoord, int i, int j) {
				fixed4 outputColor = fixed4(0, 0, 0, 0);
				//UpRight
				if (tex2D(_MainTex, texCoord + float2(_MainTex_TexelSize.x * j * _TexelScale, _MainTex_TexelSize.y * (i - j) * _TexelScale)).a > _AlphaThreshold) {
					outputColor = _OutlineColor;
					outputColor.a = lerp(outputColor.a, _OutlineMinAlpha, (float(i) - 1) / float(_OutlineSize-1));
					return outputColor;
				}

				//DownLeft
				if (tex2D(_MainTex, texCoord - float2(_MainTex_TexelSize.x * j * _TexelScale, _MainTex_TexelSize.y * (i - j) * _TexelScale)).a > _AlphaThreshold) {
					outputColor = _OutlineColor;
					outputColor.a = lerp(outputColor.a, _OutlineMinAlpha, (float(i) - 1) / float(_OutlineSize-1));
					return outputColor;
				}

				//UpLeft
				if (tex2D(_MainTex, texCoord + float2(_MainTex_TexelSize.x * j * -_TexelScale, _MainTex_TexelSize.y * (i - j) * _TexelScale)).a > _AlphaThreshold) {
					outputColor = _OutlineColor;
					outputColor.a = lerp(outputColor.a, _OutlineMinAlpha, (float(i) - 1) / float(_OutlineSize-1));
					return outputColor;
				}

				//DownRight
				if (tex2D(_MainTex, texCoord - float2(_MainTex_TexelSize.x * j * -_TexelScale, _MainTex_TexelSize.y * (i - j) * _TexelScale)).a > _AlphaThreshold) {
					outputColor = _OutlineColor;
					outputColor.a = lerp(outputColor.a, _OutlineMinAlpha, (float(i) - 1) / float(_OutlineSize-1));
					return outputColor;
				}

				return outputColor;

			}

			fixed4 ComputeOutlineColor_DirectPass(float2 texCoord, int i) {
				fixed4 outputColor = fixed4(0, 0, 0, 0);
				// Up
				if (tex2D(_MainTex, texCoord + float2(0, _MainTex_TexelSize.y * i * _TexelScale)).a > _AlphaThreshold) {
					outputColor = _OutlineColor;
					outputColor.a = lerp(outputColor.a, _OutlineMinAlpha, (float(i) - 1) / float(_OutlineSize-1));
					//outputColor = fixed4(0, 1, 0, 1);
					return outputColor;
				}
				//Down
				if (tex2D(_MainTex, texCoord - float2(0, _MainTex_TexelSize.y * i * _TexelScale)).a > _AlphaThreshold) {
					outputColor = _OutlineColor;
					outputColor.a = lerp(outputColor.a, _OutlineMinAlpha, (float(i) - 1) / float(_OutlineSize-1));
					return outputColor;
				}
				//Right
				if (tex2D(_MainTex, texCoord + float2(_MainTex_TexelSize.x * i * _TexelScale, 0)).a > _AlphaThreshold) {
					outputColor = _OutlineColor;
					outputColor.a = lerp(outputColor.a, _OutlineMinAlpha, (float(i) - 1) / float(_OutlineSize-1));
					return outputColor;
				}
				//Left
				if (tex2D(_MainTex, texCoord - float2(_MainTex_TexelSize.x * i * _TexelScale, 0)).a > _AlphaThreshold) {
					outputColor = _OutlineColor;
					outputColor.a = lerp(outputColor.a, _OutlineMinAlpha, (float(i) - 1) / float(_OutlineSize-1));
					return outputColor;
				}


				if (i > 1) {
					[unroll(10)]
					for (int j = 1; j < i + 1; j++)
					{
						outputColor = ComputeOutlineColor_EdgePass(texCoord, i, j);
						if (outputColor.a > 0) {
							return outputColor;
						}
					}
				}

				return outputColor;
			}

			fixed4 ComputeOutlineColor(float2 texCoord) {
				fixed4 outputColor = fixed4(0, 0, 0, 0);
				[unroll(10)]
				for (int i = 1; i < _OutlineSize + 1; i++) {
					outputColor = ComputeOutlineColor_DirectPass(texCoord, i);
					if (outputColor.a > 0) {
						return outputColor;
					}

				}
				return outputColor;

			}
			
			fixed _OutlinePulseFreq;
			fixed _OutlinePulseMaxAlphaOffset;

			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				//col.rgb += _Color.rgb;

				if (col.a < _AlphaThreshold) {
					col = ComputeOutlineColor(i.uv);

					col.a -= ((cos(_Time.y*3.141592*_OutlinePulseFreq) + 1)/2)*_OutlinePulseMaxAlphaOffset;
					}
				else {
					col = fixed4(0, 0, 0, 0);
				}
				// apply fog
				//UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}
