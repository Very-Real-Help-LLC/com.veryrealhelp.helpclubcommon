using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class VeryRealHelpCutoutShaderGUI : ShaderGUI
{
    private Texture2D alphaPlot;
    private MaterialProperty alphaPosition;
    private MaterialProperty alphaSlope;

    private float lastPosition;
    private float lastSlope;
    
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        base.OnGUI(materialEditor, properties);
        foreach (var item in properties)
        {
            if (item.name == "_CutoffPosition")
            {
                alphaPosition = item;
            }
            else if (item.name == "_CutoffSlope")
            {
                alphaSlope = item;
            }
        }
        bool redraw = false;
        if (lastPosition != alphaPosition.floatValue)
        {
            lastPosition = alphaPosition.floatValue;
            redraw = true;
        }
        if (lastSlope != alphaSlope.floatValue)
        {
            lastSlope = alphaSlope.floatValue;
            redraw = true;
        }
        if (redraw)
        {
            UpdateAlphaPlot();
        }
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("Alpha Cutoff Curve");
        EditorGUILayout.LabelField("A steeper curve results in harsher edges.", EditorStyles.miniLabel);
        EditorGUILayout.EndVertical();
        GUILayout.Box(alphaPlot);
        EditorGUILayout.EndHorizontal();
    }

    private void UpdateAlphaPlot()
    {
        float interp(float position, float slope, float value)
        {
            float c = 2f / (1f - slope) - 1f;
            float f(float x, float n) => Mathf.Pow(x, c) / Mathf.Pow(n, c - 1);
            return value < position ? f(value, position) : (1 - f(1 - value, 1 - position));
        }
        if (alphaPlot == null)
        {
            alphaPlot = new Texture2D(64, 64, TextureFormat.RGBA32, false);
        }
        float t;
        for (int x = 0; x < alphaPlot.width; x++)
        {
            t = x / (float)alphaPlot.width;
            for (int y = 0; y < alphaPlot.height; y++)
            {
                var z = interp(alphaPosition.floatValue, alphaSlope.floatValue, t);
                var color = z < (y / (float)alphaPlot.height) ? Color.white : Color.black;
                var refColor = x < y ? Color.white : Color.black;
                alphaPlot.SetPixel(x, y, Color.Lerp(color, refColor, 0.25f));
            }
        }
        alphaPlot.Apply();
    }
}
