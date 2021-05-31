namespace CsvFileUploadSaveDb.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class TemplateConfig
    {
        public static readonly UploadTemplate[] Templates =
        {
            new UploadTemplate
            {
                Condition = tp => String.Equals(tp.ActionName, "Upload") && tp.LocaationId == 75,
                Template = TemplateColumns.GCP_P710_Resource,
                SqlTableName = "GCP_P710_Resource",
                DeleteSpName = "Delete_GCP_P710_Resource"
            },
            new UploadTemplate
            {
                Condition = tp => String.Equals(tp.ActionName, "AnotherUpload") && tp.LocaationId == 75,
                Template = TemplateColumns.FinancialData,
                SqlTableName = "GCP_P710_Resource",
                DeleteSpName = "Delete_GCP_P710_Resource"
            }
        };
    }
}
