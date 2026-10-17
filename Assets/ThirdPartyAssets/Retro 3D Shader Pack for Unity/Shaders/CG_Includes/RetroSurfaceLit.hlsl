//////////////////////////////////////////////////
// URP port of RetroFlatLit.cginc / RetroUnityLit.cginc / the Sprite (Lit) surface code.
// Replaces the built-in surface shaders (StandardSpecular) with a forward pass using URP's physically based lighting (specular workflow).
// Optional defines (set before including):
//   RETRO_FLAT_NORMAL  - lights the mesh with the flat face normal.
//   RETRO_NORMAL_MAP   - samples _BumpMap.
//   RETRO_VERTEX_COLOR - multiplies albedo/alpha by the vertex color.
//   RETRO_TWO_SIDED    - flips the normal on back faces (use with Cull Off).
//////////////////////////////////////////////////
#ifndef _RETRO_3D_SURFACE_LIT_SHADER_
#define _RETRO_3D_SURFACE_LIT_SHADER_

#define _SPECULAR_SETUP 1 // Matches StandardSpecular: the specular color comes from the specular color/map rather than a metallic value.

#include "./RetroURPCommon.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

struct Attributes
{
	float4 positionOS : POSITION;
	float3 normalOS : NORMAL;
	float4 tangentOS : TANGENT;
	float2 texcoord : TEXCOORD0;
	#ifdef RETRO_VERTEX_COLOR
		float4 color : COLOR;
	#endif
};

struct Varyings
{
	float4 positionCS : SV_POSITION;
	float2 uv : TEXCOORD0;
	float3 uvAffine : TEXCOORD1;
	float3 positionWS : TEXCOORD2;
	half3 normalWS : TEXCOORD3;
	half4 tangentWS : TEXCOORD4;
	float drawDistClip : TEXCOORD5; // 0 == false, 1 == true.
	half fogFactor : TEXCOORD6;
	#ifdef RETRO_VERTEX_COLOR
		half4 color : TEXCOORD7;
	#endif
};

Varyings vert(Attributes input)
{
	Varyings o;

	RetroVertex rv = RetroTransformVertex(input.positionOS.xyz, input.texcoord);
	o.positionCS = rv.positionCS;
	o.positionWS = rv.positionWS;
	o.uv = input.texcoord;
	o.uvAffine = rv.uvAffine;
	o.drawDistClip = rv.drawDistClip;
	o.fogFactor = ComputeFogFactor(rv.positionCS.z);

	VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS, input.tangentOS);
	o.normalWS = normalInputs.normalWS;
	o.tangentWS = half4(normalInputs.tangentWS, input.tangentOS.w * GetOddNegativeScale());

	#ifdef RETRO_VERTEX_COLOR
		o.color = input.color;
	#endif

	return o;
}

#ifdef RETRO_TWO_SIDED
	#define RETRO_FACE_ARG , FRONT_FACE_TYPE isFront : FRONT_FACE_SEMANTIC
#else
	#define RETRO_FACE_ARG
#endif

half4 frag(Varyings i RETRO_FACE_ARG) : SV_Target
{
	// Draw distance:
	if (i.drawDistClip > 0)
		clip(-1);

	float2 finalUV = RetroFinalUV(i.uv, i.uvAffine);

	// Surface:
	half4 c = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, finalUV);

	SurfaceData surfaceData = (SurfaceData)0;
	surfaceData.albedo = c.rgb * _Color.rgb;
	surfaceData.alpha = c.a * _Color.a;
	#ifdef RETRO_VERTEX_COLOR
		surfaceData.albedo *= i.color.rgb;
		surfaceData.alpha *= i.color.a;
	#endif

	surfaceData.specular = _SpecularColor.rgb;
	#ifdef USING_SPECULAR_MAP
		surfaceData.specular = SAMPLE_TEXTURE2D(_SpecGlossMap, sampler_SpecGlossMap, finalUV).rgb;
	#endif
	surfaceData.smoothness = _Glossiness;
	surfaceData.occlusion = 1;

	#ifdef EMISSION_ENABLED
		surfaceData.emission = _EmissionColor.rgb;
		#ifdef USING_EMISSION_MAP
			surfaceData.emission *= SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, finalUV).rgb;
		#endif
	#endif

	// Normal:
	half3 normalWS;
	#ifdef RETRO_FLAT_NORMAL
		// Flat shading: the flattened world-space face normal is the cross product of the x and y position derivatives relative to the camera.
		float3 cameraRelativeWorldPos = i.positionWS - GetCameraPositionWS();
		normalWS = normalize(cross(ddy(cameraRelativeWorldPos), ddx(cameraRelativeWorldPos)));
	#else
		half3 vertexNormalWS = normalize(i.normalWS);
		#ifdef RETRO_TWO_SIDED
			vertexNormalWS *= IS_FRONT_VFACE(isFront, 1.0, -1.0);
		#endif
		#ifdef RETRO_NORMAL_MAP
			half3 normalTS = UnpackNormal(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, finalUV));
			float sgn = i.tangentWS.w;
			half3 bitangentWS = sgn * cross(vertexNormalWS, i.tangentWS.xyz);
			normalWS = TransformTangentToWorld(normalTS, half3x3(i.tangentWS.xyz, bitangentWS, vertexNormalWS));
		#else
			normalWS = vertexNormalWS;
		#endif
	#endif

	// Lighting:
	InputData inputData = (InputData)0;
	inputData.positionWS = i.positionWS;
	inputData.positionCS = i.positionCS;
	inputData.normalWS = NormalizeNormalPerPixel(normalWS);
	inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(i.positionWS);
	#if defined(MAIN_LIGHT_CALCULATE_SHADOWS)
		inputData.shadowCoord = TransformWorldToShadowCoord(i.positionWS);
	#endif
	inputData.fogCoord = i.fogFactor;
	inputData.bakedGI = SampleSH(inputData.normalWS);
	inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(i.positionCS);
	inputData.shadowMask = half4(1, 1, 1, 1);

	half4 color = UniversalFragmentPBR(inputData, surfaceData);
	color.rgb = MixFog(color.rgb, inputData.fogCoord);

	return color;
}

#endif
