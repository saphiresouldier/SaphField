Shader "Custom/Fresnel"
{
  Properties
  {
    _FresnelColor("Fresnel Color", Color) = (1,1,1,1)
    _FresnelBias("Fresnel Bias", Float) = 0
    _FresnelScale("Fresnel Scale", Float) = 1
    _FresnelPower("Fresnel Power", Float) = 1
  }

    SubShader
    {
      Tags
      {
        "Queue" = "Transparent"
        "IgnoreProjector" = "True"
        "RenderType" = "Transparent"
      }

      Cull Back
      ZWrite Off
      Blend SrcAlpha OneMinusSrcAlpha // Traditional transparency

      Pass
      {
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag alpha:fade
        #pragma target 3.0

        #include "UnityCG.cginc"

        struct appdata_t
        {
          float4 pos : POSITION;
          half3 normal : NORMAL;
        };

        struct v2f
        {
          float4 pos : SV_POSITION;
          float fresnel : TEXCOORD1;
        };

        float4 _MainTex_ST;
        fixed4 _Color;
        fixed4 _FresnelColor;
        fixed _FresnelBias;
        fixed _FresnelScale;
        fixed _FresnelPower;

        v2f vert(appdata_t v)
        {
          v2f o;
          o.pos = UnityObjectToClipPos(v.pos);

          float3 i = normalize(ObjSpaceViewDir(v.pos));
          o.fresnel = saturate(_FresnelBias + _FresnelScale * pow(1 + dot(i, v.normal), _FresnelPower));
          return o;
        }

        fixed4 frag(v2f i) : SV_Target
        {
          return fixed4(_FresnelColor.rgb, 1 - i.fresnel);
        }
        ENDCG
      }
    }
}