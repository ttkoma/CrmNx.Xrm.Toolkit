namespace CrmNx.Xrm.Toolkit.Messages;

public class RetrieveTimestampRequest : OrganizationRequest<RetrieveTimestampResponse>
{
    public RetrieveTimestampRequest() : base("RetrieveTimestamp", isWebApiAction: false)
    {
    }
}