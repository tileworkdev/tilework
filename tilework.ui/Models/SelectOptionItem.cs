namespace Tilework.Ui.Models;

public class SelectOptionItem
{
    public string Text { get; set; }
    public object Value { get; set; }
    public override string ToString() => Text;
}