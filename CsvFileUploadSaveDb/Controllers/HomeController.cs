namespace CsvFileUploadSaveDb.Controllers
{
    using CsvFileUploadSaveDb.Helpers;
    using CsvFileUploadSaveDb.Models;
    using ExcelDataReader;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Data.SqlClient;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Data;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;

    public class HomeController : Controller
    {
        private const string PROFILE_KEY = "profile";
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;
        private readonly ICacheHelper _cache;

        public HomeController(
            ILogger<HomeController> logger,
            IConfiguration configuration,
            IWebHostEnvironment env,
            ICacheHelper cache)
        {
            _logger = logger;
            _configuration = configuration;
            _env = env;
            _cache = cache;

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Upload(IFormFile file)
        {
            var templateConfig = GetTemplateConfig(new TemplateParams
            {
                ActionName = ControllerContext.ActionDescriptor.ActionName,
                LocaationId = 75 // to be taken from user
            });

            if (InvalidFileExtension(file.FileName))
            {
                ViewData["message"] = "Invalid File";
                return View("FileProcessStatus");
            }

            using Stream stream = file.OpenReadStream();
            using IExcelDataReader reader = ExcelReaderFactory.CreateReader(stream);

            SheetValidationResult validationResult = ValidateSheet(reader, templateConfig.Template);

            if (validationResult != SheetValidationResult.Success)
            {
                // Set proper message
                ViewData["message"] = "Invalid Excel Work Book";
                return View("FileProcessStatus");
            }

            using DataSet dataSet = GetDataSet(reader, templateConfig.Template);

            SaveDataToDatabase(dataSet.Tables[0], templateConfig.Template, templateConfig.SqlTableName, templateConfig.DeleteSpName);

            ViewData["message"] = $"{dataSet.Tables[0].Rows.Count} records saved successfully!";

            return View("FileProcessStatus");
        }

        private bool InvalidFileExtension(string fileName)
        {
            if (String.Equals(Path.GetExtension(fileName), ".xls", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (String.Equals(Path.GetExtension(fileName), ".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return true;
        }

        private SheetValidationResult ValidateSheet(
            IExcelDataReader reader,
            (string SheetName, string ColumnName)[] templateColumns)
        {
            if (!String.Equals(reader.Name, "sheet1", StringComparison.OrdinalIgnoreCase))
            {
                return SheetValidationResult.InvalidSheetName;
            }

            if (reader.RowCount == 0 || reader.RowCount == 0)
            {
                return SheetValidationResult.NoRecords;
            }

            reader.Read();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                var columnName = reader.GetString(i);
                bool exist = templateColumns.Any(config => string.Equals(
                    config.SheetName,
                    columnName,
                    StringComparison.OrdinalIgnoreCase));

                if (!exist)
                {
                    return SheetValidationResult.InvalidColumns;
                }
            }

            return SheetValidationResult.Success;
        }

        private DataSet GetDataSet(IExcelDataReader reader, (string SheetName, string ColumnName)[] templateColumns)
        {
            reader.Reset();
            DataSet dt = reader.AsDataSet(new ExcelDataSetConfiguration
            {
                ConfigureDataTable = (tableReader) => new ExcelDataTableConfiguration
                {
                    UseHeaderRow = true
                }
            });

            FormatDataSet(dt, templateColumns);

            return dt;
        }

        private void FormatDataSet(DataSet dt, (string SheetName, string ColumnName)[] templateColumns)
        {
            DataTable dataTable = dt.Tables[0];

            for (int i = 0; i < dataTable.Columns.Count; i++)
            {
                DataColumn dataColumn = dataTable.Columns[i];
                string columnName = dataColumn.ColumnName;
                var columnConfig = templateColumns.First(
                    config => String.Equals(config.SheetName, columnName, StringComparison.OrdinalIgnoreCase));
                dataColumn.ColumnName = columnConfig.ColumnName;
            }
        }

        private void SaveDataToDatabase(
            DataTable dataTable,
            (string SheetName, string ColumnName)[] templateColumns,
            string destinationTable,
            string deleteSpName)
        {
            string conString = _configuration.GetConnectionString("localdb");
            using var connection = new SqlConnection(conString);
            connection.Open();
            var deleteCommand = new SqlCommand(deleteSpName, connection);
            deleteCommand.CommandType = CommandType.StoredProcedure;
            deleteCommand.ExecuteNonQuery();

            using var bulkCopy = new SqlBulkCopy(conString, SqlBulkCopyOptions.UseInternalTransaction);
            bulkCopy.BulkCopyTimeout = 0;
            bulkCopy.DestinationTableName = destinationTable;
            bulkCopy.WriteToServer(dataTable);
        }

        private UploadTemplate GetTemplateConfig(TemplateParams templateParams)
        {
            return TemplateConfig.Templates.First(s => s.Condition(templateParams));
        }
    }
}
