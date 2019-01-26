#if UNITY_2018_3_OR_NEWER && !(UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN) //Windows plugin not updated yet
#define SUPPORTS_TEXTURE_UPDATE_V2
#endif

#if SUPPORTS_TEXTURE_UPDATE_V2
#define USE_TEXTURE_UPDATE_V2
#endif

using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using AOT;
using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
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

    [StructLayout(LayoutKind.Sequential)]
    public struct UnityRenderingExtTextureUpdateParamsV2
    {        
        public IntPtr texData; // source data for the texture update. Must be set by the plugin
        public IntPtr textureID; // texture ID of the texture to be updated.
        public uint userData; // user defined data. Set by the plugin
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
        if (initialized) return;

        commandBuffer = new CommandBuffer();
        pointerBuffers = new Dictionary<uint, IntPtr>();
        textureUpdateCallbackPtr = GetTextureUpdateCallback();
        SetTextureEventFunctions(OnUpdateTextureBegin, OnUpdateTextureEnd);
        
#if USE_TEXTURE_UPDATE_V2
        SetTextureEventFunctionsV2(OnUpdateTextureBeginV2, OnUpdateTextureEndV2);
#endif
        
        initialized = true;
    }

    public static void Deinit()
    {
        if (!initialized) return;

        if (commandBuffer != null)
        {
            commandBuffer.Dispose();
            commandBuffer = null;
        }

        SetTextureEventFunctions(null, null);
        
#if USE_TEXTURE_UPDATE_V2
        SetTextureEventFunctionsV2(null, null);
#endif

        initialized = false;
    }

    public static unsafe void Update(this Texture texture, byte[] content)
    {
        fixed (byte* contentPtr = content)
        {
            Update(texture, new IntPtr(contentPtr));
        }
    }

    public static unsafe void Update(this Texture texture, Color32[] content)
    {
        fixed (Color32* contentPtr = content)
        {
            Update(texture, new IntPtr(contentPtr));
        }
    }

    public static unsafe void Update<T>(this Texture texture, NativeArray<T> content) where T : struct
    {
        Update(texture, new IntPtr(content.GetUnsafePtr()));
    }

    public static void Update(this Texture texture, IntPtr content)
    {
        Init();

        var id = lastEventId++;
        pointerBuffers.Add(id, content);
#if USE_TEXTURE_UPDATE_V2
        commandBuffer.IssuePluginCustomTextureUpdateV2(textureUpdateCallbackPtr, texture, id);
#else
        commandBuffer.IssuePluginCustomTextureUpdate(textureUpdateCallbackPtr, texture, id);
#endif
        Graphics.ExecuteCommandBuffer(commandBuffer);
        commandBuffer.Clear();
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void TextureUpdateEventDelegate(IntPtr eventParamsPtr);

    [DllImport(libraryName, EntryPoint = "TextureUpdater_GetTextureUpdateCallback")]
    private static extern IntPtr GetTextureUpdateCallback();

    [DllImport(libraryName, EntryPoint = "TextureUpdater_SetTextureEventFuntions")]
    private static extern void SetTextureEventFunctions(TextureUpdateEventDelegate onUpdateTextureBegin,
        TextureUpdateEventDelegate onUpdateTextureEnd);

    [DllImport(libraryName, EntryPoint = "TextureUpdater_SetTextureEventFuntionsV2")]
    private static extern void SetTextureEventFunctionsV2(TextureUpdateEventDelegate onUpdateTextureBeginV2,
        TextureUpdateEventDelegate onUpdateTextureEndV2);

    private static uint lastEventId;
    private static Dictionary<uint, IntPtr> pointerBuffers;

    [MonoPInvokeCallback(typeof(TextureUpdateEventDelegate))]
    private static void OnUpdateTextureBegin(IntPtr eventParamsPtr)
    {
        var eventParams = (UnityRenderingExtTextureUpdateParams) Marshal.PtrToStructure(eventParamsPtr,
            typeof(UnityRenderingExtTextureUpdateParams));
        pointerBuffers.TryGetValue(eventParams.userData, out eventParams.texData);
        Marshal.StructureToPtr(eventParams, eventParamsPtr, false);
    }

    [MonoPInvokeCallback(typeof(TextureUpdateEventDelegate))]
    private static void OnUpdateTextureEnd(IntPtr eventParamsPtr)
    {
        var eventParams = (UnityRenderingExtTextureUpdateParams) Marshal.PtrToStructure(eventParamsPtr,
            typeof(UnityRenderingExtTextureUpdateParams));
        pointerBuffers.Remove(eventParams.userData);
    }
    
    [MonoPInvokeCallback(typeof(TextureUpdateEventDelegate))]
    private static void OnUpdateTextureBeginV2(IntPtr eventParamsPtr)
    {
        var eventParams = (UnityRenderingExtTextureUpdateParamsV2) Marshal.PtrToStructure(eventParamsPtr,
            typeof(UnityRenderingExtTextureUpdateParamsV2));
        pointerBuffers.TryGetValue(eventParams.userData, out eventParams.texData);
        Marshal.StructureToPtr(eventParams, eventParamsPtr, false);
    }

    [MonoPInvokeCallback(typeof(TextureUpdateEventDelegate))]
    private static void OnUpdateTextureEndV2(IntPtr eventParamsPtr)
    {
        var eventParams = (UnityRenderingExtTextureUpdateParamsV2) Marshal.PtrToStructure(eventParamsPtr,
            typeof(UnityRenderingExtTextureUpdateParamsV2));
        pointerBuffers.Remove(eventParams.userData);
    }
}