using System;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using Random = UnityEngine.Random;

public class TextureUpdaterExample : MonoBehaviour
{
    private Texture texture;

    private Color32[] colorBuffer;

    private GCHandle handle;

    // Use this for initialization
    void Start()
    {
        TextureUpdater.Init();

        texture = new Texture2D(256, 256, TextureFormat.RGBA32, false);
        GetComponent<Renderer>().material.mainTexture = texture;
        colorBuffer = new Color32[texture.width * texture.height];
        handle = GCHandle.Alloc(colorBuffer, GCHandleType.Pinned);
    }

    void Update()
    {
        Plasma(colorBuffer, texture.width, texture.height);
        TextureUpdater.Update(texture, handle.AddrOfPinnedObject());
    }

    private void OnDestroy()
    {
        TextureUpdater.Deinit();
        handle.Free();
    }

    void Plasma(Color32[] colors, int width, int height)
    {
        var time = Time.time;
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
            colors[y * width + x] = Plasma(x, y, width, height, time);
    }

    Color32 Plasma(int x, int y, int width, int height, float time)
    {
        var px = (float) x / width;
        var py = (float) y / height;

        var l = Mathf.Sin(px * Mathf.Sin(time * 1.3f) + Mathf.Sin(py * 4 + time) * Mathf.Sin(time));

        var r = (byte) (Mathf.Sin(l * 6) * 127 + 127);
        var g = (byte) (Mathf.Sin(l * 7) * 127 + 127);
        var b = (byte) (Mathf.Sin(l * 10) * 127 + 127);

        return new Color32(r, g, b, 255);
    }

    private Color32 GetRandomColor()
    {
        return new Color32((byte) Random.Range(0, 255), (byte) Random.Range(0, 255), (byte) Random.Range(0, 255), 255);
    }
}