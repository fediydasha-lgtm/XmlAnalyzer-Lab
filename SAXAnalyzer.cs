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

        using (XmlReader reader = XmlReader.Create(filePath))
        {
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "Student")
                {
                   
                    string fullName = reader.GetAttribute("FullName") ?? "";
                    string faculty = reader.GetAttribute("Faculty") ?? "";
                    string department = reader.GetAttribute("Department") ?? "";
                    string course = reader.GetAttribute("Course") ?? "";

                   
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

                    
                    if (isMatch)
                    {
                        currentStudent = new StudentData
                        {
                            FullName = fullName,
                            Faculty = faculty,
                            Department = department,
                            Course = course
                        };
                        currentDisciplines.Clear(); 
                    }
                }

                if (isMatch && reader.NodeType == XmlNodeType.Element && reader.Name == "Discipline")
                {
                    string name = reader.GetAttribute("Name") ?? "";
                    string score = reader.GetAttribute("Score") ?? "";
                    currentDisciplines.Add($"{name} - {score} балів");
                }

                if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "Student" && isMatch)
                {
                    if (currentStudent != null)
                    {
                        currentStudent.Performance = string.Join("\n", currentDisciplines);
                        results.Add(currentStudent);
                        currentStudent = null;
                    }
                    isMatch = false;
                }
            }
        }

        return results;
    }
}
