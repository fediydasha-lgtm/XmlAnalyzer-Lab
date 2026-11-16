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
            if (studentNode is XmlElement student)
            {
                bool isMatch = true;

             
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
                    var studentData = new StudentData
                    {
                        FullName = student.GetAttribute("FullName"),
                        Faculty = student.GetAttribute("Faculty"),
                        Department = student.GetAttribute("Department"),
                        Course = student.GetAttribute("Course")
                    };

                    List<string> perfList = new List<string>();
                    
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