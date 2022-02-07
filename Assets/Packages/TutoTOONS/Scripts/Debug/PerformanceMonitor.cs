using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PerformanceMonitor : MonoBehaviour
{
    [Tooltip("Graph max value ms")] [SerializeField] float maxDeltaTime = 0.1f;
    [Tooltip("Times per Second")] [SerializeField] float fpsUpdateRate = 1f;

    private int maxMsCount = 300;
    private int msCount = 0;
    private float fps;
    private float deltaTime = 0f;
    private int frameCount = 0;
    Texture2D texture1, texture2, currentTexture;
    Color backgroundColor;
    GameObject canvas;
    RawImage rawImage1, rawImage2;
    RectMask2D rectMask2D;
    bool created = false;

    void Start()
    {
        if (!created)
        {
            if (FindObjectsOfType<PerformanceMonitor>().Length > 1)
            {
                Destroy(this);
                return;
            }

            CreateCanvas();
            DontDestroyOnLoad(canvas);

            PerformanceMonitor newPerformanceMonitor = canvas.AddComponent<PerformanceMonitor>();
            newPerformanceMonitor.AddComponents(canvas);
            Destroy(this);
        }
    }

    void Update()
    {
        CalculateFps();
        LogUpdate();
        CheckToTurnOffOnGraph();
        MoveImages(rawImage1);
        MoveImages(rawImage2);
    }

    public void AddComponents(GameObject canvas)
    {
        this.canvas = canvas;
        CreateImages();
        created = true;
    }

    private void MoveImages(RawImage image)
    {
        image.rectTransform.localPosition += new Vector3(-1f, 0f, 0f);
        if (image.rectTransform.localPosition.x <= -300)
        {
            image.rectTransform.localPosition = new Vector3(300f, 0f, 0f);
        }
    }

    private void CreateCanvas()
    {
        canvas = new GameObject("PerformanceMonitorCanvas");
        canvas.AddComponent<Canvas>();
        canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
    }

    private void CreateImages()
    {
        CanvasScaler canvasScaler = canvas.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(Screen.width, Screen.height);
        canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;

        GameObject child = new GameObject("Mask");
        child.transform.parent = canvas.transform;
        rectMask2D = child.AddComponent<RectMask2D>();
        rectMask2D.rectTransform.position = new Vector2(Screen.width * 0.025f + maxMsCount / 2f, Screen.height * 0.025f + maxMsCount / 4);
        rectMask2D.rectTransform.sizeDelta = new Vector2(maxMsCount, maxMsCount * 2);
        rectMask2D.transform.localScale = new Vector3(1f, 0.15f, 1f);

        backgroundColor = Color.black;
        backgroundColor.a = 0.2f;

        CreateImageAndTexture(rectMask2D, out texture1, out rawImage1);
        CreateImageAndTexture(rectMask2D, out texture2, out rawImage2);

        rawImage2.rectTransform.localPosition += new Vector3(300f, 0f, 0f);
        currentTexture = texture2;
    }

    private void CreateImageAndTexture(RectMask2D rectMask2D, out Texture2D texture, out RawImage image)
    {
        GameObject child = new GameObject("RawImage");
        child.transform.parent = rectMask2D.transform;
        image = child.AddComponent<RawImage>();
        image.rectTransform.localPosition = Vector2.zero;
        texture = new Texture2D(maxMsCount, maxMsCount * 2, UnityEngine.Experimental.Rendering.DefaultFormat.HDR, UnityEngine.Experimental.Rendering.TextureCreationFlags.None);
        image.texture = texture;
        image.SetNativeSize();
        image.transform.localScale = new Vector3(1f, 1f, 1f);
        texture.filterMode = FilterMode.Point;

        for (int i = 0; i < texture.width; i++)
        {
            for (int j = 0; j < texture.height; j++)
            {
                texture.SetPixel(i, j, backgroundColor);
            }
        }
        texture.Apply();
    }

    void CalculateFps()
    {
        frameCount++;
        deltaTime += Time.unscaledDeltaTime;
        if (deltaTime > 1 / fpsUpdateRate)
        {
            fps = frameCount / deltaTime;
            frameCount = 0;
            deltaTime -= 1 / fpsUpdateRate;
        }
    }

    void OnGUI()
    {
        int w = Screen.width;
        int h = Screen.height;

        GUIStyle style = new GUIStyle();
        Rect rect = new Rect(0, 0, w, h / 50);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = h * 2 / 30;
        style.normal.textColor = new Color(0.0f, 0.0f, 0.5f, 1.0f);
        string text = string.Format("{0:0.0} fps", fps);
        GUI.Label(rect, text, style);
    }

    private void LogUpdate()
    {
        int ms = (int)(Time.unscaledDeltaTime * currentTexture.height / maxDeltaTime);
        for (int i = 0; i < currentTexture.height; i++)
        {
            currentTexture.SetPixel(msCount, i, backgroundColor);
            if (i <= ms)
            {
                currentTexture.SetPixel(msCount, i, Color.green);
            }
        }
        msCount++;
        currentTexture.Apply();
        if (msCount == maxMsCount)
        {
            msCount = 0;
            if (currentTexture == texture1)
            {
                currentTexture = texture2;
            }
            else
            {
                currentTexture = texture1;
            }
        }
    }

    private void CheckToTurnOffOnGraph()
    {
        if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (Input.mousePosition.x <= Screen.width * 0.12f && Input.mousePosition.y >= Screen.height * 0.935f)
                {
                    rectMask2D.gameObject.SetActive(!rectMask2D.gameObject.activeSelf);
                }
            }
        }
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            if (Input.GetTouch(0).phase == TouchPhase.Began)
            {
                if (Input.GetTouch(0).position.x <= Screen.width * 0.12f && Input.GetTouch(0).position.y >= Screen.height * 0.935f)
                {
                    rectMask2D.gameObject.SetActive(!rectMask2D.gameObject.activeSelf);
                }
            }
        }
    }
}
