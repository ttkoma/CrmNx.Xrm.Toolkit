namespace CrmNx.Xrm.Toolkit.Messages
{
    public class WhoAmIRequest : IWebApiFunction
    {
        private const string Query = "WhoAmI";

        public string QueryString()
        {
            return Query;
        }
    }
}