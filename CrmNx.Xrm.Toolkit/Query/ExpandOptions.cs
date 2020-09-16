namespace CrmNx.Xrm.Toolkit.Query
{
    public class ExpandOptions
    {
        /// <summary>
        /// Имя навигационного свойства
        /// </summary>
        public string PropertyName { get; }

        /// <summary>
        /// Атрибуты связанной сущности
        /// </summary>
        public ColumnSet ColumnSet { get; set; }

        /// <summary>
        /// Запретить определение навигационного имени своства
        /// </summary>
        public bool DisableNameResolving { get; set; }

        /// <summary>
        /// Параметры запроса атрибутов связанной сущности
        /// </summary>
        /// <param name="propertyName">Имя навигационного свойства</param>
        /// <param name="columns">Атрибуты связанной сущности</param>
        public ExpandOptions(string propertyName, params string[] columns)
        {
            PropertyName = propertyName;
            ColumnSet = new ColumnSet(columns);
        }

        /// <summary>
        /// Параметры запроса атрибутов связанной сущности
        /// </summary>
        /// <param name="propertyName">Имя навигационного свойства</param>
        /// <param name="disableNameResolving">Запретить определение навигационного имени своства</param>
        /// <param name="columns">Атрибуты связанной сущности</param>
        public ExpandOptions(string propertyName, bool disableNameResolving, params string[] columns)
        {
            PropertyName = propertyName;
            ColumnSet = new ColumnSet(columns);
            DisableNameResolving = disableNameResolving;
        }
    }
}
