using System.Numerics;

namespace exo2nuunlm;

public class DrawData
{
    public string FilePath { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double ScaleX { get; set; }
    public double ScaleY { get; set; }
    public double Rotation { get; set; }
    public double Opacity { get; set; }
    public bool ReverseX { get; set; }
    public bool ReverseY { get; set; }

    public int BlendMode { get; set; }
}
