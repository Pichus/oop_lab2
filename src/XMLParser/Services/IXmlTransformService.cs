namespace XMLParser.Services;

public interface IXmlTransformService
{
    void TransformToHtml(string xmlPath, string xslPath, string outputHtmlPath);
}