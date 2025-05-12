namespace Amaoto;

public class PointD
{
    public double X;

    public double Y;

    private Point value;

    public PointD()
    {
    }

    public PointD(Point value)
    {
        this.value = value;
        X = value.X;
        Y = value.Y;
    }
}
