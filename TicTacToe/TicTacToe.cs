using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Itransition7.Models;

namespace Itransition7.TicTacToe
{
    public class TicTacToe : TwoPlayersGameModel
    {
        private char[,] board;
        const int boardSize = 3;
        private readonly char[] moves = new char[2] { 'X', 'O' };
        private int availableCells;
        public TicTacToe()
        {
            board = new char[boardSize, boardSize];
        }
        public override void Initialize()
        {
            for (int i = 0; i < boardSize; ++i)
            {
                for (int j = 0; j < boardSize; ++j)
                {
                    board[i, j] = ' ';
                }
            }
            availableCells = boardSize * boardSize;
        }
        public override Player MakeTurn(Turn turn)
        {
            List<int> move = ((TicTacToeTurn)turn).GetData();
            board[move[0], move[1]] = moves[(int)turn.GetPlayer()];
            Player winner = Player.DUMMY;
            if (IsGameEnded(turn))
            {
                winner = turn.GetPlayer();
            }
            availableCells -= 1;

            return winner;
        }
        protected override bool IsGameEnded(Turn turn)
        {
            List<int> move = ((TicTacToeTurn)turn).GetData();
            bool res = IsWinnableRow(move[0]) || IsWinnableColumn(move[1]);
            if (move[0] == move[1])
            {
                res = res || IsWinnableDiag(true);
            }
            if (move[0] == boardSize - move[1] - 1)
            {
                res = res || IsWinnableDiag(false);
            }
            return res;
        }
        private bool IsWinnableRow(int row)
        {
            bool res = true;
            for (int i = 0; i < boardSize - 1; ++i)
            {
                res = res && (board[row, i] == board[row, i + 1]);
            }
            res = res && (board[row, 0] != ' ');
            return res;
        }
        private bool IsWinnableColumn(int col)
        {
            bool res = true;
            for (int i = 0; i < boardSize - 1; ++i)
            {
                res = res && (board[i, col] == board[i + 1, col]);
            }
            res = res && (board[0, col] != ' ');
            return res;
        }
        private bool IsWinnableDiag(bool mainDiag)
        {
            bool res = true;
            if (mainDiag)
            {
                for (int i = 0; i < boardSize - 1; ++i)
                {
                    res = res && (board[i, i] == board[i + 1, i + 1]);
                }
                res = res && (board[0, 0] != ' ');
            }
            else
            {
                for (int i = 0; i < boardSize - 1; ++i)
                {
                    res = res && (board[boardSize - i - 1, i] == board[boardSize - i - 2, i + 1]);
                }
                res = res && (board[boardSize - 1, 0] != ' ');
            }
            return res;
        }
        public TicTacToeTurn GetTurn(Player pl, int row, int col)
        {
            return new TicTacToeTurn(board, pl, row, col);
        }
        private bool IsBoardFull()
        {
            return availableCells == 0;
        }
        public override bool IsGameOver()
        {
            return IsBoardFull();
        }
    }
}
