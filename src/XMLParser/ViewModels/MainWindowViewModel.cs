using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;
using XMLParser.Models;
using XMLParser.Services;
using XMLParser.Strategies;

namespace XMLParser.ViewModels;

public class MainWindowViewModel : ReactiveObject
{
    private readonly IXmlTransformService _transformService;

    private IDictionary<string, HashSet<string>> _attributeValuesMap =
        new Dictionary<string, HashSet<string>>();

    private string? _generatedQuery;

    private string? _keyword;

    private string? _selectedAttributeName;

    private string? _selectedAttributeValue;

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

        ExitCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var result = await ConfirmExit.Handle("Чи дійсно ви хочете завершити роботу з програмою?");
            if (result)
            {
                // Вигрузку краще робити у View (закриття вікна)
                // наприклад, View підпишеться на ExitCommand і закриє себе
            }
        });

        // this.WhenAnyValue(x => x.SelectedAttributeName)
        //     .Subscribe(name =>
        //     {
        //         AttributeValues.Clear();
        //
        //         if (string.IsNullOrWhiteSpace(name))
        //             return;
        //
        //         if (_attributeValuesMap.TryGetValue(name, out var set))
        //             foreach (var v in set)
        //                 AttributeValues.Add(v);
        //     });
    }

    public ObservableCollection<IXmlSearchStrategy> Strategies { get; } = new();
    public ObservableCollection<SearchResult> Results { get; } = new();
    public ObservableCollection<string> AttributeNames { get; } = new();
    public ObservableCollection<string> AttributeValues { get; } = new();

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
    public ReactiveCommand<Unit, Unit> ExitCommand { get; }

    private void LoadXml()
    {
        if (XmlFilePath is null || SelectedStrategy is null)
            return;

        AttributeNames.Clear();
        AttributeValues.Clear();

        var names = SelectedStrategy.GetAttributeNames(XmlFilePath);
        foreach (var name in names)
            AttributeNames.Add(name);

        _attributeValuesMap = SelectedStrategy.GetAttributeValues(XmlFilePath);
    }

    private void Analyze()
    {
        if (XmlFilePath is null || SelectedStrategy is null)
            return;

        Results.Clear();

        GeneratedQuery = BuildQueryString();

        foreach (var result in SelectedStrategy.Search(
                     XmlFilePath,
                     SelectedAttributeName,
                     SelectedAttributeValue,
                     Keyword))
            Results.Add(result);
    }

    private string BuildQueryString()
    {
        var parts = new List<string> { "/library/book" };

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