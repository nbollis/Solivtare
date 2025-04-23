using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SolvitaireCore;

namespace SolvitaireGuiFunctions;

public class AgentPlayingViewModel : BaseViewModel
{
    private IAgent _agent;

    public IAgent Agent
    {
        get => _agent;
        set
        {
            _agent = value;
            OnPropertyChanged(nameof(Agent));
        }
    }
}

public class AgentPlayingModel : AgentPlayingViewModel
{
    public static AgentPlayingModel Instance => new();

    AgentPlayingModel()
    {
        Agent = new RandomAgent();
    }
}