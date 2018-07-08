# Simple GIF C# library

<a href='https://ko-fi.com/S6S5DWU2' target='_blank'><img height='36' style='border:0px;height:36px;' src='https://az743702.vo.msecnd.net/cdn/kofi2.png?v=0' border='0' alt='Buy Me a Coffee at ko-fi.com' /></a>

Hi! This project is a pure implementation of GIF based on GIF89a Specification.

Simple GIF is a lightweight cross-platform library. It doesn't use Windows system libs (like System.Drawing and System.Windows.Media) or any 3rd-party components. It also supports parallel decoding/encoding that can greatly speed up a performance.

Basically, I've created it for processing GIF files in my mobile app Pixel Studio 2018 that was made with Unity 2018. It's Mono based game engine, so you can't just include the system libraries to your project. You can test this lib with Pixel Studio 2018: https://play.google.com/store/apps/details?id=com.PixelStudio

Features:
- lightweight (~20KB binary)
- pure (no references)
- fast and parallel
- proper namings (according to GIF spec)
- well formatted (ReSharper)
- well commented

Requirements:
- set target platform .NET Framework 3.5 if you want to use this lib with Unity
- set target platform .NET Framework 4.0 to speed up a multithreading

Usage example:
- var gif = Gif.Decode(byte[] bytes); will load GIF from a binary
- var gif = new Gif(List<GifFrame> frames); will create GIF from an image list
- gif.Encode(); will return a binary ready to be displayed or written on a disk
- gif.Frames contains a frame list
- each frame has Texture and Delay
- Texture is just an abstract Color32 array (Color32 structure has RGBA byte values)
- Delay is a frame delay in milliseconds

Advanced usage example:
- var count = Gif.GetDecodeIteratorSize(bytes); will return DecodeIterator size so you can display a progress bar for large files
- var iterator = Gif.DecodeIterator(); will return iterator so you can display a progress bar for large files
- var count = gif.GetEncodeIteratorSize(); will return EncodeIterator size so you can display a progress bar for large files (it always returns frame count - 2)
- var iterator = gif.EncodeIterator(); will return iterator so you can display a progress bar for large files (the last iterator element will be a GIF header, please refer to EncodeIteratorExample)
- please refer to DecodeParallelExample and EncodeParallelExample example if you want to use a multithreading

Advanced usage notes:
- a penultimate element of EncodeIterator is GIF Trailer (0x3B)
- EncodeIterator will build a global color table "on fly", that's why a GIF header (bytes) will be the last iterator element! You need to insert this element into the resulting byte sequence beginning before writing a file (use LINQ InsertRange or refer to EncodeIteratorExample)
- calling Count() for an iterator will result a full iterator 'execution', that's why you need to use GetDecodeIteratorSize and GetEncodeIteratorSize for displaying a progress bar (progress = i / IteratorSize).

Notes:
- Texture2D and Color32 stubs were introduced to avoid referencing Unity libraries. Thus, this lib can be used outside of Unity =)
- Texture2D is a stub for Unity Texture2D (UnityEngine.CoreModule)
- Color32 is a stub for Unity Color32 (UnityEngine.CoreModule)

GIF89a Specification
- https://www.w3.org/Graphics/GIF/spec-gif89a.txt

Feedback, questions, bugs:
- <a href="https://github.com/hippogamesunity/SimpleGif/issues">Issues</a>

Contact me:
- hippogamesunity@gmail.com
