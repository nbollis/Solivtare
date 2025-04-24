using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SolvitaireCore;

namespace SolvitaireGUI.ViewModels
{
    public class CardViewModel(Card card) : BaseViewModel
    {
        private Card _card = card;

        public Card Card
        {
            get => _card;
            set
            {
                _card = value;
                OnPropertyChanged(nameof(Card));
            }
        }
    }
}
