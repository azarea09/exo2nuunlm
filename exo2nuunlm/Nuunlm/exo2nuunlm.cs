using static System.Formats.Asn1.AsnWriter;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using Potesara;
using System.Diagnostics.Metrics;

namespace exo2nuunlm;

public static class exo2nuunlm
{
    public static void Save(string path)
    {
        ExoParser exo = new ExoParser(path);
        AnimationData animation = new AnimationData();

        animation.TextureFileNames = exo.textureFileNames;
        animation.FrameLength = exo.FrameLength - 1;

        for (int i = 0; i < exo.FrameLength; i++)
        {
            FrameData frame = new FrameData();
            frame.FrameNumber = i;
            frame.DrawObjects = new List<DrawData>();

            for (int j = 0; j < exo.imageObjects.Count; j++)
            {
                var imageObject = exo.imageObjects[j];

                if (i + 1 < imageObject.StartFrame || i + 1 > imageObject.EndFrame)
                {
                    continue;
                }

                exo.UpdateTransform(imageObject, i + 1);
                exo.ApplyFilter(imageObject, i + 1);
                exo.ApplyGroupObject(imageObject, i + 1);

                DrawData drawData = new DrawData();
                drawData.FilePath = imageObject.FilePath;
                drawData.X = 960 + imageObject.Transfrom.Position.X;
                drawData.Y = 540 + imageObject.Transfrom.Position.Y;
                drawData.ScaleX = imageObject.Transfrom.ScaleX;
                drawData.ScaleY = imageObject.Transfrom.ScaleY;
                drawData.Rotation = imageObject.Transfrom.Rotation * (MathF.PI / 180);
                drawData.Opacity = 255 * imageObject.Transfrom.Opacity;
                drawData.ReverseX = imageObject.Transfrom.ReverseX;
                drawData.ReverseY = imageObject.Transfrom.ReverseY;
                drawData.BlendMode = imageObject.BlendMode;

                frame.DrawObjects.Add(drawData);
            }


            animation.nuunlm.Add(frame);
        }

        Json.Save(animation, Path.GetFileNameWithoutExtension(path) + ".nuunlm");
    }
}
