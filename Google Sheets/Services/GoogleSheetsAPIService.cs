using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google_Sheets.Data.Models;

namespace Google_Sheets.Services
{
    public class GoogleSheetsAPIService
    {
        private readonly SheetsService service;

        public GoogleSheetsAPIService(string jsonFilePath)
        {
            GoogleCredential credential;
            using (var stream = new FileStream(jsonFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                credential = GoogleCredential.FromStream(stream)
                    .CreateScoped(SheetsService.Scope.Spreadsheets);
            }

            // Създаване на Sheets API сервиза
            this.service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Your Application Name",
            });
        }

        public IList<IList<object>> GetValues(string spreadsheetId, string range)
        {
            SpreadsheetsResource.ValuesResource.GetRequest request =
                this.service.Spreadsheets.Values.Get(spreadsheetId, range);

            ValueRange response = request.Execute();
            return response.Values;
        }

        public List<Product> GetSheetData()
        {
            var spreadsheetId = "your-spreadsheet-id";
            var range = "Лист1!A2:E";
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

        public async Task UpdateSpreadsheet(string spreadsheetId, IList<IList<object>> values)
        {
            // Ensure all values are simple types that can be serialized to JSON
            for (int i = 0; i < values.Count; i++)
            {
                for (int j = 0; j < values[i].Count; j++)
                {
                    // If the value is not a simple type, convert it to string
                    if (!(values[i][j] is string || values[i][j] is double || values[i][j] is int))
                    {
                        values[i][j] = values[i][j].ToString();
                    }
                }
            }

            var range = "Лист1!A1:H";
            var valueRange = new ValueRange { Values = values };

            try
            {
                var updateRequest = service.Spreadsheets.Values.Update(valueRange, spreadsheetId, range);
                updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;

                await updateRequest.ExecuteAsync();
            }
            catch (Exception ex)
            {
                // Log the error or handle it as needed
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw; // Re-throw the exception if you cannot handle it here
            }
        }

        public async Task DeleteRow(string spreadsheetId, int rowIndex)
        {
            var range = $"Лист1!A{rowIndex}:H{rowIndex}";
            var requestBody = new BatchUpdateSpreadsheetRequest
            {
                Requests = new List<Request>
                {
                    new Request
                    {
                        DeleteDimension = new DeleteDimensionRequest
                        {
                            Range = new DimensionRange
                            {
                                SheetId = 0,
                                Dimension = "ROWS",
                                StartIndex = rowIndex - 1,
                                EndIndex = rowIndex
                            }
                        }
                    }
                }
            };

            var deleteRequest = service.Spreadsheets.BatchUpdate(requestBody, spreadsheetId);
            await deleteRequest.ExecuteAsync();
        }


        public async Task<string> CreateNewSpreadsheet(string tableName, int NumberOfColumns, string Description)
        {
            // Създаване на нов Spreadsheet обект със специфично заглавие
            var spreadsheet = new Spreadsheet()
            {
                Properties = new SpreadsheetProperties()
                {
                    Title = tableName
                },
                Sheets = new List<Sheet>() // Добавете тази линия
                {
                    new Sheet() // Създайте лист със заглавие
                    {
                        Properties = new SheetProperties()
                        {
                            Title = "Лист1" // Или използвайте уникално име за листа, както преди
                        }
                    }
                }
            };

            // Създайте таблицата
            var createRequest = service.Spreadsheets.Create(spreadsheet);
            var createdSpreadsheet = await createRequest.ExecuteAsync();

            // Получаване на SpreadsheetId от създадената таблица
            var newSpreadsheetId = createdSpreadsheet.SpreadsheetId;

            // Тук добавете логика за актуализация на листа с началния ред с имена на колони, ако е необходимо

            return newSpreadsheetId;
        }


    }
}