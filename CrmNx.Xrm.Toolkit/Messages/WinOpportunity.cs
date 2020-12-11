namespace CrmNx.Xrm.Toolkit.Messages
{
    /// <summary>
    /// Sets the state of an opportunity to Won.
    /// </summary>
    public class WinOpportunity : WebApiActionBase
    {
        private const string ActionName = "WinOpportunity";
        private const string CallerParameterName = "Caller";
        private const string StatusParameterName = "Status";
        private const string OpportunityCloseParameterName = "OpportunityClose";

        public string Caller
        {
            get => Parameters[CallerParameterName] as string;
            set => Parameters[CallerParameterName] = value;
        }
        
        public int Status
        {
            get => Parameters.ContainsKey(StatusParameterName) ? (int)Parameters[StatusParameterName] : default;
            set => Parameters[StatusParameterName] = value;
        }

        public Entity OpportunityClose
        {
            get => Parameters[OpportunityCloseParameterName] as Entity;
            set => Parameters[OpportunityCloseParameterName] = value;
        }
        
        public WinOpportunity() : base(ActionName) { }
    }
}