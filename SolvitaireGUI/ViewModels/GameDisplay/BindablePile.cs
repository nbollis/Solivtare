using System.Collections.ObjectModel;
using SolvitaireCore;

namespace SolvitaireGUI;

public class BindablePile : ObservableCollection<BindableCard>
{
    public void UpdateFromPile(Pile pile)
    {
        //this.CollectionChanged -= OnCollectionChanged;

        while (this.Count < pile.Cards.Count)
        {
            this.Add(new BindableCard(pile.Cards[this.Count]));
        }

        while (this.Count > pile.Cards.Count)
        {
            this.RemoveAt(this.Count - 1);
        }

        for (int i = 0; i < pile.Cards.Count; i++)
        {
            if (this[i].IsFaceUp != pile.Cards[i].IsFaceUp)
            {
                this[i].IsFaceUp = pile.Cards[i].IsFaceUp;
            }
        }

        //this.CollectionChanged += OnCollectionChanged;
    }

    public BindableCard? TopCard => this.Count > 0 ? this.Last() : null;
}