using System;
using System.IO;
using Microsoft.Maui.Storage; // Обов'язково для доступу до файлової системи MAUI

namespace XmlAnalyzer;

public interface IDataSaver
{
    void SaveData(string sourceFilePath);
}

public class XmlSaver : IDataSaver
{
    public void SaveData(string sourceFilePath)
    {
        // Використовуємо гарантовано існуючу кеш-папку програми
        string destPath = Path.Combine(FileSystem.CacheDirectory, "Saved_GoogleDrive.xml");
        File.Copy(sourceFilePath, destPath, true);
        
        AppLogger.Instance.Log("Збереження", "Збережено відфільтрований фрагмент у файл Saved_GoogleDrive.xml");
    }
}

public class HtmlSaver : IDataSaver
{
    public void SaveData(string sourceFilePath)
    {
        string destPath = Path.Combine(FileSystem.CacheDirectory, "Saved_GoogleDrive.html");
        File.Copy(sourceFilePath, destPath, true);
        
        AppLogger.Instance.Log("Збереження", "Збережено відфільтрований фрагмент у файл Saved_GoogleDrive.html");
    }
}

public class SaverFactory
{
    public IDataSaver CreateSaver(string format)
    {
        if (format.ToLower() == "xml") return new XmlSaver();
        else if (format.ToLower() == "html") return new HtmlSaver();
        else throw new ArgumentException("Невідомий формат файлу");
    }
}