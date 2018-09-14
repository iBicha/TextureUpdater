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
	// Use this for initialization
	IEnumerator Start () {
	    
		TextureUpdater.Init();
		
		texture = new Texture2D(256, 256, TextureFormat.RGBA32, false);
		GetComponent<Renderer>().material.mainTexture = texture;
		Color32[] colorBuffer = new Color32[texture.width * texture.height];

		for (int i = 0; i < colorBuffer.Length; i++)
		{
			colorBuffer[i] = GetRandomColor();
		}
		
		GCHandle handle = GCHandle.Alloc(colorBuffer, GCHandleType.Pinned);
		TextureUpdater.Update(texture, handle.AddrOfPinnedObject());
		
		yield return new WaitForSeconds(1f);
		TextureUpdater.Deinit();
		yield return new WaitForSeconds(1f);

		handle.Free();

	}
	
//	void Update()
//	{
//		for (int i = 0; i < colorBuffer.Length; i++)
//		{
//			colorBuffer[i] = GetRandomColor();
//		}
//		TextureUpdater.Update(texture, colorBuffer);
//	}

	private Color32 GetRandomColor()
	{
		return new Color32((byte) Random.Range(0, 255), (byte) Random.Range(0, 255), (byte) Random.Range(0, 255), 255);
	}

}
