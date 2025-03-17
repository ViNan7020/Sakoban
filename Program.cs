using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Timers;

namespace sak
{
    public delegate void CallBack();
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
        private bool isTimerShow = true, isTimerWork = true;
        private bool isStepsShow = true, isStepsWork = true;
        private bool isTriesShow = true, isTriesWork = true;
        private bool isScoresShow = true, isScoresWork = true;
        private bool isColorsShow = true;

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
                Console.Clear();
                Console.WriteLine("Level is impossible. The number of drawers is less than the space for drawers");
                Console.ReadKey();
                return;
            }

            if (isTimerWork)
            {
                timer = new System.Timers.Timer();
                timer.Elapsed += (s, e) =>
                {
                    printTime(true);
                };
                timer.Interval = 1000;
                timer.Start();
            }
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

            CallBack cb;
            if (isColorsShow)
                cb = new(printFieldWithColors);
            else
                cb = new(printField);


            while (true)
            {
                cb();
                if (!checkKeyClickInGame())
                {
                    continue;
                }
                if (checkBoxesOnFinish() >= scoreForWin)
                    break;
            }

            Console.Clear();
            cb();
            if (isTimerWork)
            {
                timer.Stop();
                timer.Dispose();
                secs = mins = 0;
            }
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
                    mins += (int)Math.Floor((double)secs/60);
                    secs %= 60;
                }
            } 

            string secsStr = secs.ToString(),
                    minsStr = mins.ToString();

            if(secs < 10)
            {
                secsStr = "0" + secs;
            }
            else
            if (mins < 10)
            {
                minsStr = "0" + mins;
            }

            Console.SetCursorPosition(Console.WindowWidth / 2 - playingArea[0].Length + 10, playingArea.Length + 2);
            Console.Write(minsStr + ":" + secsStr + "  ");
            
        }

        private void printFieldWithColors()
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
            if (isStepsShow)
                Console.Write("Steps: " + steps);
            if (isTriesShow)
                Console.WriteLine("\tTries: " + tries);
            Console.SetCursorPosition(Console.WindowWidth / 2 - playingArea[0].Length + 6, Console.CursorTop);
            if (isScoresShow)
                Console.WriteLine("Scores: " + currentScore);
            Console.SetCursorPosition(Console.WindowWidth / 2 - playingArea[0].Length + 4, Console.CursorTop);
            if (isTimerShow)
            {
                Console.Write("Time: ");
                printTime(false);
            }
        }

        private void printField()
        {
            Console.Clear();

            for (int i = 0; i < playingArea.Length; i++)
            {
                Console.SetCursorPosition(Console.WindowWidth / 2 - playingArea[i].Length, Console.CursorTop);
                for (int j = 0; j < playingArea[i].Length; j++)
                {
                     Console.Write(playingArea[i][j] + " ");
                }
                Console.WriteLine();
            }
            Console.SetCursorPosition(Console.WindowWidth / 2 - playingArea[0].Length, Console.CursorTop);
            if (isStepsShow)
                Console.Write("Steps: " + steps);
            if(isTriesShow)
                Console.WriteLine("\tTries: " + tries);
            Console.SetCursorPosition(Console.WindowWidth / 2 - playingArea[0].Length + 6, Console.CursorTop);
            if (isScoresShow) 
                Console.WriteLine("Scores: " + currentScore);
            Console.SetCursorPosition(Console.WindowWidth / 2 - playingArea[0].Length + 4, Console.CursorTop);
            if (isTimerShow)
            {
                Console.Write("Time: ");
                printTime(false);
            }
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
                if(isTriesWork)
                    tries++;
                if(isStepsWork)
                    steps = 0;
                for (int i = 0; i < _field.Length; i++)
                {
                    playingArea[i] = new char[_field[i].Length];
                    _field[i].CopyTo(playingArea[i], 0);
                }
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
                return false;
            }
            if (checkWalking(key))
            {
                if(isStepsWork)
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

        public void mainMenu()
        {
            string[] menuLetters =
            {
                "Play",
                "Rules of game",
                "Settings",
                "Exit"
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
                        if (i == choice && isColorsShow)
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine((i + 1) + ". " + menuLetters[i]);
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                        else if(i == choice)
                        {
                            Console.WriteLine((i + 1) + ". " + menuLetters[i] + " <-");
                        }
                        else Console.WriteLine((i + 1) + ". " + menuLetters[i]);
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
                        settings();
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
        private void support()
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

        private void settings()
        {
            string[] settingsLetters = 
            {
                "Timer show: ",
                "Timer work: ",
                "Steps show: ",
                "Steps work: ",
                "Tries show: ",
                "Tries work: ",
                "Scores show: ",
                "Scores work: ",
                "Colors show: ",
                "Exit"
            };
            string[] settingLetterDescriptions =
            {
               "Show Timer in Game",
               "Disable Timer in game",
               "Show Steps in Game",
               "Disable Steps in game",
               "Show Tries in Game",
               "Disable Tries in game",
               "Show Scores in Game",
               "Disable Scores in game",
               "Disable Colors in game and menu",
               " ",
            };

            int choice = 0;
            int maxLength = 0;
            foreach(string letter in settingsLetters)
            {
                if(maxLength < letter.Length)
                {
                    maxLength = letter.Length;
                }
            }
            ConsoleKeyInfo key;

            while (true)
            {
                
                string[] YesOrNoInSettingsLetters = new string[]
                {
                    YesOrNo(isTimerShow),
                    YesOrNo(isTimerWork),
                    YesOrNo(isStepsShow),
                    YesOrNo(isStepsWork),
                    YesOrNo(isTriesShow),
                    YesOrNo(isTriesWork),
                    YesOrNo(isScoresShow),
                    YesOrNo(isScoresWork),
                    YesOrNo(isColorsShow),
                    ""
                };
                
                key = new ConsoleKeyInfo();
                Console.Clear();
                do
                {
                    for (int i = 0; i < settingsLetters.Length; i++)
                    {
                        if (i == choice)
                        {
                            if (isColorsShow)
                            {
                                Console.ForegroundColor = ConsoleColor.Cyan;
                                Console.Write((i + 1) + ". " + settingsLetters[i]);
                                if (YesOrNoInSettingsLetters[i] == "Yes")
                                {
                                    Console.ForegroundColor = ConsoleColor.Yellow;
                                }
                                else
                                {
                                    Console.ForegroundColor = ConsoleColor.Magenta;
                                }
                                Console.WriteLine(YesOrNoInSettingsLetters[i]);
                                Console.ForegroundColor = ConsoleColor.White;
                            }
                            else
                            {
                                Console.WriteLine((i + 1) + ". " + settingsLetters[i] + YesOrNoInSettingsLetters[i] + " <-");
                            }
                            Console.SetCursorPosition((Console.WindowWidth / 2 - settingsLetters[i].Length / 2) / 2, Console.CursorTop - 1);
                            Console.WriteLine(settingLetterDescriptions[i]);
                        }
                        else if (isColorsShow)
                        {
                            Console.Write((i + 1) + ". " + settingsLetters[i]);
                            if (YesOrNoInSettingsLetters[i] == "Yes")
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                            }
                            Console.WriteLine(YesOrNoInSettingsLetters[i]);
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                        else Console.WriteLine((i + 1) + ". " + settingsLetters[i] + YesOrNoInSettingsLetters[i]);
                    }

                    key = Console.ReadKey();
                    if (choice > 0 && key.Key == ConsoleKey.W)
                    {
                        choice--;
                    }
                    else if (choice < settingsLetters.Length - 1
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
                        isTimerShow = !isTimerShow;
                        if(!isTimerWork)
                            isTimerWork = !isTimerWork;
                        break;
                    case 1:
                        isTimerWork = !isTimerWork;
                        if(isTimerShow)
                            isTimerShow = !isTimerShow;
                        break;
                    case 2:
                        isStepsShow = !isStepsShow;
                        if (!isStepsWork)
                            isStepsWork = !isStepsWork;
                        break;
                    case 3:
                        isStepsWork = !isStepsWork;
                        if(isStepsShow)
                            isStepsShow = !isStepsShow;
                        break;
                    case 4:
                        isTriesShow = !isTriesShow;
                        if (!isTriesWork)
                            isTriesWork = !isTriesWork;
                        break;
                    case 5:
                        isTriesWork = !isTriesWork;
                        if(isTriesShow)
                            isTriesShow = !isTriesShow;
                        break;
                    case 6:
                        isScoresShow = !isScoresShow;
                        if (!isScoresWork)
                            isScoresWork = !isScoresWork;
                        break;
                    case 7:
                        isScoresWork = !isScoresWork;
                        if (isScoresShow)
                            isScoresShow = !isScoresShow;
                        break;
                    case 8:
                        isColorsShow = !isColorsShow;
                        break;
                    case 9:
                        return;
                    default:
                        break;
                }
            }

        }

        private string YesOrNo(bool del) => del ? "Yes" : "No";
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
            game.mainMenu();
        }
    }
}