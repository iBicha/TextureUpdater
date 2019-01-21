# TextureUpdater

Texture Updater uses [CommandBuffer.IssuePluginCustomTextureUpdate](https://docs.unity3d.com/ScriptReference/Rendering.CommandBuffer.IssuePluginCustomTextureUpdate.html) to update the texture at runtime. In addition, the generated texture image is done in C# with the help of the job system and the burst compiler.

The Plasma effect example is ported from https://github.com/keijiro/TextureUpdateExample/

## Why?
`CommandBuffer.IssuePluginCustomTextureUpdate` is designed to work with native code, but any change of the logic requires recompiling the plugins for each platform. This approach makes the texture updater plugin reusable, as the texture update logic is moved to C#.

## known issues
 - Because the native thread is calling managed code (to generate the texture), it is causing the editor to hang on domain unload (the second time you press play).
The example is currently using [Texture2D.LoadRawTextureData](https://docs.unity3d.com/ScriptReference/Texture2D.LoadRawTextureData.html) in the editor to prevent this issue.
 - This repo is still uses [CommandBuffer.IssuePluginCustomTextureUpdate](https://docs.unity3d.com/ScriptReference/Rendering.CommandBuffer.IssuePluginCustomTextureUpdate.html) which is now obsolete in favor of [CommandBuffer.IssuePluginCustomTextureUpdateV2](https://docs.unity3d.com/2018.3/Documentation/ScriptReference/Rendering.CommandBuffer.IssuePluginCustomTextureUpdateV2.html)
