using System.Collections.ObjectModel;

namespace SolvitaireGUI;

public class ObservableEnumSelectionCollection<T> : ObservableSelectionCollection<T> where T : Enum
{
    public ObservableEnumSelectionCollection()
    {
        foreach (T type in Enum.GetValues(typeof(T)))
        {
            Add(new MultiSelectionViewModel<T>(type));
        }
    }
}

public class ObservableSelectionCollection<T> : ObservableCollection<MultiSelectionViewModel<T>>
{
    public ObservableSelectionCollection<T> Instance => new();
    public void Add(T toAdd, bool use = false)
    {
        var item = new MultiSelectionViewModel<T>(toAdd, use);
        Add(item);
    }

    public void DeselectAll()
    {
        foreach (var multiSelectionViewModel in Items)
        {
            multiSelectionViewModel.Use = false;
        }
    }

    public new T this[int index]
    {
        get => base[index].Value;
        set => base[index].Value = value;
    }
}