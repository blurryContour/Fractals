using UnityEngine;
using UnityEngine.UI;


public class MandelbrotCS : MonoBehaviour
{
    public double RE_START;
    public double RE_END;
    public double IM_START;
    public double IM_END;
    public int MAX_ITER;
    public double ZOOM;
    public int maxIterIncrement;
    public float RE_CENTER;
    public float IM_CENTER;

    public ComputeShader shader;
    ComputeBuffer buffer;
    RenderTexture texture;
    public RawImage image;

    double _RE_START;
    double _RE_END;
    double _IM_START;
    double _IM_END;
    int _MAX_ITER;

    Vector2 mouse;

    public struct DataStruct
    {
        public double re_s, re_e, im_s, im_e;
        public int width, height; 
    }

    DataStruct[] data;

    // Start is called before the first frame update
    void Start()
    {
        image.color = Color.white;

        texture = new RenderTexture(Screen.width, Screen.height, 0);
        texture.enableRandomWrite = true;
        texture.Create();

        data = new DataStruct[1];
        data[0] = new DataStruct
        {
            re_s = RE_START,
            re_e = RE_END,
            im_s = IM_START,
            im_e = IM_END,
            width = texture.width,
            height = texture.height
        };
        buffer = new ComputeBuffer(data.Length, 40);

        Mandelbrot();

        // Store original values
        _RE_START = RE_START;
        _RE_END = RE_END;
        _IM_START = IM_START;
        _IM_END = IM_END;
        _MAX_ITER = MAX_ITER;

        mouse = new Vector2();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
            ZoomIn(true);
        if (Input.GetMouseButton(1))
            ZoomOut(true);

        if (Input.GetKey(KeyCode.A))
            ZoomIn(false);
        if (Input.GetKey(KeyCode.D))
            ZoomOut(false);
        
        if (Input.GetKey(KeyCode.R))
            ZoomReset();
    }

    void ZoomIn(bool useMouse)
    {
        SetZoomCenter(useMouse);

        MAX_ITER = (int) Mathf.Min(400, Mathf.Max(_MAX_ITER, MAX_ITER + maxIterIncrement));
        
        double deltaRE = (RE_END - RE_START) * ZOOM * Time.deltaTime;
        RE_START += deltaRE * mouse.x / Screen.width;
        RE_END -= deltaRE * (1 - mouse.x / Screen.width);

        double deltaIM = (IM_END - IM_START) * ZOOM * Time.deltaTime;
        IM_START += deltaIM * mouse.y / Screen.height;
        IM_END -= deltaIM * (1 - mouse.y / Screen.height);

        data[0].re_s = RE_START;
        data[0].re_e = RE_END;
        data[0].im_s = IM_START;
        data[0].im_e = IM_END;
        
        Mandelbrot();
    }

    void ZoomOut(bool useMouse)
    {
        SetZoomCenter(useMouse);
        
        MAX_ITER = (int) Mathf.Min(400, Mathf.Max(_MAX_ITER, MAX_ITER + maxIterIncrement));

        double deltaRE = (RE_END - RE_START) * ZOOM * Time.deltaTime;
        RE_START -= deltaRE * mouse.x / Screen.width;
        RE_END += deltaRE * (1 - mouse.x / Screen.width);

        double deltaIM = (IM_END - IM_START) * ZOOM * Time.deltaTime;
        IM_START -= deltaIM * mouse.y / Screen.height;
        IM_END += deltaIM * (1 - mouse.y / Screen.height);

        data[0].re_s = RE_START;
        data[0].re_e = RE_END;
        data[0].im_s = IM_START;
        data[0].im_e = IM_END;

        Mandelbrot();
    }

    void ZoomReset()
    {
        RE_START = _RE_START;
        RE_END = _RE_END;
        IM_START = _IM_START;
        IM_END = _IM_END;
        MAX_ITER = _MAX_ITER;

        data[0].re_s = RE_START;
        data[0].re_e = RE_END;
        data[0].im_s = IM_START;
        data[0].im_e = IM_END;
        MAX_ITER = _MAX_ITER;

        Mandelbrot();
    }

    void SetZoomCenter(bool useMouse)
    {
        if (useMouse)
        {
            mouse.x = Input.mousePosition.x;
            mouse.y = Input.mousePosition.y;
        }
        else
        {
            mouse.x = (float)((RE_CENTER - _RE_START) / (_RE_END - _RE_START) * texture.width);
            mouse.y = (float)((IM_CENTER - _IM_START) / (_IM_END - _IM_START) * texture.height);
        }
    }


    void Mandelbrot()
    {
        int kernelIndex = shader.FindKernel("CSMain");

        buffer.SetData(data);
        shader.SetBuffer(kernelIndex, "buffer", buffer);
        shader.SetInt("maxIterations", MAX_ITER);
        shader.SetTexture(kernelIndex, "Result", texture);

        shader.Dispatch(kernelIndex, texture.width / 24, texture.height / 24, 1);

        RenderTexture.active = texture;
        image.material.mainTexture = texture;
    }

    private void OnDestroy()
    {
        buffer.Dispose();
    }
}
