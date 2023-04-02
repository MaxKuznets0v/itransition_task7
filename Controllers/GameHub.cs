using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using System.Collections.Concurrent;
using Itransition7.Models;
using Microsoft.AspNetCore.Http;
using Itransition7.ViewModels;
using Newtonsoft.Json;

namespace Itransition7.Controllers
{
    public class GameHub : Hub
    {
        private static readonly ConcurrentDictionary<string, GameSession> gameSessions = new ConcurrentDictionary<string, GameSession>();
        private static readonly ConcurrentDictionary<string, string> players = new ConcurrentDictionary<string, string>();
        private static readonly ConcurrentDictionary<string, string> playerIdToSesssion = new ConcurrentDictionary<string, string>();
        private async Task<bool> IsValidSession(string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId) || !gameSessions.ContainsKey(sessionId))
            {
                await Clients.Caller.SendAsync("SessionNotFound");
                return false;
            }
            return true;
        }
        public override Task OnConnectedAsync()
        {
            string gameSessionId = Context.GetHttpContext().Request.Query["sessionId"];
            string userName = Context.GetHttpContext().Request.Query["userName"];
            if (IsValidSession(gameSessionId).Result)
            {
                string userId = Context.ConnectionId;
                GameSession session = gameSessions[gameSessionId];
                if (session.PlayerIds.Count == 2)
                {
                    return Clients.Caller.SendAsync("FullSession");
                }
                List<Dictionary<string, string>> opponents = new List<Dictionary<string, string>>();
                foreach (string user in session.PlayerIds)
                {

                    opponents.Add(new Dictionary<string, string>()
                    {
                        { "userId", user },
                        { "userName", players[user] }
                    });
                    Clients.Client(user).SendAsync("UserConnected", new Dictionary<string, string>
                    {
                        { "userId",  userId },
                        { "userName", userName }
                    });
                }
                Clients.Caller.SendAsync("UserList", opponents);
                playerIdToSesssion[userId] = gameSessionId;
                session.PlayerIds.Add(userId);
                players[userId] = userName;
            }
            return base.OnConnectedAsync();
        }
        public override Task OnDisconnectedAsync(Exception exception)
        {
            string userId = Context.ConnectionId;
            playerIdToSesssion.TryRemove(userId, out string sessionId);
            players.TryRemove(userId, out _);
            if (IsValidSession(sessionId).Result)
            {
                GameSession session = gameSessions[sessionId];
                session.PlayerIds.Remove(userId);
                foreach (string user in session.PlayerIds)
                {
                    Clients.Client(user).SendAsync("UserLeft", userId);
                }
                if (session.PlayerIds.Count == 0)
                {
                    gameSessions.TryRemove(sessionId, out _);
                }
            }
            return base.OnDisconnectedAsync(exception);
        }
        static public string CreateSession(Game gameId, string name)
        {
            GameSession session = new GameSession()
            {
                GameId = gameId,
                Name = name,
                PlayerIds = new List<string>(),
            };
            string id = Guid.NewGuid().ToString();
            gameSessions[id] = session;
            return id;
        }
        static public List<LobbyTable> GetGames()
        {
            List<LobbyTable> games = new List<LobbyTable>();
            foreach (var game in gameSessions)
            {
                LobbyTable session = new LobbyTable
                {
                    SessionId = game.Key,
                    SessionName = game.Value.Name,
                    GameId = game.Value.GameId,
                    PlayerNumber = game.Value.PlayerIds.Count
                };
                games.Add(session);
            }
            return games;
        }
        public async Task StartGame()
        {
            string sessionId = Context.GetHttpContext().Request.Query["sessionId"];
            if (IsValidSession(sessionId).Result)
            {
                GameSession session = gameSessions[sessionId];
                session.Game = GetGame(session.GameId);
                session.Game.Initialize();
                string startTurn = session.PlayerIds[new Random().Next(session.PlayerIds.Count)];
                foreach (var user in session.PlayerIds)
                {
                    await Clients.Client(user).SendAsync("GameStarted", startTurn);
                }
            }
        }
        public async Task ConfigureGame(Dictionary<string, object> config)
        {
            string sessionId = Context.GetHttpContext().Request.Query["sessionId"];
            if (IsValidSession(sessionId).Result)
            {
                GameSession session = gameSessions[sessionId];
                try
                {
                    SetGameParameters(session, Context.ConnectionId, config);
                }
                catch (ArgumentException e)
                {
                    await Clients.Caller.SendAsync("WrongMove", e.Message);
                }
                catch (NullReferenceException e)
                {
                    await Clients.Caller.SendAsync("GameNotFound", e.Message);
                }
                foreach (var user in session.PlayerIds)
                {
                    await Clients.Client(user).SendAsync("GameConfigured");
                }
            }
        }
        private void SetGameParameters(GameSession session, string userId, Dictionary<string, object> config)
        {
            Game gameId = session.GameId;
            TwoPlayersGameModel game = session.Game;
            if (game == null)
            {
                throw new NullReferenceException("Game is not started");
            }
            int playerIndex = session.PlayerIds.IndexOf(userId);
            if (gameId == Game.SeaBattle)
            {
                config.TryGetValue("ships", out object ships);
                List<List<List<int>>> playerShips = ParseShipsJson(ships);
                if (ships == null || playerIndex == -1)
                {
                    throw new ArgumentException("List of ships is not provided or user doesn't exist");
                }
                Player player = (Player)playerIndex;
                ((SeaBattle.SeaBattle)game).SetShips(playerShips, player == Player.FIRST);
            }
            else
            {
                throw new ArgumentException("This game ID is not available for configuration (or doesn't exist): " + ((int)gameId).ToString());
            }
        }
        public async Task MakeMove(Dictionary<string, object> move)
        {
            string sessionId = Context.GetHttpContext().Request.Query["sessionId"];
            if (IsValidSession(sessionId).Result)
            {
                string userId = Context.ConnectionId;
                GameSession session = gameSessions[sessionId];
                try
                {
                    Turn turn = GetTurn(session, userId, move);
                    Player winner = session.Game.MakeTurn(turn);
                    bool gameEnded = true;
                    if (winner == Player.DUMMY && !session.Game.IsGameOver())
                    {
                        gameEnded = false;
                    }
                    foreach (var user in session.PlayerIds)
                    {
                        await Clients.Client(user).SendAsync("MoveResult", (int)winner, gameEnded, GetMoveResultObject(session, turn, move));
                    }
                    if (gameEnded)
                    {
                        session.Game = null;
                    }
                }
                catch (ArgumentException e)
                {
                    await Clients.Caller.SendAsync("WrongMove", e.Message);
                }
                catch (NullReferenceException e)
                {
                    await Clients.Caller.SendAsync("GameNotFound", e.Message);
                }
            }
        }
        private Dictionary<string, object> GetMoveResultObject(GameSession session, Turn turn, Dictionary<string, object> move)
        {
            if (session.GameId == Game.SeaBattle)
            {
                move["hit"] = ((SeaBattle.SeaBattle)session.Game).IsHit(turn);
                move["player"] = Context.ConnectionId;
            }
            return move;
        }
        private static TwoPlayersGameModel GetGame(Game gameId)
        {
            if (gameId == Game.TicTacToe)
            {
                return new TicTacToe.TicTacToe();
            }
            else if (gameId == Game.SeaBattle)
            {
                return new SeaBattle.SeaBattle();
            }
            else
            {
                throw new ArgumentException("This game ID doesn't exist: " + ((int)gameId).ToString());
            }
        }
        private Turn GetTurn(GameSession session, string userId, Dictionary<string, object> move)
        {
            Game gameId = session.GameId;
            TwoPlayersGameModel game = session.Game;
            if (game == null)
            {
                throw new NullReferenceException("Game is not started");
            }
            int playerIndex = session.PlayerIds.IndexOf(userId);
            if (playerIndex == -1)
            {
                throw new ArgumentException("Player doesn't exist in this session");
            }
            Player player = (Player)playerIndex;
            if (gameId == Game.TicTacToe)
            {
                List<int> coord = ParseCoord(move);
                TicTacToe.TicTacToeTurn turn = ((TicTacToe.TicTacToe)game).GetTurn(player, coord[0], coord[1]);
                return turn;
            }
            else if (gameId == Game.SeaBattle)
            {
                List<int> coord = ParseCoord(move);
                SeaBattle.SeaBattleTurn turn = ((SeaBattle.SeaBattle)game).GetTurn(player, coord[0], coord[1]);
                return turn;
            }
            else
            {
                throw new ArgumentException("This game ID doesn't exist: " + ((int)gameId).ToString());
            }
        }
        private List<int> ParseCoord(Dictionary<string, object> move)
        {
            move.TryGetValue("row", out object row);
            move.TryGetValue("col", out object col);
            if (row == null || col == null || !int.TryParse(row.ToString(), out int rowNum) || !int.TryParse(col.ToString(), out int colNum))
            {
                throw new ArgumentException("Row/column value not found");
            }
            return new List<int> { rowNum, colNum };
        }
        static public Game GetGameIdBySessionId(string sessionId)
        {
            if (!gameSessions.ContainsKey(sessionId))
            {
                throw new ArgumentException("Session not found");
            }
            return gameSessions[sessionId].GameId;
        }
        private List<List<List<int>>> ParseShipsJson(object ships)
        {
            return JsonConvert.DeserializeObject<List<List<List<int>>>>(ships.ToString());
        }
    }
}