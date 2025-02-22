using System;
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
        private double time;
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
                printField();
                
            };
            timer.Interval = 33;
            timer.Start();

            while (true)
            {
                
                if (!walking())
                {
                    continue;
                }
                if (checkBoxesOnFinish() >= scoreForWin)
                    break;
            }
            
            timer.Stop();
            timer.Dispose();
            printField();
            Console.SetCursorPosition(Console.WindowWidth / 2 - playingArea[0].Length, Console.CursorTop);
            Console.WriteLine("You win");
            Console.ReadKey();
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
            Console.WriteLine("Time: " + time);
            time += 0.033;
            time = Math.Round(time, 2);
           
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

        private bool walking()
        {

            char key = ' ';

            key = Console.ReadKey()
                  .KeyChar
                  .ToString()
                  .ToUpper()
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
            int row = 0, str = 0;
            for (int i = 0; i < playingArea.Length; i++)
            {
                int search = Array.IndexOf(playingArea[i], 'H');
                if (search != -1)
                {
                    row = search;
                    str = i;
                    break;
                }
            }


            if (button == 'W')
            {
                if (str < 1) return false;
                if (playingArea[str - 1][row] == '#')
                {
                    return false;
                }
                else if (playingArea[str - 1][row] == 'B')
                {
                    if (str < 2 || playingArea[str - 2][row] == 'B' ||
                        playingArea[str - 2][row] == '#')
                    {
                        return false;
                    }
                    playingArea[str][row] = ' ';
                    playingArea[str - 1][row] = 'H';
                    playingArea[str - 2][row] = 'B';
                }
                else if (str != 0 && playingArea[str - 1][row] == 'X')
                {
                    playingArea[str][row] = 'X';
                    playingArea[str - 1][row] = 'H';
                }
                else
                {
                    playingArea[str][row] = ' ';
                    playingArea[str - 1][row] = 'H';
                }
            }
            if (button == 'A')
            {
                if (row < 1) return false;
                if (playingArea[str][row - 1] == '#')
                {
                    return false;
                }
                else if (playingArea[str][row - 1] == 'B')
                {
                    if (row < 2 || playingArea[str][row - 2] == 'B' ||
                        playingArea[str][row - 2] == '#')
                    {
                        return false;
                    }
                    playingArea[str][row] = ' ';
                    playingArea[str][row - 1] = 'H';
                    playingArea[str][row - 2] = 'B';
                }

                else
                {
                    playingArea[str][row] = ' ';
                    playingArea[str][row - 1] = 'H';
                }
            }
            if (button == 'S')
            {
                if (str >= playingArea.Length - 1)
                    return false;
                if (playingArea[str + 1][row] == '#')
                {
                    return false;
                }
                else if (playingArea[str + 1][row] == 'B')
                {
                    if (str >= playingArea.Length - 2 || playingArea[str + 2][row] == 'B' ||
                        playingArea[str + 2][row] == '#'
                        )
                    {
                        return false;
                    }
                    playingArea[str][row] = ' ';
                    playingArea[str + 1][row] = 'H';
                    playingArea[str + 2][row] = 'B';
                }

                else
                {
                    playingArea[str][row] = ' ';
                    playingArea[str + 1][row] = 'H';
                }
            }
            if (button == 'D')
            {
                if (row >= playingArea.Length - 1)
                    return false;
                if (playingArea[str][row + 1] == '#')
                {
                    return false;
                }
                else if (playingArea[str][row + 1] == 'B')
                {
                    if (row >= playingArea.Length - 2 || playingArea[str][row + 2] == 'B' ||
                        playingArea[str][row + 2] == '#')
                    {
                        return false;
                    }
                    playingArea[str][row] = ' ';
                    playingArea[str][row + 1] = 'H';
                    playingArea[str][row + 2] = 'B';
                }

                else
                {
                    playingArea[str][row] = ' ';
                    playingArea[str][row + 1] = 'H';
                }
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