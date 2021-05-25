namespace CsvFileUploadSaveDb.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public static class TemplateColumns
    {
        public static readonly (string SheetName, string ColumnName)[] FinancialData =
        {
            ("Personnel No","PersonnelNbr"),
            ("Status","EmploymentStatusDescr"),
            ("Cost Code","ActivityType"),
            ("Bill Code","BillCd"),
            ("Cost Center","CostCenterNbr"),
            ("Profit Center","ProfitCenterNbr"),
            ("Rate Type","RateType"),
            ("Level (as per Finance)","jobcd"),
            ("Workforce","CCWorkForceDescr"),
            ("Data As On","DataAsOn")
        };

        public static readonly (string SheetName, string ColumnName)[] GCP_P710_Resource =
        {
            ("Sap ID","SapID"),
            ("Policy","Policy")
        };
    }
}
