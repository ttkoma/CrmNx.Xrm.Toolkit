namespace CrmNx.Xrm.Toolkit.Messages
{
    public class WhoAmIRequest : IWebApiFunction
    {
        public string QueryString() => "WhoAmI";
    }
}