using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using ReactiveUI;
using XMLParser.Services;
using XMLParser.Strategies;

namespace XMLParser.ViewModels;

public class MainWindowViewModel : ReactiveObject
{
    private readonly string _credentialsJsonPath = "credentials.json";
    private readonly IXmlTransformService _transformService;

    private IDictionary<string, HashSet<string>> _attributeValuesMap =
        new Dictionary<string, HashSet<string>>();

    private string? _generatedQuery;

    private string? _keyword;

    private string? _lastHtmlUrl;

    private string? _selectedAttributeName;

    private string? _selectedAttributeValue;

    private string _selectedFormat = "xml";

    private IXmlSearchStrategy? _selectedStrategy;

    private string? _xmlFilePath;

    private string? _xslFilePath;

    public MainWindowViewModel()
    {
        _transformService = new XslTransformService();

        Strategies.Add(new SaxXmlSearchStrategy());
        Strategies.Add(new DomXmlSearchStrategy());
        Strategies.Add(new LinqXmlSearchStrategy());
        SelectedStrategy = Strategies.FirstOrDefault();

        LoadXmlCommand = ReactiveCommand.Create(LoadXml);
        AnalyzeCommand = ReactiveCommand.Create(Analyze,
            this.WhenAnyValue(x => x.XmlFilePath, x => x.SelectedStrategy,
                (path, strategy) => !string.IsNullOrWhiteSpace(path) && strategy is not null));

        TransformCommand = ReactiveCommand.Create(Transform,
            this.WhenAnyValue(x => x.XmlFilePath, x => x.XslFilePath,
                (xml, xsl) => !string.IsNullOrWhiteSpace(xml) && !string.IsNullOrWhiteSpace(xsl)));

        ClearCommand = ReactiveCommand.Create(Clear);

        SaveFilteredCommand = ReactiveCommand.CreateFromTask(SaveFilteredAsync);
        
        this.WhenAnyValue(x => x.SelectedAttributeName)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(name =>
            {
                AttributeValues.Clear();

                if (string.IsNullOrWhiteSpace(name))
                    return;

                if (_attributeValuesMap.TryGetValue(name, out var set))
                    foreach (var v in set)
                        AttributeValues.Add(v);
            });
    }

    public ReactiveCommand<Unit, Unit> SaveFilteredCommand { get; }

    public string SelectedFormat
    {
        get => _selectedFormat;
        set => this.RaiseAndSetIfChanged(ref _selectedFormat, value);
    }

    public ObservableCollection<IXmlSearchStrategy> Strategies { get; } = new();
    public ObservableCollection<SearchResultViewModel> Results { get; } = new();
    public ObservableCollection<string> AttributeNames { get; } = new();
    public ObservableCollection<string> AttributeValues { get; } = new();
    public ObservableCollection<string> FileFormatComboBoxItems { get; } = ["xml", "html"];

    public IXmlSearchStrategy? SelectedStrategy
    {
        get => _selectedStrategy;
        set => this.RaiseAndSetIfChanged(ref _selectedStrategy, value);
    }

    public string? XmlFilePath
    {
        get => _xmlFilePath;
        set => this.RaiseAndSetIfChanged(ref _xmlFilePath, value);
    }

    public string? XslFilePath
    {
        get => _xslFilePath;
        set => this.RaiseAndSetIfChanged(ref _xslFilePath, value);
    }

    public string? SelectedAttributeName
    {
        get => _selectedAttributeName;
        set => this.RaiseAndSetIfChanged(ref _selectedAttributeName, value);
    }

    public string? SelectedAttributeValue
    {
        get => _selectedAttributeValue;
        set => this.RaiseAndSetIfChanged(ref _selectedAttributeValue, value);
    }

    public string? Keyword
    {
        get => _keyword;
        set => this.RaiseAndSetIfChanged(ref _keyword, value);
    }

    public string? GeneratedQuery
    {
        get => _generatedQuery;
        set => this.RaiseAndSetIfChanged(ref _generatedQuery, value);
    }

    public Interaction<string, bool> ConfirmExit { get; } = new();

    public ReactiveCommand<Unit, Unit> LoadXmlCommand { get; }
    public ReactiveCommand<Unit, Unit> AnalyzeCommand { get; }
    public ReactiveCommand<Unit, Unit> TransformCommand { get; }
    public ReactiveCommand<Unit, Unit> ClearCommand { get; }

    public string? LastHtmlUrl
    {
        get => _lastHtmlUrl;
        set => this.RaiseAndSetIfChanged(ref _lastHtmlUrl, value);
    }

    private string BuildFilteredFragment()
    {
        var doc = new XElement("FilteredScientists");

        foreach (var vm in Results)
        {
            var el = new XElement("Scientist");
            foreach (var kv in vm.Attributes)
                el.Add(new XElement(kv.Key, kv.Value ?? string.Empty));
            
            if (!string.IsNullOrWhiteSpace(vm.TextContent))
                el.Add(new XElement("TextContent", vm.TextContent));

            doc.Add(el);
        }

        return new XDocument(doc).ToString();
    }

    private async Task SaveFilteredAsync()
    {
        var extension = SelectedFormat;
        Console.WriteLine(extension);
        try
        {
            var fragment = BuildFilteredFragment();

            AppLogger.Instance.LogEvent(AppLogger.EventType.Filtering,
                $"Підготовлено фрагмент. Рядків: {Results.Count}");

            var saver = FilteredDataSaver.CreateSaver(extension, _credentialsJsonPath);

            var uploadedId = await saver.SaveToDriveAsync(fragment);

            AppLogger.Instance.LogEvent(AppLogger.EventType.Saving,
                $"Збережено фрагмент у форматі {extension}. DriveId={uploadedId}");
        }
        catch (Exception ex)
        {
            AppLogger.Instance.LogEvent(AppLogger.EventType.Saving,
                $"Помилка при збереженні: {ex.Message}");
            throw;
        }
    }

    private void LoadXml()
    {
        if (XmlFilePath is null || SelectedStrategy is null)
            return;

        AttributeNames.Clear();
        AttributeValues.Clear();

        var names = SelectedStrategy.GetAttributeNames(XmlFilePath, "Scientist");
        foreach (var name in names)
            AttributeNames.Add(name);

        _attributeValuesMap = SelectedStrategy.GetAttributeValues(XmlFilePath, "Scientist");
    }

    private void Analyze()
    {
        if (XmlFilePath is null || SelectedStrategy is null)
            return;

        Results.Clear();

        GeneratedQuery = BuildQueryString();

        foreach (var result in SelectedStrategy.Search(
                     XmlFilePath,
                     "Scientist",
                     SelectedAttributeName,
                     SelectedAttributeValue,
                     Keyword))
            Results.Add(new SearchResultViewModel(result));
    }


    private string BuildQueryString()
    {
        var parts = new List<string> { "/scientists/scientist" };

        if (!string.IsNullOrWhiteSpace(SelectedAttributeName) &&
            !string.IsNullOrWhiteSpace(SelectedAttributeValue))
            parts.Add($"[@{SelectedAttributeName}='{SelectedAttributeValue}']");

        if (!string.IsNullOrWhiteSpace(Keyword)) parts.Add($"[contains(., '{Keyword}')]");

        return string.Join("", parts);
    }

    private void Transform()
    {
        if (XmlFilePath is null || XslFilePath is null)
            return;

        var outputPath = Path.Combine(
            Path.GetDirectoryName(XmlFilePath)!,
            "output.html");

        _transformService.TransformToHtml(XmlFilePath, XslFilePath, outputPath);
        LastHtmlUrl = outputPath;
        GeneratedQuery = $"HTML збережено у: {outputPath}";
    }

    private void Clear()
    {
        Results.Clear();
        Keyword = string.Empty;
        SelectedAttributeName = null;
        SelectedAttributeValue = null;
        GeneratedQuery = string.Empty;
    }
}