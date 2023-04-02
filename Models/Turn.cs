using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Itransition7.Models
{
    public abstract class Turn
    {
        protected readonly Player player;
        public Turn(Player player)
        {
            this.player = player;
        }
        public Player GetPlayer()
        {
            return player;
        }
        protected abstract bool IsValid();
    }
}
