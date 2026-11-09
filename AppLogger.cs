using System;
using System.IO;

namespace XmlAnalyzer;

// Паттерн Singleton: гарантує, що у нас є лише ОДИН екземпляр логера на всю програму
public class AppLogger
{
    private static AppLogger _instance;
    private static readonly object _lock = new object();
    private string _logFilePath;

    // Приватний конструктор, щоб ніхто не міг створити логер через new
    private AppLogger()
    {
        // Протокол зберігатиметься на локальному диску (у папці кешу додатку)
        _logFilePath = Path.Combine(FileSystem.CacheDirectory, "AppProtocol.log");
        
        // Створюємо файл, якщо його немає, або додаємо роздільник для нового запуску
        File.AppendAllText(_logFilePath, $"\n--- Запуск програми: {DateTime.Now} ---\n");
    }

    // Єдина точка доступу до логера
    public static AppLogger Instance
    {
        get
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = new AppLogger();
                }
                return _instance;
            }
        }
    }

    // Метод для запису подій (підтримує рівні важливості: Фільтрація, Трансформація, Збереження)
    public void Log(string level, string message)
    {
        // Формат: 21.09.2023 13:02:22 Рівень. Повідомлення
        string logEntry = $"{DateTime.Now:dd.MM.yyyy HH:mm:ss} {level}. {message}\n";
        File.AppendAllText(_logFilePath, logEntry);
    }
}