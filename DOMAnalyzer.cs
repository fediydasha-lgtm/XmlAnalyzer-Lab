using System.Xml;

namespace XmlAnalyzer;

public class DOMAnalyzer : IXmlAnalyzerStrategy
{
    public List<StudentData> Analyze(string filePath, Dictionary<string, string> searchParams)
    {
        var results = new List<StudentData>();
        
        XmlDocument doc = new XmlDocument();
        doc.Load(filePath);

        XmlNodeList students = doc.GetElementsByTagName("Student");

        foreach (XmlNode studentNode in students)
        {
            // Працюємо ТІЛЬКИ з повноцінними елементами, безпечно приводимо тип
            if (studentNode is XmlElement student)
            {
                bool isMatch = true;

                // Перевіряємо атрибути студента безпечним методом GetAttribute
                foreach (var param in searchParams)
                {
                    if (!student.HasAttribute(param.Key) || student.GetAttribute(param.Key) != param.Value)
                    {
                        isMatch = false;
                        break;
                    }
                }

                if (isMatch)
                {
                    // Створюємо студента
                    var studentData = new StudentData
                    {
                        FullName = student.GetAttribute("FullName"),
                        Faculty = student.GetAttribute("Faculty"),
                        Department = student.GetAttribute("Department"),
                        Course = student.GetAttribute("Course")
                    };

                    List<string> perfList = new List<string>();
                    
                    // GetElementsByTagName шукає всі вкладені теги Discipline на будь-якій глибині без XPath!
                    XmlNodeList disciplines = student.GetElementsByTagName("Discipline");
                    
                    foreach (XmlNode discNode in disciplines)
                    {
                        if (discNode is XmlElement discElement)
                        {
                            string name = discElement.GetAttribute("Name");
                            string score = discElement.GetAttribute("Score");
                            perfList.Add($"{name} - {score} балів");
                        }
                    }

                    studentData.Performance = string.Join("\n", perfList);
                    results.Add(studentData);
                }
            }
        }

        return results;
    }
}