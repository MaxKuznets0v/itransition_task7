using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Itransition7.Models;

namespace Itransition7.ViewModels
{
    public class LobbyTable
    {
        public string SessionId { get; set; }
        public Game GameId { get; set; }
        public string SessionName { get; set; }
        public int PlayerNumber { get; set; }
    }
}
