using SolvitaireCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            if (_card is ObservableCard observable)
                observable.IsFaceUp = value;
        }
    }
}