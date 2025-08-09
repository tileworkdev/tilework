namespace Tilework.Ui.Interfaces;

public interface IListViewModel<T>
{
    public Task Initialize();
    public List<T> Items { get; }
}