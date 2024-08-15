Shader "Custom/Color Blinker"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
		_Show("Show", Range(0, 1)) = 1
		_BlinkStart("Blink Starting Point", Vector) = (0,-2,0)
		_BlinkEnd("Blink Ending Point", Vector) = (0,2,0)
		_BlinkBand("Blink Band Width", Float) = 0.6
		_BlinkFalloff("Blink Band Falloff Width", Float) = 0.1
		_BlinkSpeed("Blink Speed", Float) = 5
		_BlinkPeriod("Blink Cycle Period", Float) = 3
	}
	SubShader
	{
		Tags 
		{ 
			"RenderType" = "Transparent" 
			"Queue" = "Transparent+1"
		}
		LOD 10

		ZWrite Off
		Offset -1, -1
		//ZTest LEqual
		//Cull Off

		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
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
				float vertexPoint : TEXCOORD1;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _Color;
			fixed _Show;

			float4 _BlinkStart;
			float4 _BlinkEnd;
			float _BlinkBand;
			float _BlinkFalloff;

			float _BlinkSpeed;
			float _BlinkPeriod;

			// calcuate the blink
			static float blinkCurrentPosition = (_Time.y * _BlinkSpeed) % (_BlinkPeriod * _BlinkSpeed);

			// precalculate the direction vector of the blinker in world space
			static float4 blinkStart = _BlinkStart;
			static float4 blinkEnd = _BlinkEnd;
			static float4 blinkDirection = normalize(blinkEnd - blinkStart);
			static float blinkHeight = distance(blinkEnd, blinkStart);

			// rescale the blink position to also include the length of the band on both ends
			static float totalBandLength = (_BlinkBand / 2 + _BlinkFalloff + 0.001) / blinkHeight;
			static float scaledCurrentPosition = lerp(0 - totalBandLength, 1 + totalBandLength, blinkCurrentPosition);

			// calculate where the start and end points of both rising and falling component of the band lie on the direction vector
			static float risingCurveStartPoint = scaledCurrentPosition - (_BlinkBand / 2 + _BlinkFalloff) / blinkHeight;
			static float risingCurveEndPoint = scaledCurrentPosition - (_BlinkBand / 2) / blinkHeight;
			static float fallingCurveStartPoint = scaledCurrentPosition + (_BlinkBand / 2) / blinkHeight;
			static float fallingCurveEndPoint = scaledCurrentPosition + (_BlinkBand / 2 + _BlinkFalloff) / blinkHeight;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);

				// calculate where this vertex lies on the direction vector
				o.vertexPoint = dot(mul(unity_ObjectToWorld, v.vertex) - blinkStart, blinkDirection) / blinkHeight;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// calculate the both smooth step curves that contributes to the rising and falling component 
				float risingCurveStrength = smoothstep(risingCurveStartPoint, risingCurveEndPoint, i.vertexPoint);
				float fallingCurveStrength = 1 - smoothstep(fallingCurveStartPoint, fallingCurveEndPoint, i.vertexPoint);
				return fixed4(_Color.rgb, _Color.a * saturate(risingCurveStrength * fallingCurveStrength)) * step(0.5, _Show);
			}
			ENDCG
		}
	}
}
