namespace CsvFileUploadSaveDb.Helpers
{
    using CsvFileUploadSaveDb.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class UploadTemplate
    {
        public Func<TemplateParams, bool> Condition { get; set; }

        public (string SheetName, string ColumnName)[] Template { get; set; }

        public string SqlTableName { get; set; }

        public string DeleteSpName { get; set; }
    }
}
