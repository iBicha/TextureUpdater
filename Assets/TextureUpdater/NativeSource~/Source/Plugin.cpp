#include <stdint.h>
#include <IUnityRenderingExtensions.h>

typedef void (*TextureUpdateEvent)(UnityRenderingExtTextureUpdateParams* params);

static TextureUpdateEvent s_OnUpdateTextureBegin;
static TextureUpdateEvent s_OnUpdateTextureEnd;

void TextureUpdateCallback(int eventID, void* data)
{
    auto event = static_cast<UnityRenderingExtEventType>(eventID);

    if (event == kUnityRenderingExtEventUpdateTextureBegin)
    {
        auto params = reinterpret_cast<UnityRenderingExtTextureUpdateParams*>(data);
        if(s_OnUpdateTextureBegin)
            s_OnUpdateTextureBegin(params);
    } else if (event == kUnityRenderingExtEventUpdateTextureEnd)
    {
        auto params = reinterpret_cast<UnityRenderingExtTextureUpdateParams*>(data);
        if(s_OnUpdateTextureEnd)
            s_OnUpdateTextureEnd(params);
    }
}

extern "C" void UNITY_INTERFACE_EXPORT
TextureUpdater_SetTextureEventFuntions(TextureUpdateEvent onUpdateTextureBegin, TextureUpdateEvent onUpdateTextureEnd)
{
    s_OnUpdateTextureBegin = onUpdateTextureBegin;
    s_OnUpdateTextureEnd = onUpdateTextureEnd;
}

extern "C" UnityRenderingEventAndData UNITY_INTERFACE_EXPORT
TextureUpdater_GetTextureUpdateCallback()
{
    return TextureUpdateCallback;
}

