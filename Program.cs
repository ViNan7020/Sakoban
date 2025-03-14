using System;
using System.Diagnostics;
using System.Linq;
using System.Timers;

namespace sak
{

    class GameSak
    {
        private System.Timers.Timer timer;
        private char[][] _field;
        public char[][] playingArea;
        private int steps = 0;
        private int tries = 1;
        private char[] buttons = { 'W', 'A', 'S', 'D', 'R' };
        private List<List<int>> finishCoordinates;
        private int currentScore = 0;
        private int boxCount = 0;
        private int secs, mins;
        private int playerX, playerY;


        public GameSak(char[][] field)
        {
            _field = field;
            playingArea = new char[field.Length][];
            finishCoordinates = new List<List<int>>();
        }

        public void start()
        {
            Console.WriteLine("Press any button to start...");
            Console.ReadKey();



            for (int i = 0; i < _field.Length; i++)
            {
                playingArea[i] = new char[_field[i].Length];
                _field[i].CopyTo(playingArea[i], 0);
            }
            for (int i = 0; i < playingArea.Length; i++)
            {
                for (int j = 0; j < playingArea[i].Length; j++)
                {
                    if (playingArea[i][j] == 'B')
                    {
                        boxCount++;
                    }
                }
            }
            for (int i = 0; i < playingArea.Length; i++)
            {
                for (int j = 0; j < playingArea[i].Length; j++)
                {
                    if (playingArea[i][j] == 'X')
                    {
                        finishCoordinates.Add(new List<int> { i, j });
                    }
                }

            }

            int scoreForWin = finishCoordinates.Count;

            if (scoreForWin > boxCount)
            {
                Console.WriteLine("Level is impossible. The number of drawers is less than the space for drawers");
                return;
            }

            timer = new System.Timers.Timer();
            timer.Elapsed += (s, e) => {
                printTime(true);
            };
            timer.Interval = 1000;
            timer.Start();

            for (int i = 0; i < playingArea.Length; i++)
            {
                int x = Array.IndexOf(playingArea[i], 'H');
                if (x != -1)
                {
                    playerX = x;
                    playerY = i;
                    break;
                }
            }

            while (true)
            {
                printField();
                if (!checkKeyClickInGame())
                {
                    continue;
                }
                if (checkBoxesOnFinish() >= scoreForWin)
                    break;
            }

            Console.Clear();
            printField();
            timer.Stop();
            timer.Dispose();
            Console.SetCursorPosition(Console.WindowWidth / 2 - playingArea[0].Length, playingArea.Length + 4);
            Console.WriteLine("You win. Press to continue");
            Console.ReadKey();
        }

        private void printTime(bool needPlus)
        {
            if(needPlus)
            { 
                secs += 1;
                if (secs >= 60)
                {
                    secs -= 60;
                    mins++;
                }
            } 
            string secsStr = string.Empty, minsStr = string.Empty;
            if(secs < 10)
            {
                secsStr = "0" + secs;
            }
            if (mins < 10)
            {
                minsStr = "0" + mins;
            }

            Console.SetCursorPosition(Console.WindowWidth / 2 - playingArea[0].Length + 10, playingArea.Length + 2);
            Console.Write(minsStr + ":" + secsStr + "  ");
            
        }

        private void printField()
        {
            Console.Clear();

            for (int i = 0; i < playingArea.Length; i++)
            {
                Console.SetCursorPosition(Console.WindowWidth / 2 - playingArea[i].Length, Console.CursorTop);
                for (int j = 0; j < playingArea[i].Length; j++)
                {
                    if (playingArea[i][j] == 'H')
                    {
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.Write(playingArea[i][j] + " ");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    else if (playingArea[i][j] == 'B')
                    {
                        bool boxOnFinish = false;
                        for (int k = 0; k < finishCoordinates.Count; k++)
                        {
                            if (i == finishCoordinates[k][0] && j == finishCoordinates[k][1])
                            {
                                boxOnFinish = true;
                                break;
                            }
                        }
                        if (boxOnFinish)
                        {

                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.Write(playingArea[i][j] + " ");
                            Console.ForegroundColor = ConsoleColor.White;

                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.DarkYellow;
                            Console.Write(playingArea[i][j] + " ");
                            Console.ForegroundColor = ConsoleColor.White;
                        }

                    }
                    else if (playingArea[i][j] == 'X')
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write(playingArea[i][j] + " ");
                        Console.ForegroundColor = ConsoleColor.White;
                    }


                    else Console.Write(playingArea[i][j] + " ");
                }
                Console.WriteLine();
            }
            
            Console.SetCursorPosition(Console.WindowWidth / 2 - playingArea[0].Length, Console.CursorTop);
            Console.WriteLine("Steps: " + steps + "\tTries: " + tries);
            Console.SetCursorPosition(Console.WindowWidth / 2 - playingArea[0].Length + 6, Console.CursorTop);
            Console.WriteLine("Scores: " + currentScore);
            Console.SetCursorPosition(Console.WindowWidth / 2 - playingArea[0].Length + 4, Console.CursorTop);
            Console.Write("Time: ");
            printTime(false);
        }

        private int checkBoxesOnFinish()
        {
            int boxesOnFinish = 0;
            for (int i = 0; i < playingArea.Length; i++)
            {
                for (int j = 0; j < playingArea[i].Length; j++)
                {
                    if (playingArea[i][j] == 'B') {
                        for (int k = 0; k < finishCoordinates.Count; k++)
                        {
                            if (i == finishCoordinates[k][0] && j == finishCoordinates[k][1])
                            {
                                boxesOnFinish++;
                                break;
                            }
                        }
                    }
                }
            }
            return boxesOnFinish;
        }

        private bool checkKeyClickInGame()
        {

            char key = ' ';

            key = Console.ReadKey()
                  .Key
                  .ToString()
                  .ToCharArray()[0];

            if (!buttons.Contains(key))
            {
                return false;
            }

            if (key == 'R')
            {
                tries++;
                steps = 0;
                for (int i = 0; i < _field.Length; i++)
                {
                    playingArea[i] = new char[_field[i].Length];
                    _field[i].CopyTo(playingArea[i], 0);
                }
                return false;
            }
            if (checkWalking(key))
            {
                steps++;
            }

            return true;
        }

        private bool checkWalking(char button)
        {
            


            if (button == 'W')
            {
                if (playerY < 1) return false;
                if (playingArea[playerY - 1][playerX] == '#')
                {
                    return false;
                }
                else if (playingArea[playerY - 1][playerX] == 'B')
                {
                    if (playerY < 2 || playingArea[playerY - 2][playerX] == 'B' ||
                        playingArea[playerY - 2][playerX] == '#')
                    {
                        return false;
                    }
                    playingArea[playerY][playerX] = ' ';
                    playingArea[playerY - 1][playerX] = 'H';
                    playingArea[playerY - 2][playerX] = 'B';
                }
                else
                {
                    playingArea[playerY][playerX] = ' ';
                    playingArea[playerY - 1][playerX] = 'H';
                }
                playerY--;
            }
            else if (button == 'A')
            {
                if (playerX < 1) return false;
                if (playingArea[playerY][playerX - 1] == '#')
                {
                    return false;
                }
                else if (playingArea[playerY][playerX - 1] == 'B')
                {
                    if (playerX < 2 || playingArea[playerY][playerX - 2] == 'B' ||
                        playingArea[playerY][playerX - 2] == '#')
                    {
                        return false;
                    }
                    playingArea[playerY][playerX] = ' ';
                    playingArea[playerY][playerX - 1] = 'H';
                    playingArea[playerY][playerX - 2] = 'B';
                }

                else
                {
                    playingArea[playerY][playerX] = ' ';
                    playingArea[playerY][playerX - 1] = 'H';
                }
                playerX--;
            }
            else if (button == 'S')
            {
                if (playerY >= playingArea.Length - 1)
                    return false;
                if (playingArea[playerY + 1][playerX] == '#')
                {
                    return false;
                }
                else if (playingArea[playerY + 1][playerX] == 'B')
                {
                    if (playerY >= playingArea.Length - 2 || playingArea[playerY + 2][playerX] == 'B' ||
                        playingArea[playerY + 2][playerX] == '#'
                        )
                    {
                        return false;
                    }
                    playingArea[playerY][playerX] = ' ';
                    playingArea[playerY + 1][playerX] = 'H';
                    playingArea[playerY + 2][playerX] = 'B';
                }

                else
                {
                    playingArea[playerY][playerX] = ' ';
                    playingArea[playerY + 1][playerX] = 'H';
                }
                playerY++;
            }
            else if (button == 'D')
            {
                if (playerX >= playingArea.Length - 1)
                    return false;
                if (playingArea[playerY][playerX + 1] == '#')
                {
                    return false;
                }
                else if (playingArea[playerY][playerX + 1] == 'B')
                {
                    if (playerX >= playingArea.Length - 2 || playingArea[playerY][playerX + 2] == 'B' ||
                        playingArea[playerY][playerX + 2] == '#')
                    {
                        return false;
                    }
                    playingArea[playerY][playerX] = ' ';
                    playingArea[playerY][playerX + 1] = 'H';
                    playingArea[playerY][playerX + 2] = 'B';
                }

                else
                {
                    playingArea[playerY][playerX] = ' ';
                    playingArea[playerY][playerX + 1] = 'H';
                }
                playerX++;
            }
            for (int i = 0; i < finishCoordinates.Count; i++)
            {
                if (playingArea[finishCoordinates[i][0]]
                    [finishCoordinates[i][1]] == ' ')
                {
                    playingArea[finishCoordinates[i][0]]
                        [finishCoordinates[i][1]] = 'X';
                }
            }
            return true;
        }

        public void menu()
        {
            string[] menuLetters =
            {
                "1. Play",
                "2. Rules of game",
                "3. Settings",
                "4. Exit"
            };
            while (true)
            {
                int choice = 0;
                ConsoleKeyInfo key = new ConsoleKeyInfo();
                Console.Clear();
                do
                {
                    for (int i = 0; i < menuLetters.Length; i++)
                    {
                        if (i == choice)
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine(menuLetters[i]);
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                        else Console.WriteLine(menuLetters[i]);
                    }
                    
                    key = Console.ReadKey();
                    if (choice > 0 && key.Key == ConsoleKey.W)
                    {
                        choice--;
                    }
                    else if (choice < menuLetters.Length - 1
                                && key.Key == ConsoleKey.S)
                    {
                        choice++;
                    }
                    
                    Console.Clear();
                } while (key.Key != ConsoleKey.Enter);
                Console.Clear();
                switch (choice)
                {
                    case 0:
                        start();
                        break;
                    case 1:
                        support();
                        break;
                    case 2:
                        Console.WriteLine("Out of order");
                        Console.ReadKey();
                        Console.Clear();
                        break;
                    case 3:
                        Console.WriteLine("Thanks for playing");
                        Console.ReadKey();
                        Environment.Exit(0);
                        break;
                    default:
                        break;
                }
            }
        }
        public void support()
        {
            string[] supportLetters =
            {
                "You need to move all the boxes on the crosses.",
                "You cannot push more than one box at a time.",
                "You can walk in four directions: up, down, left, right.",
                "Control and navigation buttons: ",
                "W - Up",
                "S - Down",
                "A - Left",
                "D - Right",
                "R - Restart a level"
            };
            
            foreach (string letter in supportLetters)
            {
                Console.SetCursorPosition(Console.WindowWidth / 2 - letter.Length / 2, Console.CursorTop);
                Console.WriteLine(letter);
            }
            
            Console.ReadKey();
            Console.Clear();
        }

    }

    internal class Program
    {

        static void Main(string[] args)
        {
            char[][] field =
            {
                new char[]{' ', '#',' ','#','#','#',' ',' ',' ',' ',' '},
                new char[]{' ', '#',' ',' ',' ','#',' ',' ',' ',' ',' '},
                new char[]{' ', '#',' ','B','B','#',' ',' ',' ',' ',' '},
                new char[]{' ', '#',' ',' ',' ','#',' ',' ',' ',' ',' '},
                new char[]{'#', '#',' ','#',' ','#','#','#','#','#','#'},
                new char[]{' ', ' ',' ',' ',' ',' ','#',' ','X',' ',' '},
                new char[]{'#', ' ','B',' ',' ',' ',' ','H','X',' ','#'},
                new char[]{'#', ' ',' ',' ',' ',' ',' ',' ','X',' ','#'},
                new char[]{'#', '#',' ','#','B','#','#',' ','X',' ','#'},
                new char[]{' ', '#',' ',' ',' ','#',' ','#','#','#','#'},
                new char[]{' ', '#','#','#',' ','#',' ',' ',' ',' ',' '},
            };
            GameSak game = new GameSak(field);
            game.menu();
        }
    }
}