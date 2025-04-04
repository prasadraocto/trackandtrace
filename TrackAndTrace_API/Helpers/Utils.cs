using Microsoft.Data.SqlClient;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using TrackAndTrace_API.Models;
using TrackAndTrace_API.Models.ResponseModel;

namespace TrackAndTrace_API.Helpers
{
    public static class Utils
    {
        // Make sure to use a secure way to generate and store your key and IV in a real application
        private static readonly byte[] Key = Encoding.UTF8.GetBytes("12345678901234567890123456789012"); // 32 bytes for AES-256
        private static readonly byte[] IV = Encoding.UTF8.GetBytes("1234567890123456"); // 16 bytes for AES

        public static string Encrypt(this string plainText)
        {
            using Aes aes = Aes.Create();
            aes.Key = Key;
            aes.IV = IV;

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using MemoryStream ms = new MemoryStream();
            using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            {
                using StreamWriter sw = new StreamWriter(cs);
                sw.Write(plainText);
            }

            return Convert.ToBase64String(ms.ToArray());
        }
        public static string Decrypt(this string cipherText)
        {
            using Aes aes = Aes.Create();
            aes.Key = Key;
            aes.IV = IV;

            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using MemoryStream ms = new MemoryStream(Convert.FromBase64String(cipherText));
            using CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using StreamReader sr = new StreamReader(cs);

            return sr.ReadToEnd();
        }
        public static ExtractTokenDto ExtractTokenDetails(HttpContext context, ApplicationDbContext db)
        {
            var companyId = context.User.FindFirst("CompanyId")?.Value;
            var userId = context.User.FindFirst("Id")?.Value;

            ExtractTokenDto extractTokenDto = new ExtractTokenDto();

            if (companyId != null && userId != null)
            {
                extractTokenDto.CompanyId = Convert.ToInt32(companyId);
                extractTokenDto.UserId = Convert.ToInt32(userId);

                var companyData = db.Company.Any(x => x.id == extractTokenDto.CompanyId && x.active_flag == true && x.delete_flag == false);
                var userData = db.Users.Any(x => x.id == extractTokenDto.UserId && x.active_flag == true && x.delete_flag == false);

                if (companyData && userData)
                {
                    return extractTokenDto;
                }
            }

            return null;
        }
        public static class SqlTypeMapper
        {
            public static SqlParameter MapType<T>(string parameterName, string typeName, IEnumerable<T> values)
            {
                return CreateStructuredParameter(parameterName, typeName, values);
            }
        }
        public static SqlParameter CreateStructuredParameter<T>(string parameterName, string typeName, IEnumerable<T> data)
        {
            // Create a DataTable to represent the table-valued parameter
            var dataTable = new DataTable();
            var properties = typeof(T).GetProperties();

            // Add columns to the DataTable for each property of the generic type
            foreach (var prop in properties)
            {
                dataTable.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            }

            // Add rows to the DataTable for each item in the data collection
            foreach (var item in data)
            {
                var row = dataTable.NewRow();
                foreach (var prop in properties)
                {
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                }
                dataTable.Rows.Add(row);
            }

            // Create a SqlParameter for the table-valued parameter
            return new SqlParameter(parameterName, SqlDbType.Structured)
            {
                TypeName = typeName,
                Value = dataTable
            };
        }
    }
}
