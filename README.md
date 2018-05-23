# SimpleGif
Simple GIF is pure implementation of GIF based on GIF89a Specification.

Simple GIF is cross-platform library. It doesn't use Windows system libs (like System.Drawing and System.Windows.Media) or any 3rd-party components.

Basically, I've created it for processing GIF files in my mobile app Pixel Studio that was made with Unity 2018. It's Mono based game engine, so you can't just include system libraries to your project.

Requirements:
- set target platform .NET Framework 3.5 if you want to use this lib with Unity

Usage example:
- var gif = Gif.FromBytes(byte[] bytes) will load GIF from binary
- var gif = new Gif(List<GifFrame> frames) will create GIF from image list
- gif.GetBytes() will return binary ready to be displayed or written on disk
- gif.Frames contains frame list
- each frame Texture and Delay
- Texture is just abstract Color32 array (Color32 structure has RGBA byte values)
- Delay is frame delay in milliseconds
  
Nodes:
- Texture2D and Color32 stubs were introduced to avoid referencing Unity libraries. Thus, this lib can be used outside of Unity =)
- Texture2D is a stub for Unity Texture2D (UnityEngine.CoreModule)
- Color32 is a stub for Unity Color32 (UnityEngine.CoreModule)

GIF89a Specification
- https://www.w3.org/Graphics/GIF/spec-gif89a.txt

Feedback, questions, bugs:
- please navigate to Issues section!

Contact me:
- hippogamesunity@gmail.com
