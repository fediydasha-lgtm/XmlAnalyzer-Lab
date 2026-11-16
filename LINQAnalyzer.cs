using System.Xml.Linq;

namespace XmlAnalyzer;

public class LINQAnalyzer : IXmlAnalyzerStrategy
{
    public List<StudentData> Analyze(string filePath, Dictionary<string, string> searchParams)
    {
      
        var results = new List<StudentData>();
        

        var doc = XDocument.Load(filePath);
        var students = doc.Descendants("Student");

       
        foreach (var student in students)
        {
            bool isMatch = true; 

            
            foreach (var param in searchParams)
            {
                string attributeName = param.Key;   
                string expectedValue = param.Value;

               
                var attribute = student.Attribute(attributeName);
                string actualValue = attribute != null ? attribute.Value : "";

                if (actualValue != expectedValue)
                {
                    isMatch = false;
                    break; 
                }
            }

            if (isMatch)
            {
                var studentData = new StudentData
                {
                    FullName = student.Attribute("FullName")?.Value ?? "",
                    Faculty = student.Attribute("Faculty")?.Value ?? "",
                    Department = student.Attribute("Department")?.Value ?? "",
                    Course = student.Attribute("Course")?.Value ?? ""
                };

                
                var disciplines = student.Descendants("Discipline");
                List<string> perfList = new List<string>();
                
                foreach (var disc in disciplines)
                {
                    string name = disc.Attribute("Name")?.Value ?? "";
                    string score = disc.Attribute("Score")?.Value ?? "";
                    perfList.Add($"{name} - {score} балів");
                }
                
               
                studentData.Performance = string.Join("\n", perfList);

                results.Add(studentData);
            }
        }


        return results;
    }
}