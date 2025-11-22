using System.Xml.Xsl;

namespace XMLParser.Services;

public class XslTransformService : IXmlTransformService
{
    public void TransformToHtml(string xmlPath, string xslPath, string outputHtmlPath)
    {
        var transform = new XslCompiledTransform();
        transform.Load(xslPath);
        transform.Transform(xmlPath, outputHtmlPath);
    }
}