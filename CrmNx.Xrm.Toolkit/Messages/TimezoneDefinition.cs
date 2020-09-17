using System;
using Newtonsoft.Json;

namespace CrmNx.Xrm.Toolkit.Messages
{
    /// <summary>
    /// Определение часого пояса
    /// </summary>
    public class TimezoneDefinition
    {
        /// <summary>
        /// UNique identifier
        /// </summary>
        [JsonProperty("timezonedefinitionid")]
        public Guid TimezoneDefinitioId { get; set; }

        /// <summary>
        /// Localized TimeZone name
        /// </summary>
        [JsonProperty("userinterfacename")]
        public string UserInterfaceName { get; set; }

        /// <summary>
        /// TimeZone Crm code
        /// </summary>
        [JsonProperty("timezonecode")]
        public int TimezoneCode { get; set; }

        /// <summary>
        /// Bias offset in minutes
        /// </summary>
        [JsonProperty("bias")]
        public int Bias { get; set; }
    }
}