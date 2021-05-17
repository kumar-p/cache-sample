namespace CsvFileUploadSaveDb.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public interface ICacheHelper
    {
        string GetString(string key);

        T GetValue<T>(string key) where T : class;

        void SetString(string key, string value);

        void SetValue<T>(string key, T value) where T : class;
    }
}
