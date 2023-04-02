using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Itransition7.Models;

namespace Itransition7.SeaBattle
{
    public enum Cell
    {
        EMPTY,
        SHIP,
        HIT,
        MISS
    }
    public class SeaBattle : TwoPlayersGameModel
    {
        private readonly Cell[][] fieldP1;
        private readonly Cell[][] fieldP2;
        private const int boardSize = 10;
        private int cellsAliveP1;
        private int cellsAliveP2;
        public SeaBattle()
        {
            fieldP1 = InitField();
            fieldP2 = InitField();
            cellsAliveP1 = 0;
            cellsAliveP2 = 0;
        }
        public override void Initialize()
        {
            MakeDefaultField(fieldP1);
            MakeDefaultField(fieldP2);
        }
        public override Player MakeTurn(Turn turn)
        {
            List<int> move = ((SeaBattleTurn)turn).GetData();
            Cell[][] curField;
            if (turn.GetPlayer() == Player.FIRST)
            {
                curField = fieldP2;
            }
            else
            {
                curField = fieldP1;
            }
            if (curField[move[0]][move[1]] == Cell.EMPTY)
            {
                curField[move[0]][move[1]] = Cell.MISS;
            }
            else
            {
                curField[move[0]][move[1]] = Cell.HIT;
                if (turn.GetPlayer() == Player.FIRST)
                {
                    cellsAliveP2 -= 1;
                }
                else
                {
                    cellsAliveP1 -= 1;
                }
            }
            Player winner = Player.DUMMY;
            if (IsGameEnded(turn))
            {
                winner = turn.GetPlayer();
            }

            return winner;
        }
        public bool IsHit(Turn turn)
        {
            List<int> move = ((SeaBattleTurn)turn).GetData();
            Cell res;
            if (turn.GetPlayer() == Player.FIRST)
            {
                res = fieldP2[move[0]][move[1]];
            }
            else
            {
                res = fieldP1[move[0]][move[1]];
            }
            return res == Cell.HIT;
        }
        protected override bool IsGameEnded(Turn turn)
        {
            return IsGameOver();
        }
        public override bool IsGameOver()
        {
            return cellsAliveP1 == 0 || cellsAliveP2 == 0;
        }
        public void SetShips(List<List<List<int>>> playerShips, bool setP1)
        {
            if (setP1)
            {
                cellsAliveP1 = 0;
                MakeDefaultField(fieldP1);
                cellsAliveP1 = TryPlace(playerShips, fieldP1);
            } else
            {
                cellsAliveP2 = 0;
                MakeDefaultField(fieldP2);
                cellsAliveP2 = TryPlace(playerShips, fieldP2);
            }
        }
        private int TryPlace(List<List<List<int>>> ships, Cell[][] field)
        {
            int res = 0;
            foreach (var ship in ships)
            {
                if (ship.Count != 2 || ships[0].Count != 2 || ships[1].Count != 2)
                {
                    throw new ArgumentException("There should be a list of ships. " +
                        "Each ship presented as two points: coordinate of start and end");
                }
                List<int> point1 = ship[0];
                List<int> point2 = ship[1];
                if (!IsPointRangeValid(point1) || !IsPointRangeValid(point2))
                {
                    throw new ArgumentOutOfRangeException($"Point coordinates should be in range [0;{boardSize})!");
                }
                if (point1[0] == point2[0])
                {
                    int from = Math.Min(point1[1], point2[1]);
                    int to = Math.Max(point1[1], point2[1]);
                    PlaceRowShip(point1[0], from, to, field);
                    res += to - from + 1;
                }
                else if (point1[1] == point2[1])
                {
                    int from = Math.Min(point1[0], point2[0]);
                    int to = Math.Max(point1[0], point2[0]);
                    PlaceColShip(point1[1], from, to, field);
                    res += to - from + 1;
                }
                else
                {
                    throw new ArgumentException("You can't place a ship diagonally!");
                }
            }
            return res;
        }
        private void PlaceRowShip(int row, int from, int to, Cell[][] field)
        {
            for (int i = from; i <= to; ++i)
            {
                CheckForCollision(row, i, field);
            }
            for (int i = from; i <= to; ++i)
            {
                field[row][i] = Cell.SHIP;
            }
        }
        private void PlaceColShip(int col, int from, int to, Cell[][] field)
        {
            for (int i = from; i <= to; ++i)
            {
                CheckForCollision(i, col, field);
            }
            for (int i = from; i <= to; ++i)
            {
                field[i][col] = Cell.SHIP;
            }
        }
        private void CheckForCollision(int row, int col, Cell[][] field)
        {
            if (field[row][col] != Cell.EMPTY ||
                row < boardSize - 1 && field[row + 1][col] != Cell.EMPTY ||
                row > 0 && field[row - 1][col] != Cell.EMPTY ||
                col < boardSize - 1 && field[row][col + 1] != Cell.EMPTY ||
                col > 0 && field[row][col - 1] != Cell.EMPTY ||
                row < boardSize - 1 && col < boardSize - 1 && field[row + 1][col + 1] != Cell.EMPTY ||
                row < boardSize - 1 && col > 0 && field[row + 1][col - 1] != Cell.EMPTY ||
                row > 0 && col < boardSize - 1 && field[row - 1][col + 1] != Cell.EMPTY ||
                row > 0 && col > 0 && field[row - 1][col - 1] != Cell.EMPTY)
            {
                throw new ArgumentException("Can't place ship due to collision!");
            }
        }
        private bool IsPointRangeValid(List<int> p)
        {
            bool valid = true;
            if (p[0] < 0 || p[0] >= boardSize || p[1] < 0 || p[1] >= boardSize)
            {
                valid = false;
            }
            return valid;
        }
        private void MakeDefaultField(Cell[][] field)
        {
            foreach (var row in field)
            {
                for (int i = 0; i < field.Length; ++i)
                {
                    row[i] = Cell.EMPTY;
                }
            }
        }
        private Cell[][] InitField()
        {
            Cell[][] field = new Cell[boardSize][];
            for (int i = 0; i < boardSize; ++i)
            {
                field[i] = new Cell[boardSize];
            }
            return field;
        }
        public SeaBattleTurn GetTurn(Player pl, int row, int col)
        {
            if (pl == Player.FIRST)
            {
                return new SeaBattleTurn(fieldP2, pl, row, col);
            }
            else
            {
                return new SeaBattleTurn(fieldP1, pl, row, col);
            }
        }
    }
}