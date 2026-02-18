Shader "Unlit/URP_SphereGlow"
{
  Properties
    {
        [Header(Colors)]
        _CenterColor ("Center Color", Color) = (1, 0.5, 0, 1) 
        _EdgeColor ("Edge Color", Color) = (1, 0, 0, 0)
        
        [Header(Shape)]
        _Falloff ("Base Falloff (Dimensione)", Range(0.5, 10.0)) = 3.0
        _Intensity ("Base Intensity (Luminosità)", Range(1.0, 10.0)) = 2.0
        
        [Header(Animation)]
        _PulseSpeed ("Pulse Speed (Velocità)", Range(0.1, 20.0)) = 5.0
        _PulseAmount ("Pulse Amount (Variazione)", Range(0.0, 1.0)) = 0.2
    }
    
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha 
            ZWrite Off 
            Cull Back 

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 normal : TEXCOORD0;
                float3 viewDir : TEXCOORD1;
            };

            float4 _CenterColor;
            float4 _EdgeColor;
            float _Falloff;
            float _Intensity;
            
            float _PulseSpeed;
            float _PulseAmount;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.viewDir = _WorldSpaceCameraPos - worldPos;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 normal = normalize(i.normal);
                float3 viewDir = normalize(i.viewDir);

                float wave1 = sin(_Time.y * _PulseSpeed);
                
                float wave2 = sin(_Time.y * _PulseSpeed * 2.54); 
                
                float noise = (wave1 + wave2 * 0.5); 
                
                float flicker = 1.0 + (noise * _PulseAmount);

                float NdotV = saturate(dot(normal, viewDir));
                
                float currentFalloff = _Falloff * (1.0 - (noise * _PulseAmount * 0.5));
                float shape = pow(NdotV, currentFalloff);

                fixed4 finalColor = lerp(_EdgeColor, _CenterColor, shape);

                finalColor.rgb *= _Intensity * flicker;

                finalColor.a *= shape;

                return finalColor;
            }
            ENDCG
        }
    }
}
