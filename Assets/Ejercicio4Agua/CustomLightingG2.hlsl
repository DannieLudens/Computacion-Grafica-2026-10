//Include Guard
#ifndef CUSTOM_LIGHTING_G2
#define CUSTOM_LIGHTING_G2

//#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

void MainLight_float(float3 PositionWS, out float3 LightDir, out float3 LightColor, out float ShadowAtten)
{
    #ifndef SHADERGRAPH_PREVIEW
    float4 shadowCoord = TransformWorldToShadowCoord(PositionWS);
    Light mainLight = GetMainLight(shadowCoord);
    LightDir = mainLight.direction;
    LightColor = mainLight.color;
    ShadowAtten = mainLight.shadowAttenuation;
    #else
    LightDir = normalize(float3(1,1,-1));
    LightColor = 1;
    ShadowAtten = 1; 
    #endif
}

void AdditionalSimpleLit_float(float3 PositionWS, float3 NormalWS, float3 ViewDirWS, out float3 Diffuse, out float3 Specular)
{
    Diffuse = 0;
    Specular = 0;

    #ifndef SHADERGRAPH_PREVIEW
    uint additionalLightCount = GetAdditionalLightsCount();

    //TODO: Forward+
    
    LIGHT_LOOP_BEGIN(additionalLightCount) //Macro

    Light currentLight = GetAdditionalLight(lightIndex, PositionWS, 1);

    //Lambert
    float lambert = dot(NormalWS, currentLight.direction);
    lambert = max(0, lambert);
    Diffuse = lambert * currentLight.color * currentLight.shadowAttenuation * currentLight.distanceAttenuation;

    //Blinn-Phong Specular
    //BRDF
    //Bidirectional Reflection Distribution Function
    float3 h = normalize(currentLight.direction + ViewDirWS);
    float blinnPhong = dot(h, NormalWS);
    blinnPhong = max(0, blinnPhong);
    Specular = blinnPhong * currentLight.shadowAttenuation * currentLight.color * currentLight.distanceAttenuation;
    
    LIGHT_LOOP_END
    
    #endif
}

#endif


