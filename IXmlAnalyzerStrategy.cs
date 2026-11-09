namespace XmlAnalyzer;

public interface IXmlAnalyzerStrategy
{
    List<StudentData> Analyze(string filePath, Dictionary<string, string> searchParams);
}