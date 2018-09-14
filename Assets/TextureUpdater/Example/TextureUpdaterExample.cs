using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class TextureUpdaterExample : MonoBehaviour
{
    private struct PlasmaJob : IJobParallelFor
    {
        public NativeArray<Color32> colors;

        [ReadOnly] public int width;
        [ReadOnly] public int height;
        [ReadOnly] public float time;

        public void Execute(int index)
        {
            colors[index] = Plasma(index % width, index / width, width, height, time);
        }
        
        private static Color32 Plasma(int x, int y, int width, int height, float time)
        {
            var px = (float) x / width;
            var py = (float) y / height;

            var l = Mathf.Sin(px * Mathf.Sin(time * 1.3f) + Mathf.Sin(py * 4 + time) * Mathf.Sin(time));

            var r = (byte) (Mathf.Sin(l * 6) * 127 + 127);
            var g = (byte) (Mathf.Sin(l * 7) * 127 + 127);
            var b = (byte) (Mathf.Sin(l * 10) * 127 + 127);

            return new Color32(r, g, b, 255);
        }

    }

    private Texture texture;

    private NativeArray<Color32> colorBuffer;

    private void Awake()
    {
        TextureUpdater.Init();
    }

    private void OnDestroy()
    {
        TextureUpdater.Deinit();
        colorBuffer.Dispose();
    }

    private void Start()
    {
        texture = new Texture2D(256, 256, TextureFormat.RGBA32, false);
        GetComponent<Renderer>().material.mainTexture = texture;
        colorBuffer = new NativeArray<Color32>(texture.width * texture.height, Allocator.Persistent);
    }

    private JobHandle SchedulePlasma(int width, int height, float time)
    {
        var jobData = new PlasmaJob
        {
            colors = colorBuffer,
            width = width,
            height = height,
            time = time
        };
        return jobData.Schedule(colorBuffer.Length, 32);
    }

    private void Update()
    {
        var handle = SchedulePlasma(texture.width, texture.height, Time.time);
        handle.Complete();

        texture.Update(colorBuffer);
    }

}