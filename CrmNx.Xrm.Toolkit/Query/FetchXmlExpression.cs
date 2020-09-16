using System.Linq;
using System.Xml.Linq;

namespace CrmNx.Xrm.Toolkit.Query
{
    public class FetchXmlExpression
    {
        private readonly XDocument document;
        public string EntityName { get; internal set; }

        public FetchXmlExpression(string fetchXml)
        {
            document = XDocument.Parse(fetchXml);
            EntityName = document
                .Descendants("entity")
                .First()
                .Attribute("name")?.Value;
        }

        public override string ToString()
        {
            return document.ToString(SaveOptions.DisableFormatting);
        }

        //public static implicit operator FetchXmlExpression(string fetchXml)
        //{
        //    return new FetchXmlExpression(fetchXml);
        //}

        public static implicit operator string(FetchXmlExpression fetchXml)
        {
            if (fetchXml == null)
            {
                return string.Empty;
            }

            return fetchXml.ToString();
        }
    }
}
