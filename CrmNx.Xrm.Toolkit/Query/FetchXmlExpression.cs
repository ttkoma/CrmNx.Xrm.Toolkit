using System;
using System.Linq;
using System.Xml.Linq;

namespace CrmNx.Xrm.Toolkit.Query
{
    public class FetchXmlExpression
    {
        private readonly XDocument _document;
        public string EntityName { get; internal set; }

        public FetchXmlExpression(string fetchXml, bool includeAnnotations = true)
        {
            _document = XDocument.Parse(fetchXml);
            EntityName = _document
                .Descendants("entity")
                .First()
                .Attribute("name")?.Value;

            IncludeAnnotations = includeAnnotations;
            
            
        }
        public bool? Aggregate
        {
            get
            {
                var attr = _document.Root?.Attribute("aggregate");
                return attr != null && Convert.ToBoolean(attr.Value);
            }
            set => _document.Root?.SetAttributeValue("aggregate", value);
        }

        public int? Page
        {
            get
            {
                var attr = _document.Root?.Attribute("page");
                return attr != null ? Convert.ToInt32(attr.Value) : null;
            }
            set => _document.Root?.SetAttributeValue("page", value);
        }

        public bool? NoLock
        {
            get
            {
                var attr = _document.Root?.Attribute("no-lock");
                return attr != null && Convert.ToBoolean(attr.Value);
            }
            set => _document.Root?.SetAttributeValue("no-lock", value);
        }

        public int? Count
        {
            get
            {
                var attr = _document.Root?.Attribute("count");
                return attr != null ? Convert.ToInt32(attr.Value) : null;
            }
            set => _document.Root?.SetAttributeValue("count", value);
        }

        public string PagingCookie
        {
            get => _document.Root?.Attribute("paging-cookie")?.Value;
            set => _document.Root?.SetAttributeValue("paging-cookie", value);
        }

        public bool IncludeAnnotations { get; set; }

        public override string ToString()
        {
            return _document.ToString(SaveOptions.DisableFormatting);
        }

        public static implicit operator string(FetchXmlExpression fetchXml)
        {
            return fetchXml == null ? string.Empty : fetchXml.ToString();
        }
    }
}