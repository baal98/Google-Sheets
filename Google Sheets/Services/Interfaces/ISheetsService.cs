using Google_Sheets.Data.Models;

namespace Google_Sheets.Services.Interfaces
{
    public interface ISheetsService
    {
        List<Product> GetSheetData();
    }
}