using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using AOT;
using System;
using System.Text;
using UnityEngine.Rendering;


public static class TextureUpdater
{
#if UNITY_EDITOR
    private const string libraryName = "TextureUpdater";
#elif UNITY_IOS || UNITY_IPHONE || UNITY_WEBGL
	private const string libraryName = "__Internal";
#else
    private const string libraryName = "TextureUpdater";
#endif
    
    [StructLayout(LayoutKind.Sequential)]
    public struct UnityRenderingExtTextureUpdateParams
    {
        public IntPtr texData; // source data for the texture update. Must be set by the plugin
        public uint userData; // user defined data. Set by the plugin

        public uint textureID; // texture ID of the texture to be updated.
        public int format; // format of the texture to be updated
        public uint width; // width of the texture
        public uint height; // height of the texture
        public uint bpp; // texture bytes per pixel.
    }

    private static CommandBuffer commandBuffer;
    private static IntPtr textureUpdateCallbackPtr;

    private static bool initialized;
    
//    static TextureUpdater()
//    {
//        Init();
//    }
//
    public static void Init()
    {
        if(initialized) return;
    
        Debug.Log("Init");
        commandBuffer = new CommandBuffer();
        pointerBuffers = new Dictionary<uint, IntPtr>();
        textureUpdateCallbackPtr = GetTextureUpdateCallback();
        SetTextureEventFuntions(OnUpdateTextureBegin, OnUpdateTextureEnd);

        initialized = true;
    }

    public static void Deinit()
    {
        if(!initialized) return;

        Debug.Log("Deinit");
        
        if (commandBuffer != null)
        {
            commandBuffer.Dispose();
            commandBuffer = null;
        }
        
        SetTextureEventFuntions(null, null);

        initialized = false;
    }

//    public static void Update(Texture texture, byte[] content)
//    {
//        unsafe
//        {
//            fixed (byte* contentPtr = content)
//            {
//                Update(texture, new IntPtr(contentPtr));
//            }
//        }
//    }
//
//    public static void Update(Texture texture, Color32[] content)
//    {
//        unsafe
//        {
//            fixed (Color32* contentPtr = content)
//            {
//                Update(texture, new IntPtr(contentPtr));
//            }
//        }
//    }

    public static void Update(Texture texture, IntPtr content)
    {
        Init();
        
        var id = lastEventId++;
        pointerBuffers.Add(id, content);
        commandBuffer.IssuePluginCustomTextureUpdate(textureUpdateCallbackPtr, texture, id);
        Graphics.ExecuteCommandBuffer(commandBuffer);
        commandBuffer.Clear();
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void TextureUpdateEventDelegate(IntPtr eventParamsPtr);

    [DllImport(libraryName, EntryPoint = "TextureUpdater_GetTextureUpdateCallback")]
    private static extern IntPtr GetTextureUpdateCallback();

    [DllImport(libraryName, EntryPoint = "TextureUpdater_SetTextureEventFuntions")]
    private static extern void SetTextureEventFuntions(TextureUpdateEventDelegate onUpdateTextureBegin,
        TextureUpdateEventDelegate onUpdateTextureEnd);
    
    private static uint lastEventId;
    private static Dictionary<uint, IntPtr> pointerBuffers;
        
    [MonoPInvokeCallback(typeof(TextureUpdateEventDelegate))]
    private static void OnUpdateTextureBegin(IntPtr eventParamsPtr)
    {
        var eventParams = Marshal.PtrToStructure<UnityRenderingExtTextureUpdateParams>(eventParamsPtr);
        pointerBuffers.TryGetValue(eventParams.userData, out eventParams.texData);
        Marshal.StructureToPtr(eventParams, eventParamsPtr, false);
    }
    
    [MonoPInvokeCallback(typeof(TextureUpdateEventDelegate))]
    private static void OnUpdateTextureEnd(IntPtr eventParamsPtr)
    {
        var eventParams = Marshal.PtrToStructure<UnityRenderingExtTextureUpdateParams>(eventParamsPtr);
        pointerBuffers.Remove(eventParams.userData);
    }

}