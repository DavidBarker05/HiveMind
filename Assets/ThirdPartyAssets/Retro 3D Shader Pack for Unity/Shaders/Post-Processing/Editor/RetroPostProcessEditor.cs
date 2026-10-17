//////////////////////////////////////////////////
// Author:              LEAKYFINGERS
// Date created:        27.10.19
// Date last edited:    27.10.19
// URP port: now a VolumeComponentEditor for the RetroPostProcessEffect volume component.
//////////////////////////////////////////////////
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine.Rendering.Universal;


[CustomEditor(typeof(RetroPostProcessEffect))]
public sealed class RetroPostProcessEditor : VolumeComponentEditor
{
    private SerializedDataParameter usesFixedResolutionParameter;
    private SerializedDataParameter pixelScaleParameter;
    private SerializedDataParameter fixedVerticalResolutionParameter;

    private SerializedDataParameter colorDepthParameter;

    private SerializedDataParameter ditherPatternParameter;
    private SerializedDataParameter ditherPatternScaleParameter;
    private SerializedDataParameter ditherThresholdParameter;
    private SerializedDataParameter ditherIntensityParameter;

    // Called when the editor becomes active.
    public override void OnEnable()
    {
        // Connects the editor parameters to the appropriate variables in the RetroPostProcessEffect script.
        PropertyFetcher<RetroPostProcessEffect> o = new PropertyFetcher<RetroPostProcessEffect>(serializedObject);

        usesFixedResolutionParameter = Unpack(o.Find(x => x.UsesFixedResolution));
        pixelScaleParameter = Unpack(o.Find(x => x.PixelScale));
        fixedVerticalResolutionParameter = Unpack(o.Find(x => x.FixedVerticalResolution));

        colorDepthParameter = Unpack(o.Find(x => x.ColorDepth));

        ditherPatternParameter = Unpack(o.Find(x => x.DitherPattern));
        ditherPatternScaleParameter = Unpack(o.Find(x => x.DitherPatternScale));
        ditherThresholdParameter = Unpack(o.Find(x => x.DitherThreshold));
        ditherIntensityParameter = Unpack(o.Find(x => x.DitherIntensity));
    }

    // Called each time the inspector GUI is updated.
    public override void OnInspectorGUI()
    {
        EditorGUILayout.LabelField("Pixelation", EditorStyles.boldLabel);
        PropertyField(usesFixedResolutionParameter);
        // Displays the parameter that is currently enabled according to the 'fixed resolution mode' checkbox.
        if (usesFixedResolutionParameter.value.boolValue)
            PropertyField(fixedVerticalResolutionParameter);
        else
            PropertyField(pixelScaleParameter);

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Posterization", EditorStyles.boldLabel);
        PropertyField(colorDepthParameter);

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Dithering", EditorStyles.boldLabel);
        PropertyField(ditherPatternParameter);
        PropertyField(ditherPatternScaleParameter);
        PropertyField(ditherThresholdParameter);
        PropertyField(ditherIntensityParameter);
    }
}
