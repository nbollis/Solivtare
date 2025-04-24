using System.Collections.ObjectModel;
using SolvitaireCore;

namespace SolvitaireGUI;

public class BindablePile : ObservableCollection<BindableCard>
{
    public void UpdateFromPile(Pile pile)
    {
        this.Clear();
        foreach (var card in pile.Cards)
        {
            this.Add(new BindableCard(card));
        }
    }

    public BindableCard? TopCard => this.Count > 0 ? this.Last() : null;
}