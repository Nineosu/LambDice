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
            int input;

            while (true)
            {
                Console.WriteLine("Играть против бота: 1\nИграть вдвоём: 2\nПоказать правила игры: 3\n");

                do
                {
                    Console.WriteLine("[Выберите пункт]");
                } while (int.TryParse(Console.ReadLine(), out input) && input < 1 || input > 3 );
                
                switch (input)
                {
                    case 1:
                        GameWithBot();
                        break;
                    case 2:
                        GameWithUser();
                        break;
                    case 3:
                        Console.Clear();
                        Console.WriteLine("Играть против бота: 1\nИграть вдвоём: 2\nПоказать правила игры: 3\n");
                        Console.WriteLine("\nЕсть два игрока\r\n" +
                            "У них имеется доска 3х3\r\n" +
                            "Каждый из них бросает кубик, и должен решить в какую колонку разместить выпавшее число\r\n" +
                            "Все числа в колонке складываются\r\n" +
                            "Если разместить два и более одинаковых чисел в одной колонке, они складываются и перемножаются на количество таких чисел\r\n" +
                            "Если на своем ходу оппонент разместит эквивалентное число в ту же колонку, что и у игрока, все числа игрока, равные эквивалентному числу, обнулятся\r\n" +
                            "Игра закончена когда первая доска будет заполнена\r\n" +
                            "Все очки суммируются с трёх колонок\r\n" +
                            "Победитель тот, кто набрал больше очков");
                        break;
                }
            }

            void GameWithBot()
            {
                Player user = new Player("Игрок", ConsoleColor.Green);
                Bot bot = new Bot("Бот", ConsoleColor.Red);
                RenderBoards(user, bot);

                while (true)
                {
                    user.MakeMove(bot);
                    RenderBoards(user, bot);
                    if (IsGameOver(user, bot))
                    {
                        Console.Write($"Игра закончена. Победил ");
                        Console.ForegroundColor = WhoWin(user, bot).PlayerColor;
                        Console.Write(WhoWin(user, bot).PlayerName + "!\n");
                        Console.ResetColor();
                        break;
                    }

                    bot.MakeMove(user);
                    Thread.Sleep(500);
                    RenderBoards(user, bot);
                    if (IsGameOver(user, bot))
                    {
                        Console.Write($"Игра закончена. Победил ");
                        Console.ForegroundColor = WhoWin(user, bot).PlayerColor;
                        Console.Write(WhoWin(user, bot).PlayerName + "!\n");
                        Console.ResetColor();
                        break;
                    }
                }
            }

            void GameWithUser()
            {
                Player user = new Player("Игрок", ConsoleColor.Green);
                Player user2 = new Player("Игрок 2", ConsoleColor.Blue);
                string winner;
                
                RenderBoards(user, user2);

                while (true)
                {
                    user.MakeMove(user2);
                    RenderBoards(user, user2);
                    if (IsGameOver(user, user2))
                    {
                        Console.Write($"Игра закончена. Победил ");
                        Console.ForegroundColor = WhoWin(user, user2).PlayerColor;
                        Console.Write(WhoWin(user, user2).PlayerName + "!\n");
                        Console.ResetColor();
                        break;
                    }

                    user2.MakeMove(user);
                    RenderBoards(user, user2);
                    if (IsGameOver(user, user2))
                    {
                        Console.Write($"Игра закончена. Победил ");
                        Console.ForegroundColor = WhoWin(user, user2).PlayerColor;
                        Console.Write(WhoWin(user, user2).PlayerName + "!\n");
                        Console.ResetColor();
                        break;
                    }
                }
            }

            bool IsGameOver(Player player1, Player player2)
            {
                if (player1.IsBoardFilled() || player2.IsBoardFilled())
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

        static public void RenderBoards(Player user, Player user2)
        {
            Console.Clear();

            Console.SetCursorPosition(70, 2);
            Console.WriteLine(user.PlayerName);
            user.DrawBoardWithPosition(70, 4);

            Console.SetCursorPosition(70, 10);
            Console.WriteLine(user2.PlayerName);
            user2.DrawBoardWithPosition(70, 12);
            Console.SetCursorPosition(0, 0);
        }
    }

    class Player
    {
        protected string Name { get; }
        protected ConsoleColor BoardColor;
        protected static Random random = new Random();
        protected int[,] board = new int[3, 3];
        protected int Score = 0;

        public string PlayerName => Name;
        public int PlayerScore => Score;
        public ConsoleColor PlayerColor => BoardColor;

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
            Console.ForegroundColor = ConsoleColor.White;
            for (int column = 0; column < board.GetLength(1); column++)
            {
                Console.Write(CalculateColumnSum(column) + " ");
            }
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Yellow;
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

            Repaint(Name, BoardColor);
            Console.Write(" кидает кубик... Выпало число ");

            Repaint($"{diceRoll}!\n", ConsoleColor.Cyan);

            Thread.Sleep(500);
            Console.WriteLine("В какую колонку его поставить?");

            int chosenColumn;
            while (true)
            {
                Console.ForegroundColor = BoardColor;
                string input = Console.ReadLine();
                Console.ResetColor();

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
            Thread.Sleep(10);
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

        public void Repaint(string text, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ResetColor();
        }
    }

    class Bot : Player
    {
        public Bot(string name, ConsoleColor color) : base(name, color) { }

        public void MakeMove(Player opponent)
        {
            int diceRoll = ThrowDice();

            Repaint(Name, BoardColor);

            Console.Write(" кидает кубик... Выпало число ");

            Repaint($"{diceRoll}!\n", ConsoleColor.Cyan);

            Thread.Sleep(500);
            Repaint(Name, BoardColor);
            Console.WriteLine(" выбирает колонку...");
            Thread.Sleep(500);

            int column;
            do
            {
                column = random.Next(0, 3);
            } while (CheckRow(column) == -1);

            UpdateBoard(column, diceRoll, opponent);
        }
    }
}
