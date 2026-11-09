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
    // Тут ми будемо зберігати шлях до завантаженого файлу
    private string _currentFilePath = string.Empty;

    public MainPage()
    {
        InitializeComponent();
    }

    // -------------------------------------------------------------
    // КРОК 1: Кнопка "Завантажити XML"
    // -------------------------------------------------------------

 private async void OnLoadFileClicked(object sender, EventArgs e)
{
    try
    {
        // Безпосередньо читаємо файл з ресурсів програми у вигляді потоку без зайвого копіювання в кеш
        using Stream stream = await FileSystem.OpenAppPackageFileAsync("StudentsData.xml");
        
        // Якщо твій метод LoadAttributesIntoPickers приймає шлях до файлу (string), 
        // то ми можемо зберегти його у тимчасовий файл унікальним способом або напряму передати шлях ресурсу:
        string targetFile = Path.Combine(FileSystem.CacheDirectory, "StudentsData_temp.xml");
        
        if (File.Exists(targetFile))
        {
            File.Delete(targetFile); // Видаляємо старий заблокований файл, якщо він залишився
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
    // -------------------------------------------------------------
    // КРОК 2: Динамічне зчитування атрибутів для заповнення списків (Pickers)
    // -------------------------------------------------------------
    private void LoadAttributesIntoPickers(string filePath)
    {
        // Завантажуємо XML-документ у пам'ять за допомогою LINQ to XML
        var doc = XDocument.Load(filePath);

        // Отримуємо всі вузли <Student>
        var students = doc.Descendants("Student");

        // 1. Збираємо всі унікальні П.І.П.
        var fullNames = students
            .Select(s => s.Attribute("FullName")?.Value)
            .Where(val => !string.IsNullOrEmpty(val))
            .Distinct()
            .ToList();

        // 2. Збираємо всі унікальні Факультети
        var faculties = students
            .Select(s => s.Attribute("Faculty")?.Value)
            .Where(val => !string.IsNullOrEmpty(val))
            .Distinct()
            .ToList();

        // 3. Збираємо всі унікальні Кафедри
        var departments = students
            .Select(s => s.Attribute("Department")?.Value)
            .Where(val => !string.IsNullOrEmpty(val))
            .Distinct()
            .ToList();

        // 4. Збираємо всі унікальні Курси
        var courses = students
            .Select(s => s.Attribute("Course")?.Value)
            .Where(val => !string.IsNullOrEmpty(val))
            .Distinct()
            .ToList();

        // Додаємо варіант "Усі", щоб користувач міг скинути вибір у будь-якому списку
        fullNames.Insert(0, "Усі");
        faculties.Insert(0, "Усі");
        departments.Insert(0, "Усі");
        courses.Insert(0, "Усі");

        // Прив'язуємо списки до наших елементів UI у MainPage.xaml
        PickerFullName.ItemsSource = fullNames;
        PickerFaculty.ItemsSource = faculties;
        PickerDepartment.ItemsSource = departments;
        PickerCourse.ItemsSource = courses;

        // Встановлюємо значення за замовчуванням ("Усі")
        PickerFullName.SelectedIndex = 0;
        PickerFaculty.SelectedIndex = 0;
        PickerDepartment.SelectedIndex = 0;
        PickerCourse.SelectedIndex = 0;
    }
    // -------------------------------------------------------------
    // КРОК 3: Кнопка "Пошук (Фільтрація)" - Використання Стратегії
    // -------------------------------------------------------------
    private void OnSearchClicked(object sender, EventArgs e)
    {
        // 1. Перевіряємо, чи користувач взагалі завантажив файл
        if (string.IsNullOrEmpty(_currentFilePath))
        {
            DisplayAlert("Увага", "Спочатку завантажте XML файл!", "OK");
            return; // Зупиняємо виконання, далі йти немає сенсу
        }

        // 2. Збираємо параметри пошуку в "словничок" (Dictionary)
        var searchParams = new Dictionary<string, string>();

        // Якщо вибрано НЕ "Усі" (і список не порожній), додаємо критерій для пошуку
        if (PickerFullName.SelectedItem != null && PickerFullName.SelectedItem.ToString() != "Усі")
            searchParams.Add("FullName", PickerFullName.SelectedItem.ToString());
            
        if (PickerFaculty.SelectedItem != null && PickerFaculty.SelectedItem.ToString() != "Усі")
            searchParams.Add("Faculty", PickerFaculty.SelectedItem.ToString());
            
        if (PickerDepartment.SelectedItem != null && PickerDepartment.SelectedItem.ToString() != "Усі")
            searchParams.Add("Department", PickerDepartment.SelectedItem.ToString());
            
        if (PickerCourse.SelectedItem != null && PickerCourse.SelectedItem.ToString() != "Усі")
            searchParams.Add("Course", PickerCourse.SelectedItem.ToString());

        // 3. Визначаємо, який метод пошуку (яку Стратегію) обрав користувач
        IXmlAnalyzerStrategy currentStrategy = null;

        if (RadioLINQ.IsChecked)
        {
            currentStrategy = new LINQAnalyzer(); // Вставляємо картридж LINQ
        }
        else if (RadioSAX.IsChecked)
{
    currentStrategy = new SAXAnalyzer(); 
}
       else if (RadioDOM.IsChecked)
{
    currentStrategy = new DOMAnalyzer(); 
}
        // 4. МАГІЯ ПАТЕРНУ СТРАТЕГІЯ
        // Вікно не пише логіку пошуку! Воно просто передає файл і параметри обраному аналізатору:
        var results = currentStrategy.Analyze(_currentFilePath, searchParams);
string logParams = string.Join(", ", searchParams.Select(p => $"«{p.Key}» - {p.Value}"));
if (string.IsNullOrEmpty(logParams)) logParams = "Усі";

AppLogger.Instance.Log("Фільтрація", $"Параметри: {logParams}");
        // 5. Виводимо знайдені результати в таблицю на екран
        StudentsCollectionView.ItemsSource = results;
    }
    // -------------------------------------------------------------
    // КРОК 4: Кнопка "Очистити (Clear)"
    // -------------------------------------------------------------
    private void OnClearClicked(object sender, EventArgs e)
    {
        // 1. Скидаємо всі параметри пошуку на значення за замовчуванням ("Усі" - це індекс 0)
        PickerFullName.SelectedIndex = 0;
        PickerFaculty.SelectedIndex = 0;
        PickerDepartment.SelectedIndex = 0;
        PickerCourse.SelectedIndex = 0;

        // 2. Очищаємо таблицю результатів на екрані
        StudentsCollectionView.ItemsSource = null;
    }
    // Заглушки для методів, яких не вистачало
   private async void OnSaveToDriveClicked(object sender, EventArgs e)
{
    if (string.IsNullOrEmpty(_currentFilePath))
    {
        await DisplayAlert("Увага", "Спочатку завантажте XML файл!", "OK");
        return;
    }

    try
    {
        // 1. Формуємо відфільтрований XML на основі ПОТОЧНО обраного студента
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

        // 2. Запитуємо формат
        string format = await DisplayActionSheet("Оберіть формат для Google Drive", "Скасувати", null, "XML", "HTML");
        if (format == "Скасувати" || string.IsNullOrEmpty(format)) return;

        string sourceFile = filteredXmlPath;

        // Якщо вибрано HTML — генеруємо актуальний HTML для вибраного студента
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

        // 3. Зберігаємо через Фабрику
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
    // Завершує роботу всього додатку
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
        // --- НОВИЙ БЛОК: Створюємо відфільтрований XML ---
        var doc = System.Xml.Linq.XDocument.Load(_currentFilePath);
        var students = doc.Descendants("Student").ToList();

        // Збираємо параметри, які зараз обрані у випадаючих списках
        var searchParams = new Dictionary<string, string>();
        if (PickerFullName.SelectedItem != null && PickerFullName.SelectedItem.ToString() != "Усі")
            searchParams.Add("FullName", PickerFullName.SelectedItem.ToString());
        if (PickerFaculty.SelectedItem != null && PickerFaculty.SelectedItem.ToString() != "Усі")
            searchParams.Add("Faculty", PickerFaculty.SelectedItem.ToString());
        if (PickerDepartment.SelectedItem != null && PickerDepartment.SelectedItem.ToString() != "Усі")
            searchParams.Add("Department", PickerDepartment.SelectedItem.ToString());
        if (PickerCourse.SelectedItem != null && PickerCourse.SelectedItem.ToString() != "Усі")
            searchParams.Add("Course", PickerCourse.SelectedItem.ToString());

        // Видаляємо з XML тих студентів, які не підходять під твій фільтр
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
                student.Remove(); // Видаляємо вузол з копії документа
            }
        }

        // Зберігаємо відфільтрований XML у тимчасовий файл
        string filteredXmlPath = Path.Combine(FileSystem.CacheDirectory, "FilteredStudents.xml");
        doc.Save(filteredXmlPath);
        // -------------------------------------------------

        // 1. Витягуємо XSLT-файл
        using Stream xsltStream = await FileSystem.OpenAppPackageFileAsync("Transform.xslt");
        string xsltPath = Path.Combine(FileSystem.CacheDirectory, "Transform_temp.xslt");
        using (FileStream fs = File.Create(xsltPath))
        {
            await xsltStream.CopyToAsync(fs);
        }

        // 2. Вказуємо шлях для фінального HTML
        string htmlPath = Path.Combine(FileSystem.CacheDirectory, "StudentsResult.html");

        // 3. Робимо трансформацію (використовуємо ВІДФІЛЬТРОВАНИЙ файл)
        System.Xml.Xsl.XslCompiledTransform transform = new System.Xml.Xsl.XslCompiledTransform();
        transform.Load(xsltPath);
        transform.Transform(filteredXmlPath, htmlPath);
        
        AppLogger.Instance.Log("Трансформація", "Збережено у файл StudentsResult.html");

        // 4. Відкриваємо готовий HTML-файл у браузері
        await Launcher.Default.OpenAsync(new OpenFileRequest("HTML", new ReadOnlyFile(htmlPath)));
    }
    catch (Exception ex)
    {
        await DisplayAlert("Помилка", $"Помилка трансформації: {ex.Message}", "OK");
    }

}
}