#include <stdint.h>
#include <IUnityRenderingExtensions.h>

typedef void (*TextureUpdateEvent)(UnityRenderingExtTextureUpdateParams* params);
typedef void (*TextureUpdateEventV2)(UnityRenderingExtTextureUpdateParamsV2* params);

static TextureUpdateEvent s_OnUpdateTextureBegin;
static TextureUpdateEvent s_OnUpdateTextureEnd;
static TextureUpdateEventV2 s_OnUpdateTextureBeginV2;
static TextureUpdateEventV2 s_OnUpdateTextureEndV2;

void TextureUpdateCallback(int eventID, void* data)
{
    auto event = static_cast<UnityRenderingExtEventType>(eventID);
	
	switch(event) {
	    case kUnityRenderingExtEventUpdateTextureBegin: 
	    	{
		        auto params = reinterpret_cast<UnityRenderingExtTextureUpdateParams*>(data);
		        if(s_OnUpdateTextureBegin)
		            s_OnUpdateTextureBegin(params);
	    	}
            break;
	    case kUnityRenderingExtEventUpdateTextureEnd: 
	    	{
		        auto params = reinterpret_cast<UnityRenderingExtTextureUpdateParams*>(data);
		        if(s_OnUpdateTextureEnd)
		            s_OnUpdateTextureEnd(params);
	    	}
            break;
	    case kUnityRenderingExtEventUpdateTextureBeginV2: 
	    	{
		        auto params = reinterpret_cast<UnityRenderingExtTextureUpdateParamsV2*>(data);
		        if(s_OnUpdateTextureBeginV2)
		            s_OnUpdateTextureBeginV2(params);
	    	}
            break;
	    case kUnityRenderingExtEventUpdateTextureEndV2: 
	    	{
		        auto params = reinterpret_cast<UnityRenderingExtTextureUpdateParamsV2*>(data);
		        if(s_OnUpdateTextureEndV2)
		            s_OnUpdateTextureEndV2(params);
	    	}
            break;
        default:
        	break;
	}
}

extern "C" void UNITY_INTERFACE_EXPORT
TextureUpdater_SetTextureEventFuntions(TextureUpdateEvent onUpdateTextureBegin, TextureUpdateEvent onUpdateTextureEnd)
{
    s_OnUpdateTextureBegin = onUpdateTextureBegin;
    s_OnUpdateTextureEnd = onUpdateTextureEnd;
}

extern "C" void UNITY_INTERFACE_EXPORT
TextureUpdater_SetTextureEventFuntionsV2(TextureUpdateEventV2 onUpdateTextureBeginV2, TextureUpdateEventV2 onUpdateTextureEndV2)
{
    s_OnUpdateTextureBeginV2 = onUpdateTextureBeginV2;
    s_OnUpdateTextureEndV2 = onUpdateTextureEndV2;
}

extern "C" UnityRenderingEventAndData UNITY_INTERFACE_EXPORT
TextureUpdater_GetTextureUpdateCallback()
{
    return TextureUpdateCallback;
}

