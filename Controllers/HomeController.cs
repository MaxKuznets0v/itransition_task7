using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Itransition7.Models;

namespace Itransition7.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public IActionResult CreateLobby(Game gameId, string name)
        {
            string sessionId = GameHub.CreateSession(gameId, name);
            return Json(sessionId);
        }
        public IActionResult Games(string sessionId = null)
        {
            if (string.IsNullOrEmpty(sessionId))
            {
                return View(GameHub.GetGames());
            }
            try
            {
                return JoinSession(GameHub.GetGameIdBySessionId(sessionId));
            }
            catch (ArgumentException)
            {
                return View(GameHub.GetGames());
            }
        }
        private IActionResult JoinSession(Game gameId)
        {
            if (gameId == Game.TicTacToe)
            {
                return View("TicTacToe");
            }
            else if (gameId == Game.SeaBattle)
            {
                return View("SeaBattle");
            }
            else
            {
                return View(GameHub.GetGames());
            }
        }
    }
}
