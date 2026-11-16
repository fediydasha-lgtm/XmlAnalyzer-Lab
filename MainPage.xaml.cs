using System.Xml.Linq;
using XmlAnalyzer;
using Microsoft.Maui.Platform;
#if MACCATALYST
using UIKit;
using Foundation;
#endif
namespace XmlAnalyzer;

public partial class MainPage : ContentPage
{
    private string _currentFilePath = string.Empty;

    public MainPage()
    {
        InitializeComponent();
    }

 private async void OnLoadFileClicked(object sender, EventArgs e)
{
    try
    {
        using Stream stream = await FileSystem.OpenAppPackageFileAsync("StudentsData.xml");
        
        string targetFile = Path.Combine(FileSystem.CacheDirectory, "StudentsData_temp.xml");
        
        if (File.Exists(targetFile))
        {
            File.Delete(targetFile);
        }

        using (FileStream outputStream = File.Create(targetFile))
        {
            await stream.CopyToAsync(outputStream);
        }

        _currentFilePath = targetFile;
        LoadAttributesIntoPickers(_currentFilePath);

        await DisplayAlert("Успіх", "Файл успішно завантажено!", "OK");
    }
    catch (Exception ex)
    {
        await DisplayAlert("Помилка", $"Щось пішло не так: {ex.Message}", "OK");
    }
}
    
    private void LoadAttributesIntoPickers(string filePath)
    {
        var doc = XDocument.Load(filePath);

        var students = doc.Descendants("Student");

        var fullNames = students
            .Select(s => s.Attribute("FullName")?.Value)
            .Where(val => !string.IsNullOrEmpty(val))
            .Distinct()
            .ToList();

        var faculties = students
            .Select(s => s.Attribute("Faculty")?.Value)
            .Where(val => !string.IsNullOrEmpty(val))
            .Distinct()
            .ToList();

        var departments = students
            .Select(s => s.Attribute("Department")?.Value)
            .Where(val => !string.IsNullOrEmpty(val))
            .Distinct()
            .ToList();

        var courses = students
            .Select(s => s.Attribute("Course")?.Value)
            .Where(val => !string.IsNullOrEmpty(val))
            .Distinct()
            .ToList();

        fullNames.Insert(0, "Усі");
        faculties.Insert(0, "Усі");
        departments.Insert(0, "Усі");
        courses.Insert(0, "Усі");

        PickerFullName.ItemsSource = fullNames;
        PickerFaculty.ItemsSource = faculties;
        PickerDepartment.ItemsSource = departments;
        PickerCourse.ItemsSource = courses;

        PickerFullName.SelectedIndex = 0;
        PickerFaculty.SelectedIndex = 0;
        PickerDepartment.SelectedIndex = 0;
        PickerCourse.SelectedIndex = 0;
    }
    private void OnSearchClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_currentFilePath))
        {
            DisplayAlert("Увага", "Спочатку завантажте XML файл!", "OK");
            return; 
        }

        var searchParams = new Dictionary<string, string>();

        if (PickerFullName.SelectedItem != null && PickerFullName.SelectedItem.ToString() != "Усі")
            searchParams.Add("FullName", PickerFullName.SelectedItem.ToString());
            
        if (PickerFaculty.SelectedItem != null && PickerFaculty.SelectedItem.ToString() != "Усі")
            searchParams.Add("Faculty", PickerFaculty.SelectedItem.ToString());
            
        if (PickerDepartment.SelectedItem != null && PickerDepartment.SelectedItem.ToString() != "Усі")
            searchParams.Add("Department", PickerDepartment.SelectedItem.ToString());
            
        if (PickerCourse.SelectedItem != null && PickerCourse.SelectedItem.ToString() != "Усі")
            searchParams.Add("Course", PickerCourse.SelectedItem.ToString());

      
        IXmlAnalyzerStrategy currentStrategy = null;

    if (RadioLINQ.IsChecked)
    {
        currentStrategy = new LINQAnalyzer(); 
    }
    else if (RadioSAX.IsChecked)
    {
        currentStrategy = new SAXAnalyzer(); 
    }
    else if (RadioDOM.IsChecked)
    {
        currentStrategy = new DOMAnalyzer(); 
    }

        var results = currentStrategy.Analyze(_currentFilePath, searchParams);
string logParams = string.Join(", ", searchParams.Select(p => $"«{p.Key}» - {p.Value}"));
if (string.IsNullOrEmpty(logParams)) logParams = "Усі";

AppLogger.Instance.Log("Фільтрація", $"Параметри: {logParams}");
       
        StudentsCollectionView.ItemsSource = results;
    }

    private void OnClearClicked(object sender, EventArgs e)
    {
        PickerFullName.SelectedIndex = 0;
        PickerFaculty.SelectedIndex = 0;
        PickerDepartment.SelectedIndex = 0;
        PickerCourse.SelectedIndex = 0;

        StudentsCollectionView.ItemsSource = null;
    }
   private async void OnSaveToDriveClicked(object sender, EventArgs e)
{
    if (string.IsNullOrEmpty(_currentFilePath))
    {
        await DisplayAlert("Увага", "Спочатку завантажте XML файл!", "OK");
        return;
    }

    try
    {
    
        var doc = System.Xml.Linq.XDocument.Load(_currentFilePath);
        var students = doc.Descendants("Student").ToList();

        var searchParams = new Dictionary<string, string>();
        if (PickerFullName.SelectedItem != null && PickerFullName.SelectedItem.ToString() != "Усі")
            searchParams.Add("FullName", PickerFullName.SelectedItem.ToString());
        if (PickerFaculty.SelectedItem != null && PickerFaculty.SelectedItem.ToString() != "Усі")
            searchParams.Add("Faculty", PickerFaculty.SelectedItem.ToString());
        if (PickerDepartment.SelectedItem != null && PickerDepartment.SelectedItem.ToString() != "Усі")
            searchParams.Add("Department", PickerDepartment.SelectedItem.ToString());
        if (PickerCourse.SelectedItem != null && PickerCourse.SelectedItem.ToString() != "Усі")
            searchParams.Add("Course", PickerCourse.SelectedItem.ToString());

        foreach (var student in students)
        {
            bool isMatch = true;
            foreach (var param in searchParams)
            {
                if (student.Attribute(param.Key)?.Value != param.Value)
                {
                    isMatch = false;
                    break;
                }
            }
            if (!isMatch)
            {
                student.Remove();
            }
        }

        string filteredXmlPath = Path.Combine(FileSystem.CacheDirectory, "FilteredStudents.xml");
        doc.Save(filteredXmlPath);

        string format = await DisplayActionSheet("Оберіть формат для Google Drive", "Скасувати", null, "XML", "HTML");
        if (format == "Скасувати" || string.IsNullOrEmpty(format)) return;

        string sourceFile = filteredXmlPath;


        if (format == "HTML")
        {
            string xsltPath = Path.Combine(FileSystem.CacheDirectory, "Transform_temp.xslt");
            using (Stream xsltStream = await FileSystem.OpenAppPackageFileAsync("Transform.xslt"))
            using (FileStream fs = File.Create(xsltPath))
            {
                await xsltStream.CopyToAsync(fs);
            }

            string htmlPath = Path.Combine(FileSystem.CacheDirectory, "StudentsResult.html");
            var transform = new System.Xml.Xsl.XslCompiledTransform();
            transform.Load(xsltPath);
            transform.Transform(filteredXmlPath, htmlPath);
            sourceFile = htmlPath;
        }

        SaverFactory factory = new SaverFactory();
        IDataSaver saver = factory.CreateSaver(format);
        saver.SaveData(sourceFile);

        await DisplayAlert("Успіх", $"Файл для {PickerFullName.SelectedItem} успішно збережено у форматі {format}!", "OK");
    }
    catch (Exception ex)
    {
        await DisplayAlert("Помилка", $"Помилка збереження: {ex.Message}", "OK");
    }
}

    private void OnExitClicked(object sender, EventArgs e)
{
    Application.Current.Quit();
}

    private async void OnTransformClicked(object sender, EventArgs e)
{
    if (string.IsNullOrEmpty(_currentFilePath))
    {
        await DisplayAlert("Увага", "Спочатку завантажте XML файл!", "OK");
        return;
    }

    try
    {
        var doc = System.Xml.Linq.XDocument.Load(_currentFilePath);
        var students = doc.Descendants("Student").ToList();

        var searchParams = new Dictionary<string, string>();
        if (PickerFullName.SelectedItem != null && PickerFullName.SelectedItem.ToString() != "Усі")
            searchParams.Add("FullName", PickerFullName.SelectedItem.ToString());
        if (PickerFaculty.SelectedItem != null && PickerFaculty.SelectedItem.ToString() != "Усі")
            searchParams.Add("Faculty", PickerFaculty.SelectedItem.ToString());
        if (PickerDepartment.SelectedItem != null && PickerDepartment.SelectedItem.ToString() != "Усі")
            searchParams.Add("Department", PickerDepartment.SelectedItem.ToString());
        if (PickerCourse.SelectedItem != null && PickerCourse.SelectedItem.ToString() != "Усі")
            searchParams.Add("Course", PickerCourse.SelectedItem.ToString());

        foreach (var student in students)
        {
            bool isMatch = true;
            foreach (var param in searchParams)
            {
                if (student.Attribute(param.Key)?.Value != param.Value)
                {
                    isMatch = false;
                    break;
                }
            }
            if (!isMatch)
            {
                student.Remove();
            }
        }

    
        string filteredXmlPath = Path.Combine(FileSystem.CacheDirectory, "FilteredStudents.xml");
        doc.Save(filteredXmlPath);


        using Stream xsltStream = await FileSystem.OpenAppPackageFileAsync("Transform.xslt");
        string xsltPath = Path.Combine(FileSystem.CacheDirectory, "Transform_temp.xslt");
        using (FileStream fs = File.Create(xsltPath))
        {
            await xsltStream.CopyToAsync(fs);
        }
  
        string htmlPath = Path.Combine(FileSystem.CacheDirectory, "StudentsResult.html");

        System.Xml.Xsl.XslCompiledTransform transform = new System.Xml.Xsl.XslCompiledTransform();
        transform.Load(xsltPath);
        transform.Transform(filteredXmlPath, htmlPath);
        
        AppLogger.Instance.Log("Трансформація", "Збережено у файл StudentsResult.html");

        await Launcher.Default.OpenAsync(new OpenFileRequest("HTML", new ReadOnlyFile(htmlPath)));
    }
    catch (Exception ex)
    {
        await DisplayAlert("Помилка", $"Помилка трансформації: {ex.Message}", "OK");
    }

}
}