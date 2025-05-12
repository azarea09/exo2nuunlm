using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using DxLibDLL;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Taiko;

namespace Amaoto
{
    /// <summary>
    /// テクスチャ。
    /// </summary>
    public class Texture : IDisposable, ITextureReturnable
    {
        public class FontCathers
        {
            public Texture Texture;

            public Color Color;
        }

        public FontCathers FONT = new FontCathers();

        public static int TotalTexture;

        public Color Color = Color.Empty;

        public double MaxOpacity = 255.0;

        public double BaseScale = 1.0;

        private int textureSizeX;

        private int textureSizeY;

        public bool IsEnable { get; set; }

        public BlendMode BlendMode { get; set; }

        public string FileName { get; private set; }

        public double Opacity { get; set; }

        public int ID { get; private set; }

        public double Rotation { get; set; }

        public ReferencePoint ReferencePoint { get; set; }

        public DrawState DrawMode { get; set; }

        public double ScaleX { get; set; }

        public double ScaleY { get; set; }

        public Size TextureSize => new Size(textureSizeX, textureSizeY);

        public Size ActualSize
        {
            get
            {
                Size s = TextureSize;
                return new Size((int)(ScaleX * (double)s.Width), (int)(ScaleY * (double)s.Height));
            }
        }

        public Texture()
        {
            Rotation = 0.0;
            ScaleX = 1.0;
            ScaleY = 1.0;
            Opacity = 255.0;
            ReferencePoint = ReferencePoint.TopLeft;
            DrawMode = DrawState.None;
        }

        public void setGraphSize()
        {
            DX.GetGraphSize(ID, out var width, out var height);
            textureSizeX = width;
            textureSizeY = height;
        }

        public Texture(string fileName)
            : this()
        {
            string ex = Path.GetExtension(fileName);
            string path = fileName;
            if (ex != "")
            {
                string ddsPath = fileName.Replace(ex, ".dds");
                string pngPath = fileName.Replace(ex, ".png");
                path = (File.Exists(ddsPath) ? ddsPath : (File.Exists(pngPath) ? pngPath : fileName));
            }
            if (File.Exists(path))
            {
                ID = DX.LoadGraph(path, 1);
                if (ID != -1)
                {
                    setGraphSize();
                    TotalTexture++;
                    IsEnable = true;
                }
                FileName = fileName;
            }
        }

        public Texture(int handle)
            : this()
        {
            ID = handle;
            if (ID != -1)
            {
                setGraphSize();
                IsEnable = true;
            }
            FileName = null;
        }

        ~Texture()
        {
            if (IsEnable)
            {
                Dispose();
            }
        }

        public void Dispose()
        {
            FONT?.Texture?.Dispose();
            DX.DeleteGraph(ID);
            IsEnable = false;
            TotalTexture--;
        }

        public void ScaleReset(double value = 1.0)
        {
            double scaleX = (ScaleY = value);
            ScaleX = scaleX;
        }

        public void Reset()
        {
            ReferencePoint = ReferencePoint.TopLeft;
            double scaleX = (ScaleY = 1.0);
            ScaleX = scaleX;
            Opacity = 255.0;
            BlendMode = BlendMode.None;
            Rotation = 0.0;
        }

        public void BaseScaleset(double value = 1.0)
        {
            BaseScale = value;
        }

        public PointD CalcReference(double sizeX, double sizeY)
        {
            PointD origin = new PointD();
            switch (ReferencePoint)
            {
                case ReferencePoint.TopLeft:
                    origin.X = 0.0;
                    origin.Y = 0.0;
                    break;
                case ReferencePoint.TopCenter:
                    origin.X = sizeX / 2.0;
                    origin.Y = 0.0;
                    break;
                case ReferencePoint.TopRight:
                    origin.X = sizeX;
                    origin.Y = 0.0;
                    break;
                case ReferencePoint.CenterLeft:
                    origin.X = 0.0;
                    origin.Y = sizeY / 2.0;
                    break;
                case ReferencePoint.Center:
                    origin.X = sizeX / 2.0;
                    origin.Y = sizeY / 2.0;
                    break;
                case ReferencePoint.CenterRight:
                    origin.X = sizeX;
                    origin.Y = sizeY / 2.0;
                    break;
                case ReferencePoint.BottomLeft:
                    origin.X = 0.0;
                    origin.Y = sizeY;
                    break;
                case ReferencePoint.BottomCenter:
                    origin.X = sizeX / 2.0;
                    origin.Y = sizeY;
                    break;
                case ReferencePoint.BottomRight:
                    origin.X = sizeX;
                    origin.Y = sizeY;
                    break;
                default:
                    origin.X = 0.0;
                    origin.Y = 0.0;
                    break;
            }
            return origin;
        }

        public void Draw(double x = 0.0, double y = 0.0, Rectangle? rectangle = null, Point? drawOrigin = null, bool reverseX = false, bool reverseY = false)
        {
            if (!IsEnable)
            {
                return;
            }
            if (BaseScale <= 0.0)
            {
                BaseScale = 1.0;
            }
            if (textureSizeX == 0 && textureSizeY == 0)
            {
                setGraphSize();
            }
            PointD origin = new PointD();
            bool isDefinedRect = rectangle.HasValue;
            if (!rectangle.HasValue)
            {
                rectangle = new Rectangle(0, 0, textureSizeX, textureSizeY);
            }
            origin = CalcReference(rectangle.Value.Width, rectangle.Value.Height);
            if (drawOrigin.HasValue)
            {
                origin = new PointD(drawOrigin.Value);
            }
            int blendParam = (int)Opacity;
            if ((double)blendParam >= MaxOpacity)
            {
                blendParam = (int)MaxOpacity;
            }
            DX.SetDrawBlendMode(DXLibUtil.GetBlendModeConstant(BlendMode), blendParam);
            if (DrawMode == DrawState.None)
            {
                if (ScaleX != 1.0 || ScaleY != 1.0)
                {
                    DX.SetDrawMode(0);
                }
                else
                {
                    DX.SetDrawMode(1);
                }
            }
            else if (DrawMode == DrawState.Bliner)
            {
                DX.SetDrawMode(1);
            }
            else if (DrawMode == DrawState.Nearest)
            {
                DX.SetDrawMode(0);
            }
            else if (DrawMode == DrawState.Dot)
            {
                DX.SetDrawMode(3);
            }
            if (!Color.IsEmpty)
            {
                DX.SetDrawBright(Color.R, Color.G, Color.B);
            }
            if (!isDefinedRect)
            {
                if (ScaleX == 1.0 && ScaleY == 1.0)
                {
                    DX.DrawRotaGraph2F((float)x, (float)y, (float)origin.X, (float)origin.Y, 1.0 * BaseScale, Rotation, ID, 1, reverseX ? 1 : 0, reverseY ? 1 : 0);
                }
                else
                {
                    DX.DrawRotaGraph3F((float)x, (float)y, (float)origin.X, (float)origin.Y, ScaleX * BaseScale, ScaleY * BaseScale, Rotation, ID, 1, reverseX ? 1 : 0, reverseY ? 1 : 0);
                }
            }
            else if (ScaleX == 1.0 && ScaleY == 1.0)
            {
                DX.DrawRectRotaGraph2F((float)x, (float)y, rectangle.Value.X, rectangle.Value.Y, rectangle.Value.Width, rectangle.Value.Height, (float)origin.X, (float)origin.Y, 1.0 * BaseScale, Rotation, ID, 1, reverseX ? 1 : 0, reverseY ? 1 : 0);
            }
            else
            {
                DX.DrawRectRotaGraph3F((float)x, (float)y, rectangle.Value.X, rectangle.Value.Y, rectangle.Value.Width, rectangle.Value.Height, (float)origin.X, (float)origin.Y, ScaleX * BaseScale, ScaleY * BaseScale, Rotation, ID, 1, reverseX ? 1 : 0, reverseY ? 1 : 0);
            }
            DX.SetDrawBlendMode(17, 255);
            DX.SetDrawBright(255, 255, 255);
        }

        public void DrawExntendRate(double x = 0.0, double y = 0.0, Rectangle? rectangle = null, Point? drawOrigin = null, bool reverseX = false, bool reverseY = false, double rate = 1.5)
        {
            if (BaseScale <= 0.0)
            {
                BaseScale = 1.0;
            }
            PointD origin = new PointD();
            bool isDefinedRect = rectangle.HasValue;
            if (!rectangle.HasValue)
            {
                rectangle = new Rectangle(0, 0, textureSizeX, textureSizeY);
            }
            switch (ReferencePoint)
            {
                case ReferencePoint.TopLeft:
                    origin.X = 0.0;
                    origin.Y = 0.0;
                    break;
                case ReferencePoint.TopCenter:
                    origin.X = rectangle.Value.Width / 2;
                    origin.Y = 0.0;
                    break;
                case ReferencePoint.TopRight:
                    origin.X = rectangle.Value.Width;
                    origin.Y = 0.0;
                    break;
                case ReferencePoint.CenterLeft:
                    origin.X = 0.0;
                    origin.Y = rectangle.Value.Height / 2;
                    break;
                case ReferencePoint.Center:
                    origin.X = rectangle.Value.Width / 2;
                    origin.Y = rectangle.Value.Height / 2;
                    break;
                case ReferencePoint.CenterRight:
                    origin.X = rectangle.Value.Width;
                    origin.Y = rectangle.Value.Height / 2;
                    break;
                case ReferencePoint.BottomLeft:
                    origin.X = 0.0;
                    origin.Y = rectangle.Value.Height;
                    break;
                case ReferencePoint.BottomCenter:
                    origin.X = rectangle.Value.Width / 2;
                    origin.Y = rectangle.Value.Height;
                    break;
                case ReferencePoint.BottomRight:
                    origin.X = rectangle.Value.Width;
                    origin.Y = rectangle.Value.Height;
                    break;
                default:
                    origin.X = 0.0;
                    origin.Y = 0.0;
                    break;
            }
            x *= rate;
            y *= rate;
            if (drawOrigin.HasValue)
            {
                origin = new PointD(drawOrigin.Value);
            }
            int blendParam = (int)Opacity;
            if ((double)blendParam >= MaxOpacity)
            {
                blendParam = (int)MaxOpacity;
            }
            DX.SetDrawBlendMode(DXLibUtil.GetBlendModeConstant(BlendMode), blendParam);
            if (DrawMode == DrawState.None)
            {
                if (ScaleX != 1.0 || ScaleY != 1.0)
                {
                    DX.SetDrawMode(0);
                }
                else
                {
                    DX.SetDrawMode(1);
                }
            }
            else if (DrawMode == DrawState.Bliner)
            {
                DX.SetDrawMode(1);
            }
            else if (DrawMode == DrawState.Nearest)
            {
                DX.SetDrawMode(0);
            }
            else if (DrawMode == DrawState.Dot)
            {
                DX.SetDrawMode(3);
            }
            if (!Color.IsEmpty)
            {
                DX.SetDrawBright(Color.R, Color.G, Color.B);
            }
            if (!isDefinedRect)
            {
                if (ScaleX == 1.0 && ScaleY == 1.0)
                {
                    DX.DrawRotaGraph2F((float)x, (float)y, (float)origin.X, (float)origin.Y, 1.0 * BaseScale, Rotation, ID, 1, reverseX ? 1 : 0, reverseY ? 1 : 0);
                }
                else
                {
                    DX.DrawRotaGraph3F((float)x, (float)y, (float)origin.X, (float)origin.Y, ScaleX * BaseScale, ScaleY * BaseScale, Rotation, ID, 1, reverseX ? 1 : 0, reverseY ? 1 : 0);
                }
            }
            else if (ScaleX == 1.0 && ScaleY == 1.0)
            {
                DX.DrawRectRotaGraph2F((float)x, (float)y, rectangle.Value.X, rectangle.Value.Y, rectangle.Value.Width, rectangle.Value.Height, (float)origin.X, (float)origin.Y, 1.0 * BaseScale, Rotation, ID, 1, reverseX ? 1 : 0, reverseY ? 1 : 0);
            }
            else
            {
                DX.DrawRectRotaGraph3F((float)x, (float)y, rectangle.Value.X, rectangle.Value.Y, rectangle.Value.Width, rectangle.Value.Height, (float)origin.X, (float)origin.Y, ScaleX * BaseScale, ScaleY * BaseScale, Rotation, ID, 1, reverseX ? 1 : 0, reverseY ? 1 : 0);
            }
            DX.SetDrawBlendMode(17, 255);
            DX.SetDrawBright(255, 255, 255);
        }

        public void SaveAsPNG(string path)
        {
            DX.SaveDrawValidGraphToPNG(ID, 0, 0, TextureSize.Width, TextureSize.Height, path, 0);
        }

        public Texture GetTexture()
        {
            return this;
        }
    }

    /// <summary>
    /// 描画補完の状態。
    /// </summary>
    public enum DrawState
    {
        None,
        Bliner,
        Nearest,
        Dot
    }

    /// <summary>
    /// 合成モード。
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum BlendMode
    {
        /// <summary>
        /// なし
        /// </summary>
        None,

        /// <summary>
        /// 加算合成
        /// </summary>
        Add,

        /// <summary>
        /// 減算合成
        /// </summary>
        Subtract
    }

    /// <summary>
    /// 描画基準点。
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ReferencePoint
    {
        /// <summary>
        /// 左上
        /// </summary>
        TopLeft,

        /// <summary>
        /// 中央上
        /// </summary>
        TopCenter,

        /// <summary>
        /// 右上
        /// </summary>
        TopRight,

        /// <summary>
        /// 左中央
        /// </summary>
        CenterLeft,

        /// <summary>
        /// 中央
        /// </summary>
        Center,

        /// <summary>
        /// 右中央
        /// </summary>
        CenterRight,

        /// <summary>
        /// 左下
        /// </summary>
        BottomLeft,

        /// <summary>
        /// 中央下
        /// </summary>
        BottomCenter,

        /// <summary>
        /// 右下
        /// </summary>
        BottomRight
    }
}