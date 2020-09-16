namespace CrmNx.Xrm.Toolkit.Messages
{
    public class WhoAmIRequest : IWebApiFunction
    {
        private const string Query = "WhoAmI";

        public string ToQueryString()
        {
            return Query;
        }
    }
}
