namespace CrmNx.Xrm.Toolkit.Messages
{
    /// <summary>
    ///     Sets the state of an opportunity to Won.
    /// </summary>
    public class WinOpportunityRequest : OrganizationRequest<object>
    {
        private const string WebApiRequestName = "WinOpportunity";

        public WinOpportunityRequest() : base(WebApiRequestName, true)
        {
        }

        public string Caller
        {
            get => Parameters.ContainsKey(nameof(Caller)) ? (string) Parameters[nameof(Caller)] : null;
            set => Parameters[nameof(Caller)] = value;
        }

        public int Status
        {
            get => Parameters.ContainsKey(nameof(Status)) ? (int) Parameters[nameof(Status)] : -1;
            set => Parameters[nameof(Status)] = value;
        }

        public Entity OpportunityClose
        {
            get => Parameters.ContainsKey(nameof(OpportunityClose))
                ? (Entity) Parameters[nameof(OpportunityClose)]
                : null;
            set => Parameters[nameof(OpportunityClose)] = value;
        }
    }
}