namespace MabaControlCenter.Models;

public class CncFrameBounds
{
    public decimal MinX { get; set; }
    public decimal MaxX { get; set; }
    public decimal MinY { get; set; }
    public decimal MaxY { get; set; }
    public bool IsValid => MaxX > MinX && MaxY > MinY;
}
