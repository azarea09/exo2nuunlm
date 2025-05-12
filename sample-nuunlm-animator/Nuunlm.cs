using Amaoto;
using System.IO;

namespace samplenuunlmanimator;

public class Nuunlm
{
    public AnimationData animationData { get; set; } = new AnimationData();

    public Dictionary<string, Texture> TextureDictionary = new Dictionary<string, Texture>();

    private Dictionary<int, List<DrawData>> framesByNumber = new(); // フレーム番号と描画データのリストを格納する

    private Counter Counter;

    public void Load(string path)
    {
        animationData = Json.Load<AnimationData>(path);
        foreach (var textureFileName in animationData.TextureFileNames)
        {
            string filePath = Path.Combine(Directory.GetParent(path).FullName, textureFileName);
            TextureDictionary.Add(textureFileName, new Texture(filePath));
        }

        // フレームごとに描画データを整理
        foreach (var frame in animationData.nuunlm)
        {
            if (!framesByNumber.ContainsKey(frame.FrameNumber))
            {
                framesByNumber[frame.FrameNumber] = new List<DrawData>();
            }
            framesByNumber[frame.FrameNumber].AddRange(frame.DrawObjects);
        }
    }

    public void Start(bool isLoop)
    {
        Counter = new Counter(0, animationData.FrameLength, 16666.67, isLoop);
        Counter.Start();
    }
    public void Stop()
    {
        if (Counter == null) return;
        Counter.Stop();
        Counter.Reset();
    }

    public void Update()
    {
        if (Counter == null) return;
        Counter.Tick();
    }

    public void Draw(double opacity = 255)
    {
        if (Counter == null) return;
        if (!IsPlaying()) return;
        if (opacity <= 0.0) return;
        if (framesByNumber.TryGetValue((int)Counter.Value, out var drawDataList))
        {
            foreach (var drawData in drawDataList)
            {
                if (TextureDictionary.TryGetValue(drawData.FilePath, out var texture))
                {
                    // 前回の設定と異なる場合のみ更新
                    if (texture.ScaleX != drawData.ScaleX) texture.ScaleX = drawData.ScaleX;
                    if (texture.ScaleY != drawData.ScaleY) texture.ScaleY = drawData.ScaleY;
                    if (texture.Rotation != drawData.Rotation) texture.Rotation = drawData.Rotation;
                    if (texture.Opacity != drawData.Opacity || opacity != 255.0) texture.Opacity = drawData.Opacity * (opacity / 255.0);
                    if (texture.BlendMode != GetBlendMode(drawData.BlendMode))
                        texture.BlendMode = GetBlendMode(drawData.BlendMode);

                    texture.ReferencePoint = ReferencePoint.Center;
                    texture.Draw(drawData.X, drawData.Y, null, null, drawData.ReverseX, drawData.ReverseY);
                }
            }
        }
    }

    public bool IsPlaying()
    {
        if (Counter == null) return false;
        return Counter.Value < Counter.End && Counter.State == TimerState.Started;
    }

    private Amaoto.BlendMode GetBlendMode(int blend)
    {
        switch (blend)
        {
            case 0:
                return Amaoto.BlendMode.None;
            case 1:
                return Amaoto.BlendMode.Add;
            case 2:
                return Amaoto.BlendMode.Subtract;
            default:
                return Amaoto.BlendMode.None;
        }
    }
}

public class AnimationData
{
    public List<FrameData> nuunlm { get; set; } = new List<FrameData>();

    public List<string> TextureFileNames = new();

    public int FrameLength;
}
public class FrameData
{
    public int FrameNumber { get; set; }
    public List<DrawData> DrawObjects { get; set; }
}

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

