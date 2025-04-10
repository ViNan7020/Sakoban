using Microsoft.VisualBasic;
using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Timers;

namespace sak
{

    public delegate void CallBack(char[][] area);
    class GameSak
    {
        private LevelsChecker levelsChecker;
        private System.Timers.Timer? timer;
        private Dictionary<string, char[][]> levels;
        private char[][] currentLevel;
        public char[][] playingArea;
        private Dictionary<string, ConsoleKey> _buttons;
        private List<List<int>> finishCoordinates;
        private int currentScore = 0,
            steps = 0, tries = 1,
            boxCount = 0, gamesCount = 0,
            secs, mins,
            playerX, playerY;
        private bool isTimerShow = true, isTimerWork = true,
            isStepsShow = true, isStepsWork = true,
            isTriesShow = true, isTriesWork = true,
            isScoresShow = true, isScoresWork = true,
            isColorsShow = true;
        private string dirPath = Directory.GetCurrentDirectory();
        private string IncomingTime = DateTime.Now.ToString();
        public GameSak()
        {
            levelsChecker = new LevelsChecker(); 
            levels = new Dictionary<string, char[][]>();
            foreach (var key in levelsChecker.LevelsAdd())
            {
                levels.Add(key.Key, key.Value);
            }
            finishCoordinates = new List<List<int>>();
            
            _buttons = new Dictionary<string, ConsoleKey>()
            {
                {"Up", ConsoleKey.W },
                {"Down", ConsoleKey.S },
                {"Left", ConsoleKey.A },
                {"Right", ConsoleKey.D },
                {"Restart", ConsoleKey.R },
                {"Interact", ConsoleKey.Enter },
            };

            IncomingTime = string.Join(string.Empty,IncomingTime.Split(' ', '.', ':'));
            dirPath = dirPath.Substring(0, dirPath.IndexOf("\\bin")) + "\\logs";
            
        } 

        public void start()
        {
            string levelTitle;
            try
            {
                levelTitle = SelectMenuLevel();
                currentLevel = levels[levelTitle];
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.ReadKey();
                return;
            }
            FileStream file = File.Create(dirPath + "\\log" + IncomingTime + ".txt");
            Console.WriteLine("Press any button to start...");
            Console.ReadKey();



            playingArea = new char[currentLevel.Length][];
            for (int i = 0; i < currentLevel.Length; i++)
            {
                playingArea[i] = new char[currentLevel[i].Length];
                currentLevel[i].CopyTo(playingArea[i], 0);
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

            gamesCount++;
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

            while (true)
            {
                printField(playingArea);
                if (!checkKeyClickInGame())
                {
                    continue;
                }
                if (checkBoxesOnFinish() >= scoreForWin)
                    break;
            }

            Console.Clear();

            printField(playingArea);
            


            using (StreamWriter writer = new StreamWriter(file))
            {
                writer.Write("Game" + gamesCount + "_Time: " + DateTime.Now + "_Level: " + levelTitle + "_Steps: " + steps +
                    "_Timer: " + mins + ":" + secs + "_Tries: " + tries + " |");
                writer.Flush();
            }

            if (isTimerWork)
            {
                timer.Stop();
                timer.Dispose();
                secs = mins = 0;
            }

            tries = 1;
            steps = 0;

            Console.SetCursorPosition(Console.WindowWidth / 2 - playingArea[0].Length, playingArea.Length + 4);
            Console.WriteLine("You win. Press to continue");
            Console.ReadKey();
        }

        private string SelectMenuLevel()
        {
            if (levels.Count == 0)
            {
                throw new Exception("Levels not found");
            }
            else if (levels.Count == 1)
            {
                return levels.Keys.First();
            }

            ConsoleKey key;
            int choice = 0;
            string[] levelsNames = levels.Keys.ToArray();
            CallBack cb;

            if (isColorsShow)
                cb = new(printFieldWithColors);
            else
                cb = new(printFieldWithoutColors);

            
            do
            {
                Console.Clear();

                Console.SetCursorPosition(Console.WindowWidth / 2 - 8, Console.CursorTop);
                Console.WriteLine("Select a level");

                Console.SetCursorPosition(Console.WindowWidth / 2 - (levelsNames[choice].Length + 10) / 2, Console.CursorTop);
                Console.WriteLine(_buttons["Left"] + " < " + levelsNames[choice] +  " > " + _buttons["Right"]);
                Console.WriteLine();
                Console.SetCursorPosition(Console.WindowWidth / 2 - levelsNames[choice].Length / 2, Console.CursorTop);
                cb(levels[levelsNames[choice]]);

                key = Console.ReadKey().Key;
                if (key == _buttons["Left"])
                {
                    if (choice > 0) choice--;
                    else choice = levelsNames.Length - 1;
                }
                else if (key == _buttons["Right"])
                {
                    if (choice < levelsNames.Length - 1) choice++;
                    else choice = 0;
                }

                Console.Clear();
            } while (key != _buttons["Interact"]);

            Console.Clear();




            return levelsNames[choice];
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

        private void printField(char[][] area)
        {
            Console.Clear();
            CallBack cb;
            if (isColorsShow)
                cb = new(printFieldWithColors);
            else
                cb = new(printFieldWithoutColors);

            cb(area);

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

        private void printFieldWithColors(char[][] area)
        {
            

            for (int i = 0; i < area.Length; i++)
            {
                Console.SetCursorPosition(Console.WindowWidth / 2 - area[i].Length, Console.CursorTop);
                for (int j = 0; j < area[i].Length; j++)
                {
                    if (area[i][j] == 'H')
                    {
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.Write(area[i][j] + " ");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    else if (area[i][j] == 'B')
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
                            Console.Write(area[i][j] + " ");
                            Console.ForegroundColor = ConsoleColor.White;

                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.DarkYellow;
                            Console.Write(area[i][j] + " ");
                            Console.ForegroundColor = ConsoleColor.White;
                        }

                    }
                    else if (area[i][j] == 'X')
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write(area[i][j] + " ");
                        Console.ForegroundColor = ConsoleColor.White;
                    }


                    else Console.Write(area[i][j] + " ");
                }
                Console.WriteLine();

            }

            
        }

        private void printFieldWithoutColors(char[][] area)
        {
            

            for (int i = 0; i < area.Length; i++)
            {
                Console.SetCursorPosition(Console.WindowWidth / 2 - area[i].Length, Console.CursorTop);
                for (int j = 0; j < area[i].Length; j++)
                {
                     Console.Write(area[i][j] + " ");
                }
                Console.WriteLine();
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
            currentScore = boxesOnFinish;
            return boxesOnFinish;
        }

        private bool checkKeyClickInGame()
        {

            ConsoleKey key = new ConsoleKey();

            key = Console.ReadKey().Key;


            if (!_buttons.ContainsValue(key))
            {
                return false;
            }

            if (_buttons["Restart"] == key)
            {
                if(isTriesWork)
                    tries++;
                if(isStepsWork)
                    steps = 0;
                for (int i = 0; i < currentLevel.Length; i++)
                {
                    playingArea[i] = new char[currentLevel[i].Length];
                    currentLevel[i].CopyTo(playingArea[i], 0);
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

        private bool walking(int x, int y, int XNext, int YNext)
        {

            try
            {
                if (playingArea[y][x] == '#')
                {
                    return false;
                }
                else if (playingArea[y][x] == 'B')
                {
                    if (playingArea[YNext][XNext] == 'B' ||
                        playingArea[YNext][XNext] == '#')
                    {
                        return false;
                    }
                    playingArea[playerY][playerX] = ' ';
                    playingArea[y][x] = 'H';
                    playingArea[YNext][XNext] = 'B';
                }
                else
                {
                    playingArea[playerY][playerX] = ' ';
                    playingArea[y][x] = 'H';
                }
            }
            catch
            {
                return false;
            }

            playerX = x;
            playerY = y;

            return true;
        }

        private bool checkWalking(ConsoleKey button)
        {
            if (button == _buttons["Up"])
            {
                if (!walking(playerX, playerY - 1, playerX, playerY - 2))
                {
                    return false;
                }
            }
            if (button == _buttons["Down"])
            {
                if (!walking(playerX, playerY + 1, playerX, playerY + 2))
                {
                    return false;
                }
            }
            if (button == _buttons["Left"])
            {
                if (!walking(playerX - 1, playerY, playerX - 2, playerY))
                {
                    return false;
                }
            }
            if (button == _buttons["Right"])
            {
                if (!walking(playerX + 1, playerY, playerX + 2, playerY))
                {
                    return false;
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

        public void mainMenu()
        {
            string[] menuLetters =
            {
                "Play",
                "Rules of game",
                "Logs",
                "Settings",
                "Exit"
            };
            while (true)
            {
                int choice = 0;
                ConsoleKey key = new ConsoleKey();
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
                    
                    key = Console.ReadKey().Key;
                    if (key == _buttons["Up"])
                    {
                        if (choice > 0) choice--;
                        else choice = menuLetters.Length - 1;
                    }
                    else if (key == _buttons["Down"])
                    {
                        if (choice < menuLetters.Length - 1) choice++;
                        else choice = 0;
                    }

                    Console.Clear();
                } while (key != _buttons["Interact"]);

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
                        logs();
                        break;
                    case 3:
                        settings();
                        break;
                    case 4:
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
                _buttons["Up"] + " - Up",
                _buttons["Down"] + " - Down",
                _buttons["Left"] + " - Left",
                _buttons["Right"] + " - Right",
                _buttons["Restart"] + " - Restart a level",
                _buttons["Interact"] + " - Button interaction in menus"
            };
            
            foreach (string letter in supportLetters)
            {
                Console.SetCursorPosition(Console.WindowWidth / 2 - letter.Length / 2, Console.CursorTop);
                Console.WriteLine(letter);
            }
            
            Console.ReadKey();
            Console.Clear();
        }

        private void logs()
        {
            string[] files = Directory.GetFiles(dirPath);
            string[] lettersLogs = new string[files.Length + 1];

            if(files.Length == 0)
            {
                Console.WriteLine("Logs not found");
                Console.ReadKey();
                return;
            }
            for (int i = 0; i < Directory.GetFiles(dirPath).Length; i++)
            {
                string name = files[i].Split("\\").Last();
                files[i] = name.Substring(0, name.IndexOf(".txt"));
                lettersLogs[i] = files[i].Substring(3);
            }

            lettersLogs[lettersLogs.Length - 1] = "Exit";

            while (true) {
                int choice = 0;
                ConsoleKey key;
                do
                {
                    Console.Clear() ;
                    for (int i = 0; i < lettersLogs.Length; i++)
                    {
                        if (i == choice && isColorsShow)
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine((i + 1) + ". " + lettersLogs[i]);
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                        else if (i == choice)
                        {
                            Console.WriteLine((i + 1) + ". " + lettersLogs[i] + " <-");
                        }
                        else Console.WriteLine((i + 1) + ". " + lettersLogs[i]);
                    }
                    key = Console.ReadKey().Key;
                    if (key == _buttons["Up"])
                    {
                        if (choice > 0) choice--;
                        else choice = files.Length - 1;
                    }
                    else if (key == _buttons["Down"])
                    {
                        if (choice < lettersLogs.Length - 1) choice++;
                        else choice = 0;
                    }
                    Console.Clear();
                } while (key != _buttons["Interact"]);
                
                if(choice == lettersLogs.Length - 1)return;

                using (StreamReader reader = new StreamReader(dirPath + "\\" + files[choice] + ".txt"))
                {
                    choice = 0;
                    string[] gamesInOneIncoming = reader.ReadToEnd().Split('|');
                    List<string> lettersForGames = new List<string>();
                    foreach (string game in gamesInOneIncoming)
                    {
                        if(game == "")continue;
                        lettersForGames.Add(game);
                    }
                    if (lettersForGames.Count != 0)
                    {
                        lettersForGames.Add("Exit");
                        do
                        {
                            for (int i = 0; i < lettersForGames.Count; i++)
                            {

                                if (i == choice && isColorsShow)
                                {
                                    Console.ForegroundColor = ConsoleColor.Cyan;
                                    Console.WriteLine((i + 1) + ". " + lettersForGames[i].Split("_").First());
                                    Console.ForegroundColor = ConsoleColor.White;
                                }
                                else if (i == choice)
                                {
                                    Console.WriteLine((i + 1) + ". " + lettersForGames[i].Split("_").First() + " <-");
                                }
                                else Console.WriteLine((i + 1) + ". " + lettersForGames[i].Split("_").First());
                            }
                            key = Console.ReadKey().Key;
                            if (key == _buttons["Up"])
                            {
                                if (choice > 0) choice--;
                                else choice = files.Length - 1;
                            }
                            else if (key == _buttons["Down"])
                            {
                                if (choice < lettersForGames.Count - 1) choice++;
                                else choice = 0;
                            }
                            Console.Clear();
                        } while (key != _buttons["Interact"]);
                        if (choice != lettersForGames.Count - 1)
                            logPrint(gamesInOneIncoming[choice].Split("_"));
                    }
                }
                
            }
        }

        private void logPrint(string[] game)
        {
            Console.WriteLine(game[0]);
            for (int i = 1; i < game.Length; i++) {
                Console.WriteLine("   " + game[i]);
            
            }
            Console.ReadKey();
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
                "Key Bindings ",
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
               "You can change keys bindings",
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
            ConsoleKey key = new ConsoleKey();
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
                    "",
                    "",
                };
                
                
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

                    key = Console.ReadKey().Key;
                    if ( key == _buttons["Up"])
                    {
                        if(choice > 0) choice--;
                        else choice = settingsLetters.Length - 1;
                    }
                    else if (key == _buttons["Down"])
                    {
                        if (choice < settingsLetters.Length - 1) choice++;
                        else choice = 0;
                    }

                    Console.Clear();
                } while (key != _buttons["Interact"]);

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
                        changeKeyBindings();
                        break;
                    case 10:
                        return;
                    default:
                        break;
                }
            }


        }

        private void changeKeyBindings()
        {
            int choice = 0;
            string[] keyBindings = new string[_buttons.Count + 1];
            ConsoleKey key = new ConsoleKey();

            while (true) {

                choice = 0;
                foreach (var button in _buttons)
                {
                    keyBindings[choice++] = button.Key + ": " + button.Value;
                }

                keyBindings[choice++] = "Exit";
                choice = 0;
                do
                {
                    for (int i = 0; i < keyBindings.Length; i++)
                    {
                        if (i == choice)
                        {
                            if (isColorsShow)
                            {
                                Console.ForegroundColor = ConsoleColor.Cyan;
                                Console.WriteLine(keyBindings[i]);
                                Console.ForegroundColor = ConsoleColor.White;
                            }
                            else
                                Console.WriteLine(keyBindings[i] + " <-");
                        }
                        else
                        {
                            Console.WriteLine(keyBindings[i]);
                        }
                    }
                    

                   

                    key = Console.ReadKey().Key;
                    if (key == _buttons["Up"])
                    {
                        if (choice > 0) choice--;
                        else choice = keyBindings.Length - 1;
                    }
                    else if (key == _buttons["Down"])
                    {
                        if (choice < keyBindings.Length - 1) choice++;
                        else choice = 0;
                    }

                    Console.Clear();
                } while (key != _buttons["Interact"]);

                if(choice == keyBindings.Length - 1)
                {
                    return;
                }

                string keySubString = keyBindings[choice].Substring(0, keyBindings[choice].IndexOf(":"));
                keyBindings[choice] = keySubString + ": ....";

                Console.ForegroundColor= ConsoleColor.Gray;
                for (int i = 0; i < keyBindings.Length; i++)
                {
                    if (i == choice)
                    {
                        if (isColorsShow)
                        {
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine(keyBindings[i]);
                            Console.ForegroundColor = ConsoleColor.Gray;
                        }
                        else
                            Console.WriteLine(keyBindings[i] + " <-");
                    }
                    else
                    {
                        Console.WriteLine(keyBindings[i]);
                    }
                }

                Console.WriteLine("Press to bind a button");
                _buttons[keySubString] = Console.ReadKey().Key;
                Console.Clear();
            }

        }
        private string YesOrNo(bool del) => del ? "Yes" : "No";
    }

    class LevelsChecker
    {
        private char[][] defaultFirstLevel =
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

        private char[][] defaultSecondLevel =
        {
                new char[]{' ', '#',' ','#','#','#',' ',' ',' ',' ',' '},
                new char[]{' ', '#',' ',' ',' ','#',' ',' ',' ',' ',' '},
                new char[]{' ', '#',' ','B',' ','#',' ',' ',' ',' ',' '},
                new char[]{' ', '#',' ',' ',' ','#',' ',' ',' ',' ',' '},
                new char[]{'#', '#',' ','#',' ','#','#','#','#','#','#'},
                new char[]{' ', ' ',' ',' ',' ',' ','#',' ','X',' ',' '},
                new char[]{'#', ' ','B',' ','B',' ',' ','H','X',' ','#'},
                new char[]{'#', ' ',' ',' ',' ',' ',' ',' ','X',' ','#'},
                new char[]{'#', '#',' ','#','B','#','#',' ','X',' ','#'},
                new char[]{' ', '#',' ',' ',' ','#',' ','#','#','#','#'},
                new char[]{' ', '#','#','#',' ','#',' ',' ',' ',' ',' '},
            };

        public LevelsChecker()
        {

        }

        public Dictionary<string, char[][]> LevelsAdd()
        {
            string dirPath = Directory.GetCurrentDirectory();
            dirPath = dirPath.Substring(0, dirPath.IndexOf("\\bin")) + "\\levels";
            if (Directory.Exists(dirPath) && Directory.GetFiles(dirPath).Length > 0)
            {
                Dictionary<string, char[][]> levels = new Dictionary<string, char[][]>();
                foreach (string levelName in Directory.GetFiles(dirPath))
                { 
                    using(StreamReader reader = new StreamReader(levelName))
                    {
                        string[] fullLevel = reader.ReadToEnd().Split("\r\n");
                        char[][] level = new char[fullLevel.Length][];
                        for (int i = 0; i < level.Length; i++)
                        {
                            level[i] = fullLevel[i].ToCharArray();
                        }
                        string name = levelName.Split("\\").Last();
                        levels.Add(name.Substring(0, name.IndexOf(".txt")), level);
                    }
                }
                return levels;
            }
            Dictionary<string, char[][]> defaultLevels = new Dictionary<string, char[][]>();
            defaultLevels.Add(nameof(defaultFirstLevel), defaultFirstLevel);
            defaultLevels.Add(nameof(defaultSecondLevel), defaultSecondLevel);
            return defaultLevels;
        }
    }

    internal class Program
    {

        static void Main(string[] args)
        {
            
            GameSak game = new GameSak();
            game.mainMenu();
        }
    }
}