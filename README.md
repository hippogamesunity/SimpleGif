# Simple GIF C# library

<a href='https://ko-fi.com/S6S5DWU2' target='_blank'><img height='36' style='border:0px;height:36px;' src='https://az743702.vo.msecnd.net/cdn/kofi2.png?v=0' border='0' alt='Buy Me a Coffee at ko-fi.com' /></a>

Hi! This project is pure implementation of GIF based on GIF89a Specification.

Simple GIF is lightweight cross-platform library. It doesn't use Windows system libs (like System.Drawing and System.Windows.Media) or any 3rd-party components.

Basically, I've created it for processing GIF files in my mobile app Pixel Studio 2018 that was made with Unity 2018. It's Mono based game engine, so you can't just include system libraries to your project. You can test this lib with Pixel Studio 2018: https://play.google.com/store/apps/details?id=com.PixelStudio

Features:
- lightweight (18KB binary)
- pure (no references)
- fast (want to believe)
- proper namings (according to GIF spec)
- well formatted (ReSharper)
- well commented

Requirements:
- set target platform .NET Framework 3.5 if you want to use this lib with Unity

Usage example:
- var gif = Gif.Decode(byte[] bytes); will load GIF from binary
- var gif = new Gif(List<GifFrame> frames); will create GIF from image list
- gif.Encode(); will return binary ready to be displayed or written on disk
- gif.Frames contains frame list
- each frame has Texture and Delay
- Texture is just abstract Color32 array (Color32 structure has RGBA byte values)
- Delay is frame delay in milliseconds

Advanced usage example:
- var count = Gif.GetDecodeIteratorSize(bytes); will return DecodeIterator size so you can display progress bar for large files
- var iterator = Gif.DecodeIterator(); will return iterator so you can display progress bar for large files
- var count = gif.GetEncodeIteratorSize(); will return EncodeIterator size so you can display progress bar for large files (it always returns frame count - 2)
- var iterator = gif.EncodeIterator(); will return iterator so you can display progress bar for large files (last iterator element will be GIF header, please refer to EncodeIteratorExample)

Advanced usage notes:
- penultimate element of EncodeIterator is GIF Trailer (0x3B)
- EncodeIterator will build global color table "on fly", that's why GIF header (bytes) will be the last iterator element! You need to insert this element into resulting byte sequence beginning before writing file (use LINQ InsertRange or refer to EncodeIteratorExample)
- calling Count() for iterator will result full iterator 'execution', that's why you need to use GetDecodeIteratorSize and GetEncodeIteratorSize for displaying progress bar (progress = i / IteratorSize).


Nodes:
- Texture2D and Color32 stubs were introduced to avoid referencing Unity libraries. Thus, this lib can be used outside of Unity =)
- Texture2D is a stub for Unity Texture2D (UnityEngine.CoreModule)
- Color32 is a stub for Unity Color32 (UnityEngine.CoreModule)

GIF89a Specification
- https://www.w3.org/Graphics/GIF/spec-gif89a.txt

Feedback, questions, bugs:
- <a href="https://github.com/hippogamesunity/SimpleGif/issues">Issues</a>

Contact me:
- hippogamesunity@gmail.com
