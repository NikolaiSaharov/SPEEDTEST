using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Diagnostics;
using System.Threading;

[Serializable]
public class User
{
    public string Name { get; set; }
    public int CharactersPerMinute { get; set; }
    public int CharactersPerSecond { get; set; }
}

public static class Leaderboard
{
    private const string FileName = "leaderboard.json";
    private static List<User> users;

    static Leaderboard()
    {
        users = LoadLeaderboard();
    }

    public static void AddUser(User user)
    {
        users.Add(user);
        SaveLeaderboard();
    }

    public static List<User> GetLeaderboard()
    {
        return users;
    }

    private static List<User> LoadLeaderboard()
    {
        if (File.Exists(FileName))
        {
            var json = File.ReadAllText(FileName);
            return JsonSerializer.Deserialize<List<User>>(json);
        }

        return new List<User>();
    }

    private static void SaveLeaderboard()
    {
        var json = JsonSerializer.Serialize(users);
        File.WriteAllText(FileName, json);
    }
}

public class TypingTest
{
    private const int TestDuration = 60;
    private string text;
    private string userText;
    private Stopwatch stopwatch;
    private Thread timerThread;

    public TypingTest(string text)
    {
        this.text = text;
    }

    public void Start()
    {
        Console.Write("Введите ваше имя: ");
        var name = Console.ReadLine();
        Console.WriteLine($"Вам нужно будет набрать следующий текст:\n{text}");
        Console.WriteLine("\nНажмите Enter, чтобы начать набор текста...");
        Console.ReadLine();

        stopwatch = new Stopwatch();
        userText = "";
        timerThread = new Thread(() => TimerCallback(name));
        timerThread.Start();

        Console.Clear();

        stopwatch.Start();
        ConsoleKey[] allowedKeys = text.ToCharArray().Select(c => (ConsoleKey)c).ToArray();


        while (true)
        {
            Console.SetCursorPosition(0, 3);
            Console.Write(new string(' ', Console.WindowWidth - 1));
            Console.SetCursorPosition(0, 3);
            Console.Write($"Вводите текст: \"{text}\"");

            Console.SetCursorPosition(0, 4);
            Console.Write(new string(' ', Console.WindowWidth - 1));
            Console.SetCursorPosition(0, 4);
            Console.Write(userText);

            ConsoleKeyInfo key;
            do
            {
                key = Console.ReadKey(true);
            } while (!allowedKeys.Contains(key.Key));

            if ((key.Key == ConsoleKey.Enter || stopwatch.Elapsed.TotalSeconds >= TestDuration) && allowedKeys.Contains(key.Key))

            {
                break;
            }

            if (text.Contains(key.KeyChar.ToString()))
            {
                Console.Write(key.KeyChar);
                userText += key.KeyChar;
            }

            userText += key.KeyChar;
        }

        stopwatch.Stop();
        timerThread.Join();

        CalculateTypingSpeed(name, stopwatch.Elapsed.TotalSeconds);
    }

    private void TimerCallback(string name)
    {
        Console.CursorVisible = false;

        for (int i = 0; i < TestDuration; i++)
        {
            Console.SetCursorPosition(0, 1);
            Console.Write($"Оставшееся время: {TestDuration - i} сек.");

            Thread.Sleep(1000);
        }

        Console.CursorVisible = true;

        CalculateTypingSpeed(name, TestDuration);
    }

    private void CalculateTypingSpeed(string name, double elapsedTime)
    {
        var charactersTyped = userText.Length;
        var charactersPerMinute = (int)(charactersTyped / (elapsedTime / 60));
        var charactersPerSecond = (int)(charactersTyped / elapsedTime);

        var user = new User
        {
            Name = name,
            CharactersPerMinute = charactersPerMinute,
            CharactersPerSecond = charactersPerSecond
        };

        Leaderboard.AddUser(user);

        Console.Clear();
        Console.WriteLine("Таблица рекордов:");
        var leaderboard = Leaderboard.GetLeaderboard();
        foreach (var entry in leaderboard)
        {
            Console.WriteLine($"Имя: {entry.Name}, Символов в минуту: {entry.CharactersPerMinute}, Символов в секунду: {entry.CharactersPerSecond}");
        }
    }
}

class Program
{
    static void Main(string[] args)
    {
        bool playAgain = true;

        while (playAgain)
        {
            Console.Write("Введите текст для теста: ");
            var text = Console.ReadLine();
            var typingTest = new TypingTest(text);
            typingTest.Start();

            Console.Write("Хотите пройти тест ещё раз? (да/нет): ");
            string response = Console.ReadLine();

            if (response.ToLower() != "да")
            {
                playAgain = false;
            }

            Console.Clear();
        }
    }
}

