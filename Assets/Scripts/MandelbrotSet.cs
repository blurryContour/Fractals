using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Numerics;

[System.Serializable]
public struct Pallete
{
    public Color color;
    public float time;
}

public class MandelbrotSet : MonoBehaviour
{
    public float RE_START;
    public float RE_END;
    public float IM_START;
    public float IM_END;
    public int MAX_ITER;
    public RawImage image;
    public bool useColorList;
    public Gradient colorPallete;
    public Pallete[] colorList;

    Texture2D texture;
    int N_COLORS;
    
    // Start is called before the first frame update
    void Start()
    {
        texture = new Texture2D(Screen.width, Screen.height);
        image.color = Color.white;
        
        N_COLORS = colorList.Length;
        if (useColorList)
        {
            CreateGradient();
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateTexture();
        image.texture = texture;
    }

    void UpdateTexture()
    {
        for (int x = 0; x < texture.width; x++)
        {
            for (int y = 0; y < texture.height; y++)
            {
                Complex c = new Complex(RE_START + ((float) x / texture.width) * (RE_END - RE_START),
                                        IM_START + ((float) y / texture.height) * (IM_END - IM_START));
                int n = Mandelbrot(c);
                float value = (float) n / MAX_ITER;
                Color color = colorPallete.Evaluate(value);
                texture.SetPixel(x, y, color);
            }
        }
        texture.Apply();
    }

    public int Mandelbrot(Complex c)
    {
        int n = 0;
        Complex z = Complex.Zero;
        while (n < MAX_ITER && Complex.Abs(z) <= 2)
        {
            z = z * z + c;
            n += 1;
        }
        return n;
    }

    void CreateGradient()
    {
        // Fixing gradient
        GradientColorKey[] gradientColorKeys = new GradientColorKey[N_COLORS];
        GradientAlphaKey[] gradientAlphaKeys = new GradientAlphaKey[2];
        for (int i = 0; i < N_COLORS; i++)
        {
            gradientColorKeys[i].color = colorList[i].color;
            gradientColorKeys[i].time = colorList[i].time;
        }
        gradientAlphaKeys[0].alpha = 1.0f;
        gradientAlphaKeys[0].time = 0;
        gradientAlphaKeys[1].alpha = 1.0f;
        gradientAlphaKeys[1].time = 1;

        colorPallete.SetKeys(gradientColorKeys, gradientAlphaKeys);
        colorPallete.mode = GradientMode.Blend;
    }
}
