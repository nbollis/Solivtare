using SolvitaireCore;
using System.ComponentModel;

namespace SolvitaireGUI;

public class BindableCard : INotifyPropertyChanged
{
    private readonly Card _card;
    public Suit Suit => _card.Suit;
    public Rank Rank => _card.Rank;

    public event PropertyChangedEventHandler? PropertyChanged;

    public BindableCard(Card card)
    {
        _card = card;
        if (card is ObservableCard observableCard)
        {
            observableCard.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(Card.IsFaceUp))
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsFaceUp)));
            };
        }
    }

    public bool IsFaceUp
    {
        get => _card.IsFaceUp;
        set
        {
            if (_card.IsFaceUp != value && _card is ObservableCard observable)
            {
                observable.IsFaceUp = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsFaceUp)));
            }
        }
    }
}