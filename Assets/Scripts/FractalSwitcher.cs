using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FractalSwitcher : MonoBehaviour
{
    Fractal julia;
    Fractal mandelbrot;

    // Start is called before the first frame update
    void Start()
    {
        Fractal[] fractals = gameObject.GetComponents<Fractal>();
        foreach (Fractal fractal  in fractals)
        {
            Debug.Log(fractal.NAME);
            if (fractal.NAME == "Mandelbrot Set")
                mandelbrot = fractal;
            if (fractal.NAME == "Julia Set")
                julia = fractal;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.M))
        {
            mandelbrot.enabled = true;
            julia.enabled = false;
        }   
        if (Input.GetKey(KeyCode.J))
        {
            mandelbrot.enabled = false;
            julia.enabled = true;
        }   
    }
}
