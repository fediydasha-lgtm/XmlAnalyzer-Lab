namespace XmlAnalyzer;

public class StudentData
{
    // Властивості (properties) для зберігання атрибутів головного вузла <Student>
    public string FullName { get; set; } = string.Empty;
    public string Faculty { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Course { get; set; } = string.Empty;

    // Спеціальні властивості для відображення вкладених даних (дисциплін та оцінок)
    public string Disciplines { get; set; } = string.Empty;
    public string Performance { get; set; } = string.Empty;
}