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
	void Start () {
	    
		TextureUpdater.Init();
		
		texture = new Texture2D(256, 256, TextureFormat.RGBA32, false);
		GetComponent<Renderer>().material.mainTexture = texture;
		colorBuffer = new Color32[texture.width * texture.height];		
	    handle = GCHandle.Alloc(colorBuffer, GCHandleType.Pinned);
	}
	
	void Update()
	{
		for (int i = 0; i < colorBuffer.Length; i++)
		{
			colorBuffer[i] = GetRandomColor();
		}
		TextureUpdater.Update(texture, handle.AddrOfPinnedObject());
	}

	private void OnDestroy()
	{
		TextureUpdater.Deinit();
		handle.Free();
	}

	private Color32 GetRandomColor()
	{
		return new Color32((byte) Random.Range(0, 255), (byte) Random.Range(0, 255), (byte) Random.Range(0, 255), 255);
	}

}
