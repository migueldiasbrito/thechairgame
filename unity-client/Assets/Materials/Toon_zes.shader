Shader "Custom/Toon_zes"
{
   Properties
    {
         _color("display name", Color) = (1,1,1,1)
        _nBandas("n Bnadas", Range(0,10)) = 0
           _mainTex("main tex",2D) = "defalttexture" {}
    }

    SubShader
    {
     Cull off
        CGPROGRAM
        #pragma surface surf Toon


        struct Input {
             float2 uv_mainTex;
        };

    
        float3 _color;
        float _nBandas;
        
        sampler2D _mainTex;

       half4 LightingToon (SurfaceOutput s, half3 lightDir, half atten)
        {
            half NdotL = dot(s.Normal, lightDir);
            NdotL = floor(NdotL*_nBandas)/_nBandas;



            half4 result;
            result.rgb = s.Albedo * _LightColor0 * NdotL ;
            result.a = s.Alpha;
            return result;
        }


        
         void surf(Input IN, inout SurfaceOutput o) {
          //o.Albedo = tex2D(_mainTex,IN.uv_mainTex) * _color;
          o.Albedo =  _color;
     
        }

        ENDCG
    }
        FallBack "Diffuse"
        }