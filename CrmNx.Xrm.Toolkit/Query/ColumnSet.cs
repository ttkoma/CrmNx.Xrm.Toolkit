using System.Collections.Generic;

namespace CrmNx.Xrm.Toolkit.Query
{
    public class ColumnSet
    {
        /// <summary>
        /// Все поля сущности
        /// </summary>
        public bool AllColumns { get; set; }

        /// <summary>
        /// Список полей
        /// </summary>
        public ICollection<string> Columns { get; } = new List<string>();

        public ColumnSet(params string[] columns)
        {
            Columns = columns;
        }

        public ColumnSet(ICollection<string> columns)
        {
            Columns = columns;
        }
    }
}
