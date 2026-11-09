using System.Xml.Linq;

namespace XmlAnalyzer;

// Двокрапка означає "Я підписую контракт IXmlAnalyzerStrategy і зобов'язуюсь його виконувати"
public class LINQAnalyzer : IXmlAnalyzerStrategy
{
    public List<StudentData> Analyze(string filePath, Dictionary<string, string> searchParams)
    {
        // Створюємо порожній список, куди будемо складати знайдених студентів
        var results = new List<StudentData>();
        
        // Завантажуємо XML-документ
        var doc = XDocument.Load(filePath);
        var students = doc.Descendants("Student");

        // Перебираємо абсолютно всіх студентів з файлу по черзі
        foreach (var student in students)
        {
            bool isMatch = true; // Спочатку віримо, що цей студент нам підходить

            // Перевіряємо студента за кожним критерієм пошуку, який обрав користувач
            foreach (var param in searchParams)
            {
                string attributeName = param.Key;     // Наприклад: "Faculty"
                string expectedValue = param.Value;   // Наприклад: "ФІОТ"

                // Шукаємо цей атрибут у поточного студента
                var attribute = student.Attribute(attributeName);
                string actualValue = attribute != null ? attribute.Value : "";

                // Якщо значення не збігається — бракуємо цього студента і припиняємо його перевірку
                if (actualValue != expectedValue)
                {
                    isMatch = false;
                    break; 
                }
            }

            // Якщо студент успішно пройшов усі перевірки (isMatch залишилось true)
            if (isMatch)
            {
                // Створюємо нову "картку" студента для нашої таблиці
                var studentData = new StudentData
                {
                    FullName = student.Attribute("FullName")?.Value ?? "",
                    Faculty = student.Attribute("Faculty")?.Value ?? "",
                    Department = student.Attribute("Department")?.Value ?? "",
                    Course = student.Attribute("Course")?.Value ?? ""
                };

                // Тепер витягуємо вкладені дані (дисципліни та оцінки) - це другий рівень XML
                var disciplines = student.Descendants("Discipline");
                List<string> perfList = new List<string>();
                
                foreach (var disc in disciplines)
                {
                    string name = disc.Attribute("Name")?.Value ?? "";
                    string score = disc.Attribute("Score")?.Value ?? "";
                    perfList.Add($"{name} - {score} балів");
                }
                
                // Зліплюємо всі дисципліни в один текст, кожна з нового рядка (\n)
                studentData.Performance = string.Join("\n", perfList);

                // Додаємо готового студента у фінальний список
                results.Add(studentData);
            }
        }

        // Повертаємо список знайдених студентів (як і вимагав контракт!)
        return results;
    }
}