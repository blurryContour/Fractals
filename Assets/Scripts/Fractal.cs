using UnityEngine;
using UnityEngine.UI;

public class Fractal : MonoBehaviour
{
    public string NAME;

    public double RE_START;
    public double RE_END;
    public double IM_START;
    public double IM_END;

    [Range(-1, 1)]
    public double RE_CONST;
    [Range(-1, 1)]
    public double IM_CONST;

    public int MAX_ITER;
    public double ZOOM;
    public int maxIterIncrement;
    
    public float RE_CENTER;
    public float IM_CENTER;
    public bool autoZoom;

    public ComputeShader shader;
    internal ComputeBuffer buffer;
    internal RenderTexture texture;
    public RawImage image;

    internal int _MAX_ITER;
    internal double _RE_START;
    internal double _RE_END;
    internal double _IM_START;
    internal double _IM_END;

    internal Vector2 mouse;

    public struct DataStruct
    {
        public double re_s, re_e;
        public double im_s, im_e;
        public double re_c, im_c;
        public int width, height;
    }

    internal DataStruct[] data;

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
            re_c = RE_CONST,
            im_c = IM_CONST,
            width = texture.width,
            height = texture.height
        };
        int bufferSize = sizeof(double) * 6 + sizeof(int) * 2;
        buffer = new ComputeBuffer(data.Length, bufferSize);

        // Store original values
        _RE_START = RE_START;
        _RE_END = RE_END;
        _IM_START = IM_START;
        _IM_END = IM_END;
        _MAX_ITER = MAX_ITER;

        mouse = new Vector2();

        ComputeFractal();
        SaveTexture();
    }

    // Update is called once per frame
    void Update()
    {
        ComputeFractal();

        if (Input.GetKey(KeyCode.LeftArrow))
            RE_CONST -= 0.001f;
        if (Input.GetKey(KeyCode.RightArrow))
            RE_CONST += 0.001f;

        if (Input.GetKey(KeyCode.UpArrow))
            IM_CONST -= 0.001f;
        if (Input.GetKey(KeyCode.DownArrow))
            IM_CONST += 0.001f;

        if (Input.GetMouseButton(0))
            ZoomIn(true);
        if (Input.GetMouseButton(1))
            ZoomOut(true);

        if (Input.GetKey(KeyCode.A) || autoZoom)
            ZoomIn(false);
        if (Input.GetKey(KeyCode.D))
            ZoomOut(false);

        if (Input.GetKey(KeyCode.R))
            ZoomReset();
    }

    void ZoomIn(bool useMouse)
    {
        SetZoomCenter(useMouse);

        MAX_ITER = (int)Mathf.Min(400, Mathf.Max(_MAX_ITER, MAX_ITER + maxIterIncrement));

        double deltaRE = (RE_END - RE_START) * ZOOM * Time.deltaTime;
        RE_START += deltaRE * mouse.x / Screen.width;
        RE_END -= deltaRE * (1 - mouse.x / Screen.width);

        double deltaIM = (IM_END - IM_START) * ZOOM * Time.deltaTime;
        IM_START += deltaIM * mouse.y / Screen.height;
        IM_END -= deltaIM * (1 - mouse.y / Screen.height);
    }

    void ZoomOut(bool useMouse)
    {
        SetZoomCenter(useMouse);

        MAX_ITER = (int)Mathf.Min(400, Mathf.Max(_MAX_ITER, MAX_ITER + maxIterIncrement));

        double deltaRE = (RE_END - RE_START) * ZOOM * Time.deltaTime;
        RE_START -= deltaRE * mouse.x / Screen.width;
        RE_END += deltaRE * (1 - mouse.x / Screen.width);

        double deltaIM = (IM_END - IM_START) * ZOOM * Time.deltaTime;
        IM_START -= deltaIM * mouse.y / Screen.height;
        IM_END += deltaIM * (1 - mouse.y / Screen.height);
    }

    void ZoomReset()
    {
        RE_START = _RE_START;
        RE_END = _RE_END;
        IM_START = _IM_START;
        IM_END = _IM_END;
        MAX_ITER = _MAX_ITER;
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


    internal void ComputeFractal()
    {
        data[0].re_s = RE_START;
        data[0].re_e = RE_END;
        data[0].im_s = IM_START;
        data[0].im_e = IM_END;
        data[0].re_c = RE_CONST;
        data[0].im_c = IM_CONST;

        int kernelIndex = shader.FindKernel("CSMain");

        buffer.SetData(data);
        shader.SetBuffer(kernelIndex, "buffer", buffer);
        shader.SetInt("maxIterations", MAX_ITER);
        shader.SetTexture(kernelIndex, "Result", texture);

        shader.Dispatch(kernelIndex, texture.width / 24, texture.height / 24, 1);

        RenderTexture.active = texture;
        image.texture = texture;
    }

    public void SaveTexture()
    {
        Texture2D tex = ToTexture2D(texture);
        byte[] bytes = tex.EncodeToPNG();
        System.IO.File.WriteAllBytes("Assets/SavedScreen.png", bytes);
    }

    Texture2D ToTexture2D(RenderTexture rTex)
    {
        Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.RGB24, false);
        RenderTexture.active = rTex;
        tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex.Apply();
        return tex;
    }

    private void OnDestroy()
    {
        if (buffer != null)
            buffer.Dispose();
    }
}
