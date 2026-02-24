Shader "Unlit/S_AURA"
{
   Properties
    {
        _MainTex ("Texture (Albedo)", 2D) = "white" {}
        _Color ("Hologram Color", Color) = (0, 0.5, 1, 1)
        _RimPower ("Rim Power", Range(0.5, 8.0)) = 3.0
        _EffectHeight ("Effect Height", Range(-1, 1)) = 0.0
        _NoiseFrequency ("Aura Jaggedness", Range(1, 20)) = 5.0
        _NoiseStrength ("Aura Wave Strength", Range(0, 0.5)) = 0.1
        _Speed ("Rotation Speed", Range(0, 10)) = 2.0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        // Fondamentale: Renderizza entrambi i lati del cilindro
        Cull Off 
        ZWrite Off
        Blend SrcAlpha One // Effetto Additivo (tipico degli ologrammi)

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 localPos : TEXCOORD1;
                float3 viewDir : TEXCOORD2;
                float3 normal : NORMAL;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _RimPower;
            float _EffectHeight;
            float _NoiseFrequency;
            float _NoiseStrength;
            float _Speed;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                
                // Salviamo la posizione locale per calcolare l'altezza e l'angolo
                o.localPos = v.vertex.xyz;
                
                // Calcoliamo la direzione della vista per l'effetto Fresnel (rim light)
                o.viewDir = normalize(ObjSpaceViewDir(v.vertex));
                o.normal = v.normal;
                
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 1. CALCOLO DELL'ANGOLO (per l'effetto rotante attorno al cilindro)
                // Usiamo atan2 sulle coordinate X e Z locali per sapere "dove siamo" nella circonferenza
                float angle = atan2(i.localPos.x, i.localPos.z);
                
                // 2. CREAZIONE DEL BORDO FRASTAGLIATO (Aura)
                // Creiamo un'onda sinusoidale basata sull'angolo + tempo per la rotazione
                float noise = sin(angle * _NoiseFrequency + _Time.y * _Speed) * _NoiseStrength;
                
                // 3. CLIPPING DELL'ALTEZZA
                // Se la Y locale è maggiore dell'altezza impostata (+ il rumore), scartiamo il pixel
                float currentHeightLimit = _EffectHeight + noise;
                
                // Usiamo smoothstep per un bordo leggermente sfumato invece di un taglio netto
                float alphaMask = smoothstep(currentHeightLimit + 0.05, currentHeightLimit, i.localPos.y);
                
                // Se vuoi un taglio netto "hard clip", usa: 
                // if (i.localPos.y > currentHeightLimit) discard;

                // 4. EFFETTO OLOGRAFICO (Fresnel / Rim Light)
                // Più la normale è perpendicolare alla vista, più il bordo brilla
                float rim = 1.0 - saturate(dot(normalize(i.viewDir), normalize(i.normal)));
                rim = pow(rim, _RimPower);

                // 5. SCANLINES (Linee orizzontali tipiche degli ologrammi)
                float scanlines = sin(i.localPos.y * 50 + _Time.w) * 0.1 + 0.9;

                // Colore finale
                fixed4 col = _Color * rim * scanlines;
                col.a *= alphaMask; // Applichiamo la maschera di altezza frastagliata

                return col;
            }
            ENDCG
        }
    }
}
