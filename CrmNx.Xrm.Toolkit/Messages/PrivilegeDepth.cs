using System.Runtime.Serialization;

namespace CrmNx.Xrm.Toolkit.Messages
{
    /// <summary>
    ///     Contains the possible values for the depth of a privilege within a role.
    /// </summary>
    public enum PrivilegeDepth
    {
        /// <summary>
        ///     Indicates basic privileges.
        ///     Users who have basic privileges can only use privileges to perform actions on objects that are owned by, or shared
        ///     with, the user.
        /// </summary>
        [EnumMember] Basic = 0,

        /// <summary>
        ///     Indicates local privileges.
        ///     Users who have local privileges can only use privileges to perform actions on data and objects that are in the
        ///     user's current business unit.
        /// </summary>
        [EnumMember] Local = 1,

        /// <summary>
        ///     Indicates deep privileges.
        ///     Users who have deep privileges can perform actions on all objects in the user's current business units and all
        ///     objects down the hierarchy of business units.
        /// </summary>
        [EnumMember] Deep = 2,

        /// <summary>
        ///     Indicates global privileges.
        ///     Users who have global privileges can perform actions on data and objects anywhere within the organization
        ///     regardless of the business unit or user to which it belongs.
        /// </summary>
        [EnumMember] Global = 3
    }
}