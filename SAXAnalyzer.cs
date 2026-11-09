using System.Xml;

namespace XmlAnalyzer;

public class SAXAnalyzer : IXmlAnalyzerStrategy
{
    public List<StudentData> Analyze(string filePath, Dictionary<string, string> searchParams)
    {
        var results = new List<StudentData>();
        StudentData currentStudent = null;
        List<string> currentDisciplines = new List<string>();
        bool isMatch = false;

        // Створюємо XmlReader для потокового читання
        using (XmlReader reader = XmlReader.Create(filePath))
        {
            while (reader.Read())
            {
                // Якщо натрапили на відкриваючий тег <Student>
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "Student")
                {
                    // Зчитуємо атрибути
                    string fullName = reader.GetAttribute("FullName") ?? "";
                    string faculty = reader.GetAttribute("Faculty") ?? "";
                    string department = reader.GetAttribute("Department") ?? "";
                    string course = reader.GetAttribute("Course") ?? "";

                    // Перевіряємо, чи підходить цей студент під фільтри
                    isMatch = true;
                    foreach (var param in searchParams)
                    {
                        string attrValue = reader.GetAttribute(param.Key) ?? "";
                        if (attrValue != param.Value)
                        {
                            isMatch = false;
                            break;
                        }
                    }

                    // Якщо підходить, створюємо об'єкт (але поки без дисциплін)
                    if (isMatch)
                    {
                        currentStudent = new StudentData
                        {
                            FullName = fullName,
                            Faculty = faculty,
                            Department = department,
                            Course = course
                        };
                        currentDisciplines.Clear(); // Очищаємо список оцінок для нового студента
                    }
                }

                // Якщо ми всередині "підходящого" студента і знайшли тег <Discipline>
                if (isMatch && reader.NodeType == XmlNodeType.Element && reader.Name == "Discipline")
                {
                    string name = reader.GetAttribute("Name") ?? "";
                    string score = reader.GetAttribute("Score") ?? "";
                    currentDisciplines.Add($"{name} - {score} балів");
                }

                // Якщо натрапили на закриваючий тег </Student> і цей студент нам підходив
                if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "Student" && isMatch)
                {
                    if (currentStudent != null)
                    {
                        // Зліплюємо всі дисципліни в один рядок
                        currentStudent.Performance = string.Join("\n", currentDisciplines);
                        results.Add(currentStudent);
                        currentStudent = null; // Скидаємо для наступного
                    }
                    isMatch = false;
                }
            }
        }

        return results;
    }
}
