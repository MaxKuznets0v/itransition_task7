using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Itransition7.Models;

namespace Itransition7.SeaBattle
{
    public class SeaBattleTurn : Turn
    {
        private readonly Cell[][] board;
        private readonly List<int> data;
        public SeaBattleTurn(Cell[][] board, Player player, int row, int col) : base(player)
        {
            this.board = board;
            data = new List<int>() { row, col };
            if (!IsValid())
            {
                throw new ArgumentException($"Cell is used or invalid range (should be in range [0;{board.Length})");
            }
        }
        protected override bool IsValid()
        {
            return player != Player.DUMMY && data.Count == 2 && (data[0] >= 0 && data[0] < board.Length) &&
                (data[1] >= 0 && data[1] < board.Length) && (board[data[0]][data[1]] == Cell.EMPTY || board[data[0]][data[1]] == Cell.SHIP);
        }
        public List<int> GetData()
        {
            return data;
        }
    }
}
