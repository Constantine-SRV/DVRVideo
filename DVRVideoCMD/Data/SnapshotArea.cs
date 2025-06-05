/// <summary>
/// Описание области кадра для конкретного канала DVR-камеры.
/// </summary>
public class SnapshotArea
{
    public int Channel { get; set; }

    // Имя полного снимка (для канала 3 → ch_03.jpg)
    public string FileName => $"ch_{Channel:D2}.jpg";

    // Имя кроп-снимка (ch_03_crop.jpg)
    public string CropFileName => $"ch_{Channel:D2}_crop.jpg";

    // Координаты кроп-прямоугольника
    public int X1 { get; set; }
    public int Y1 { get; set; }
    public int X2 { get; set; }
    public int Y2 { get; set; }
}
