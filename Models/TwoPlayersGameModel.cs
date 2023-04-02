using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Itransition7.Models
{
    public enum Player 
    { 
        FIRST,
        SECOND,
        DUMMY
    }

    public abstract class TwoPlayersGameModel
    {
        abstract public void Initialize();
        abstract public Player MakeTurn(Turn turn);
        abstract protected bool IsGameEnded(Turn turn);
        abstract public bool IsGameOver();
    }
}
