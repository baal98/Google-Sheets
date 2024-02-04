using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4.Data;
using Google_Sheets.Data.Models;
using Google_Sheets.Services.Interfaces;

namespace Google_Sheets.Services
{
    namespace Google_Sheets.Services
    {
        public class MySheetsService : ISheetsService
        {
            private readonly string[] Scopes = { Google.Apis.Sheets.v4.SheetsService.Scope.SpreadsheetsReadonly };
            private readonly string ApplicationName = "Your Application Name";
            private Google.Apis.Sheets.v4.SheetsService service;

            public MySheetsService()
            {
                GoogleCredential credential;

                using (var stream = new FileStream("path/to/your/service-account-file.json", FileMode.Open, FileAccess.Read))
                {
                    credential = GoogleCredential.FromStream(stream).CreateScoped(Scopes);
                }

                this.service = new Google.Apis.Sheets.v4.SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });
            }

            public List<Product> GetSheetData()
            {
                var spreadsheetId = "your-spreadsheet-id";
                var range = "Sheet1!A2:E";
                var request = service.Spreadsheets.Values.Get(spreadsheetId, range);

                ValueRange response = request.Execute();
                IList<IList<object>> values = response.Values;

                List<Product> products = new List<Product>();
                if (values != null && values.Count > 0)
                {
                    foreach (var row in values)
                    {
                        products.Add(new Product
                        {
                            Name = row[0]?.ToString(),
                            Description = row[1]?.ToString(),
                        });
                    }
                }

                return products;
            }
        }
    }

}
