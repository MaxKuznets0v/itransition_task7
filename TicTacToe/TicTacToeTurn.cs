using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Itransition7.Models;
using System.Collections;

namespace Itransition7.TicTacToe
{
    public class TicTacToeTurn : Turn
    {
        private readonly char[,] board;
        private readonly List<int> data;
        public TicTacToeTurn(char[,] board, Player player, int row, int col) : base(player)
        {
            this.board = board;
            data = new List<int>() { row, col };
            if (!IsValid())
            {
                throw new ArgumentException("Wrong turn: row and col should be in range of [0, 3) and cell shouldn't be occupied");
            }
        }
        protected override bool IsValid()
        {
            return player != Player.DUMMY && data.Count == 2 && (data[0] >= 0 && data[0] < board.Length) &&
                (data[1] >= 0 && data[1] < board.Length) && board[data[0], data[1]] == ' ';
        }
        public List<int> GetData()
        {
            return data;
        }
    }
}
