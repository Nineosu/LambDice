using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Data.Common;

namespace LambDice
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Player user = new Player("Игрок", ConsoleColor.Green);
            Bot bot = new Bot("Бот", ConsoleColor.Red);
            string winner;

            RenderBoards(user, bot);

            while (true)
            {
                user.MakeMove(bot);
                RenderBoards(user, bot);
                if (IsGameOver())
                {
                    Console.WriteLine($"Игра закончена. Победил {WhoWin(user, bot).PlayerName}!");
                    break;
                }

                bot.MakeMove(user);
                Thread.Sleep(1000);
                RenderBoards(user, bot);
                if (IsGameOver())
                {
                    Console.WriteLine($"Игра закончена. Победил {WhoWin(user, bot).PlayerName}!");
                    break;
                }
            }

            bool IsGameOver()
            {
                if (user.IsBoardFilled() || bot.IsBoardFilled())
                    return true;
                else
                    return false;
            }

            Player WhoWin(Player player1, Player player2)
            {
                if (player1.PlayerScore > player2.PlayerScore)
                    return player1;
                else
                    return player2;
            }
        }

        static public void RenderBoards(Player user, Bot bot)
        {
            Console.Clear();

            Console.SetCursorPosition(70, 2);
            Console.WriteLine(user.PlayerName);
            user.DrawBoardWithPosition(70, 3);

            Console.SetCursorPosition(70, 10);
            Console.WriteLine(bot.PlayerName);
            bot.DrawBoardWithPosition(70, 11);
            Console.SetCursorPosition(0, 2);
        }
    }

    class Player
    {
        protected string Name { get; }
        protected ConsoleColor BoardColor;
        protected Random random = new Random();
        protected int[,] board = new int[3, 3];
        protected int Score = 0;

        public string PlayerName => Name;
        public int PlayerScore => Score;

        public Player(string name, ConsoleColor color)
        {
            Name = name;
            BoardColor = color;
        }

        public void DrawBoardWithPosition(int offsetX, int offsetY)
        {
            Console.ForegroundColor = BoardColor;

            for (int row = 0; row < board.GetLength(0); row++)
            {
                Console.SetCursorPosition(offsetX, offsetY + row);
                for (int column = 0; column < board.GetLength(1); column++)
                {
                    Console.Write(board[row, column] + " ");
                }
            }

            Console.SetCursorPosition(offsetX, offsetY + board.GetLength(0));
            Console.ForegroundColor = ConsoleColor.Yellow;
            for (int column = 0; column < board.GetLength(1); column++)
            {
                Console.Write(CalculateColumnSum(column) + " ");
            }
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.White;
            Console.SetCursorPosition(offsetX + board.GetLength(1)*3, offsetY + board.GetLength(0));
            Console.Write(CalculateScore());
            Console.ResetColor();
            
        }

        private int CalculateColumnSum(int column)
        {
            Dictionary<int, int> numberCounts = new Dictionary<int, int>();
            int sum = 0;

            for (int row = 0; row < board.GetLength(0); row++)
            {
                int value = board[row, column];
                if (value != 0)
                {
                    if (numberCounts.ContainsKey(value))
                        numberCounts[value]++;
                    else
                        numberCounts[value] = 1;
                }
            }

            foreach (var pair in numberCounts)
            {
                int number = pair.Key;
                int count = pair.Value;

                if (count == 2)
                    sum += (number + number) * count;
                else if (count == 3)
                    sum += (number + number + number) * count;
                else
                    sum += number;
            }

            return sum;
        }

        public int CalculateScore()
        {
            int score = 0;
            
            for (int column = 0; column < board.GetLength(1); column++)
            {
                score += CalculateColumnSum(column);
            }

            return score;
        }


        public void UpdateBoard(int column, int number, Player opponent)
        {
            int row = CheckRow(column);

            if (row != -1)
            {
                board[row, column] = number;
                ResolveColumnConflicts(column, number, opponent);
            }
            else
            {
                Console.WriteLine("Колонка заполнена!");
            }
        }

        public void ResolveColumnConflicts(int column, int number, Player opponent)
        {
            for (int row = 0; row < opponent.board.GetLength(0); row++)
            {
                if (opponent.board[row, column] == number)
                {
                    opponent.board[row, column] = 0;
                }
            }
        }

        public int CheckRow(int column)
        {
            for (int row = 0; row < board.GetLength(0); row++)
            {
                if (board[row, column] == 0)
                    return row;
            }
            return -1;
        }

        public void MakeMove(Player opponent)
        {
            int diceRoll = ThrowDice();
            Console.WriteLine($"{Name} кидает кубик... Выпало число {diceRoll}!");
            Thread.Sleep(500);
            Console.WriteLine("В какую колонку его поставить?");

            int chosenColumn;
            while (true)
            {
                string input = Console.ReadLine();

                if (int.TryParse(input, out chosenColumn) && chosenColumn >= 1 && chosenColumn <= 3)
                {
                    if (CheckRow(chosenColumn - 1) != -1)
                        break;
                    else
                        Console.WriteLine("Колонка заполнена!");
                }
                else
                {
                    Console.WriteLine("Некорректный ввод! Пожалуйста, введите число от 1 до 3.");
                }
            }
            Score = CalculateScore();

            Console.WriteLine();
            UpdateBoard(chosenColumn - 1, diceRoll, opponent);
        }

        public int ThrowDice()
        {
            return random.Next(1, 7);
        }

        public bool IsBoardFilled()
        {
            for (int row = 0; row < board.GetLength(0); row++)
            {
                for (int column = 0; column < board.GetLength(1); column++)
                {
                    if (board[row, column] == 0)
                        return false;
                }
            }
            return true;
        }
    }

    class Bot : Player
    {
        public Bot(string name, ConsoleColor color) : base(name, color) { }

        public void MakeMove(Player opponent)
        {
            int diceRoll = ThrowDice();
            Console.WriteLine($"{Name} кидает кубик... Выпало число {diceRoll}!");
            Thread.Sleep(500);
            Console.WriteLine($"{Name} выбирает колонку...");
            Thread.Sleep(1000);

            int column;
            do
            {
                column = random.Next(0, 3);
            } while (CheckRow(column) == -1);

            Console.WriteLine($"{Name} сделал ход: Кубик {diceRoll} в колонку {column + 1}");
            UpdateBoard(column, diceRoll, opponent);
        }
    }
}
