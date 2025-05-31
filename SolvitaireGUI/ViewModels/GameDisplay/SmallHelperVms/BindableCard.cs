using SolvitaireCore;
using System.ComponentModel;

namespace SolvitaireGUI;

public class BindableCard : Card, INotifyPropertyChanged
{
    private readonly Card _card;

    public event PropertyChangedEventHandler? PropertyChanged;

    public BindableCard(Card card) : base(card.Suit, card.Rank, card.IsFaceUp)
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

    public override bool IsFaceUp
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