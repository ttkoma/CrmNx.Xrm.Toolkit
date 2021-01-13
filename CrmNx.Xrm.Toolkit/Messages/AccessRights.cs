using System;
using System.Runtime.Serialization;

namespace CrmNx.Xrm.Toolkit.Messages
{
    /// <summary>
    ///     Contains the possible access rights for a user.
    /// </summary>
    [Flags]
    public enum AccessRights
    {
        /// <summary>
        ///     No access.
        /// </summary>
        [EnumMember] None = 0,

        /// <summary>
        ///     The right to read the specified type of record.
        /// </summary>
        [EnumMember] ReadAccess = 1,

        /// <summary>
        ///     The right to update the specified record.
        /// </summary>
        [EnumMember] WriteAccess = 2,

        /// <summary>
        ///     The right to append the specified record to another object.
        /// </summary>
        [EnumMember] AppendAccess = 4,

        /// <summary>
        ///     The right to append another record to the specified object.
        /// </summary>
        [EnumMember] AppendToAccess = 16,

        /// <summary>
        ///     The right to create a record.
        /// </summary>
        [EnumMember] CreateAccess = 32,

        /// <summary>
        ///     The right to delete the specified record.
        /// </summary>
        [EnumMember] DeleteAccess = 65536,

        /// <summary>
        ///     The right to share the specified record.
        /// </summary>
        [EnumMember] ShareAccess = 262144,

        /// <summary>
        ///     The right to assign the specified record to another user or team.
        /// </summary>
        [EnumMember] AssignAccess = 524288
    }
}