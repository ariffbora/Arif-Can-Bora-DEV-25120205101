using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

class Program
{
    static int width = 50;
    static int height = 25;

    static int playerX;
    static int playerY;

    static int score = 0;
    static int level = 1;
    static bool gameOver = false;
    static bool paused = false;

    static List<(int x, int y, char symbol)> items = new List<(int, int, char)>();
    static Random rnd = new Random();

    static string logFile = "log.txt";
    static DateTime startTime;

    static void Main()
    {
        Console.CursorVisible = false;
        Console.SetWindowSize(width, height + 2);
        Console.SetBufferSize(width, height + 2);

        File.WriteAllText(logFile, "=== GAME START ===\n");

        ShowMenu();

        playerX = width / 2;
        playerY = height - 2;

        startTime = DateTime.Now;

        while (!gameOver)
        {
            Input();

            if (!paused)
            {
                Update();
                Draw();
            }

            if ((DateTime.Now - startTime).TotalSeconds > 60 || score >= 30)
            {
                Log($"GAME OVER → score={score}");
                gameOver = true;
            }

            level = score / 5 + 1;

            int speed = 150 - level * 15;
            if (speed < 40) speed = 40;

            Thread.Sleep(speed);
        }

        GameOverScreen();
    }

    static void ShowMenu()
    {
        Console.Clear();
        Console.WriteLine("=== CATCH GAME ===");
        Console.WriteLine("← → hareket");
        Console.WriteLine("* = puan  O = puan");
        Console.WriteLine("P = pause   ESC = çık");
        Console.WriteLine("\nBaşlamak için tuşa bas...");
        Console.ReadKey();
    }

    static void Input()
    {
        while (Console.KeyAvailable)
        {
            var key = Console.ReadKey(true).Key;

            Log($"INPUT → key={key} playerX={playerX} playerY={playerY}");

            if (key == ConsoleKey.LeftArrow && playerX > 1) playerX--;
            if (key == ConsoleKey.RightArrow && playerX < width - 2) playerX++;

            if (key == ConsoleKey.P) paused = !paused;
            if (key == ConsoleKey.Escape) gameOver = true;

            Log($"MOVE → playerX={playerX} playerY={playerY}");
        }
    }

    static void Update()
    {
        if (rnd.Next(0, 3) == 0)
        {
            int x = rnd.Next(1, width - 1);
            char symbol = rnd.Next(0, 2) == 0 ? '*' : 'O';

            items.Add((x, 1, symbol));
            Log($"UPDATE → itemSpawned x={x} y=1 symbol={symbol}");
        }

        for (int i = 0; i < items.Count; i++)
        {
            var item = items[i];
            item.y++;
            items[i] = item;

            Log($"UPDATE → itemMoved x={item.x} y={item.y}");

            if (item.x == playerX && item.y == playerY)
            {
                score++;
                Log($"COLLISION → score={score}");

                items.RemoveAt(i);
                i--;
                continue;
            }

            if (item.y >= height - 1)
            {
                items.RemoveAt(i);
                i--;
            }
        }
    }

    static void Draw()
    {
        Console.SetCursorPosition(0, 0);

        char[,] buffer = new char[height, width];

        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                buffer[y, x] = ' ';

        for (int x = 0; x < width; x++)
        {
            buffer[0, x] = '#';
            buffer[height - 1, x] = '#';
        }

        for (int y = 0; y < height; y++)
        {
            buffer[y, 0] = '#';
            buffer[y, width - 1] = '#';
        }

        buffer[playerY, playerX] = '@';

        foreach (var item in items)
        {
            if (item.y > 0 && item.y < height)
                buffer[item.y, item.x] = item.symbol;
        }

        for (int y = 0; y < height; y++)
        {
            Console.SetCursorPosition(0, y);
            for (int x = 0; x < width; x++)
            {
                char c = buffer[y, x];

                if (c == '@') Console.ForegroundColor = ConsoleColor.Green;
                else if (c == '*') Console.ForegroundColor = ConsoleColor.Yellow;
                else if (c == 'O') Console.ForegroundColor = ConsoleColor.Cyan;
                else Console.ResetColor();

                Console.Write(c);
            }
        }

        Console.ResetColor();
        Console.SetCursorPosition(2, 0);
        Console.Write($"Score:{score} Level:{level}");

        if (paused)
        {
            Console.SetCursorPosition(width / 2 - 3, height / 2);
            Console.Write("PAUSE");
        }
    }

    static void GameOverScreen()
    {
        Console.Clear();
        Console.WriteLine("===== GAME OVER =====");
        Console.WriteLine("Score: " + score);

        int high = 0;
        if (File.Exists("high.txt"))
            high = int.Parse(File.ReadAllText("high.txt"));

        if (score > high)
        {
            File.WriteAllText("high.txt", score.ToString());
            Console.WriteLine("NEW HIGH SCORE!");
        }
        else
        {
            Console.WriteLine("High Score: " + high);
        }

        Console.WriteLine("Çıkmak için tuşa bas...");
        Console.ReadKey();
    }

    static void Log(string message)
    {
        File.AppendAllText(logFile, message + "\n");
    }
}