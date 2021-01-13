namespace CrmNx.Xrm.Toolkit.Messages
{
    // public class WhoAmIRequest : IWebApiFunction
    // {
    //     public EntityReference BoundEntity => throw new System.NotImplementedException();
    //
    //     public string RequestName => "WhoAmI";
    //
    //     public IDictionary<string, object> Parameters => throw new System.NotImplementedException();
    // }

    public class WhoAmIRequest : OrganizationRequest<WhoAmIResponse>
    {
        public WhoAmIRequest() : base("WhoAmI", false)
        {
        }
    }
}