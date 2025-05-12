namespace SolvitaireGUI;

public class MultiSelectionViewModel<T> : BaseViewModel
{
    private bool _use;

    public bool Use
    {
        get => _use;
        set { _use = value; OnPropertyChanged(nameof(Use)); }
    }

    private T _value;

    public T Value
    {
        get => _value;
        set { _value = value; OnPropertyChanged(nameof(Value)); }
    }

    public MultiSelectionViewModel(T value, bool use = false)
    {
        Value = value;
        Use = use;
    }

    public override string ToString() => Value.ToString();
}