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


        public async Task<string> CreateNewSpreadsheet(string tableName, List<string> columnNames, string spreadsheetId)
        {
            // Create a new spreadsheet with the specified title
            var spreadsheet = new Spreadsheet()
            {
                Properties = new SpreadsheetProperties()
                {
                    Title = tableName
                }
            };

            // Генериране на текущата дата и час в желания формат (например: "yyyyMMddHHmmss")
            string currentDateTime = DateTime.Now.ToString("yyyyMMddHHmmss");

            // Създаване на уникално име на листа, включващо текущата дата и час
            string sheetName = $"Sheet1_{currentDateTime}";

            // Използване на уникалното име на листа при създаването на Sheet обекта
            var sheet = new Sheet()
            {
                Properties = new SheetProperties()
                {
                    Title = sheetName // Уникалното име на листа
                }
            };


            // Create header row with column names
            var headerRow = new List<object>();
            foreach (var columnName in columnNames)
            {
                headerRow.Add(columnName);
            }

            var dataRow = new List<IList<object>> { headerRow };
            var valueRange = new ValueRange
            {
                Values = dataRow
            };

            // Add the header row to the sheet
            var updateRequest = service.Spreadsheets.Values.Update(valueRange, spreadsheetId, "Sheet1!A1");
            updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
            await updateRequest.ExecuteAsync();

            // Create the spreadsheet
            var createRequest = service.Spreadsheets.Create(spreadsheet);
            var createdSpreadsheet = await createRequest.ExecuteAsync();

            return createdSpreadsheet.SpreadsheetId;
        }

    }
}