using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

public class TextureUpdaterExample : MonoBehaviour
{
    [BurstCompile(Accuracy.Std, Support.Relaxed)]
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
        texture = new Texture2D(512, 512, TextureFormat.RGBA32, false);
        GetComponent<Renderer>().material.mainTexture = texture;
        colorBuffer = new NativeArray<Color32>(texture.width * texture.height, Allocator.Persistent);
    }

    private void ScheduleAndWaitForPlasmaJob(NativeArray<Color32> colors, int width, int height, float time)
    {
        var jobData = new PlasmaJob
        {
            colors = colors,
            width = width,
            height = height,
            time = time
        };
        var handle = jobData.Schedule(colorBuffer.Length, 32);
        handle.Complete();
    }

    private void Update()
    {
        ScheduleAndWaitForPlasmaJob(colorBuffer, texture.width, texture.height, Time.time);
        
        //BUG: texture.Update(colorBuffer) hangs the editor on .Net 4.6
#if UNITY_EDITOR
        var tex2d = (Texture2D) texture;
        unsafe
        {
            tex2d.LoadRawTextureData(new IntPtr(colorBuffer.GetUnsafePtr()), colorBuffer.Length * 4);
        }
        tex2d.Apply();
#else
        texture.Update(colorBuffer);
#endif
        
    }
    
    

}