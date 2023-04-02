using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Itransition7.Models
{
    public enum Game
    {
        TicTacToe,
        SeaBattle
    }
    public class GameSession
    {
        public Game GameId { get; set; }
        public string Name { get; set; }
        public List<string> PlayerIds { get; set; }
        public TwoPlayersGameModel Game { get; set; }
    }
}
