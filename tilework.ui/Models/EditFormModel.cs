namespace Tilework.Ui.Models;

public class EditFormModel
{
    public EditFormModel(BaseForm form, string? title = null)
    {
        Form = form;
        Title = title;
    }

    public BaseForm Form { get; }
    public string? Title { get; }
}
