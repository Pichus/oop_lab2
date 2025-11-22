using System;
using System.IO;

namespace XMLParser.Services;

public class AppLogger
{
    public enum EventType
    {
        Filtering,
        Transformation,
        Saving
    }

    private static readonly Lazy<AppLogger> _instance = new(() => new AppLogger());
    private readonly object _locker = new();

    private readonly string _logFilePath;

    private AppLogger()
    {
        var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "XMLParser");
        Directory.CreateDirectory(dir);
        _logFilePath = Path.Combine(dir, "app.log");
    }

    public static AppLogger Instance => _instance.Value;

    public void LogEvent(EventType type, string message)
    {
        var line = $"{DateTime.Now:dd.MM.yyyy HH:mm:ss} {TypeToText(type)}. {message}";
        Console.WriteLine(line);
        lock (_locker)
        {
            File.AppendAllLines(_logFilePath, new[] { line });
        }
    }

    private static string TypeToText(EventType t)
    {
        return t switch
        {
            EventType.Filtering => "Фільтрація",
            EventType.Transformation => "Трансформація",
            EventType.Saving => "Збереження",
            _ => "Подія"
        };
    }
}