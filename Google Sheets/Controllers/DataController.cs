using Google_Sheets.Data.Models;
using Google_Sheets.Services;
using Microsoft.AspNetCore.Mvc;

namespace Google_Sheets.Controllers
{
    public class DataController : Controller
    {
        private readonly GoogleSheetsAPIService _googleSheetsService;
        private readonly IConfiguration _configuration;

        public DataController(GoogleSheetsAPIService googleSheetsService, IConfiguration configuration)
        {
            _googleSheetsService = googleSheetsService;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            var spreadsheetId = _configuration["GoogleSheets:SpreadsheetId"];
            ViewBag.SpreadsheetId = spreadsheetId;
            var range = "Лист1!A1:H";
            var values = _googleSheetsService.GetValues(spreadsheetId, range);
            int expectedColumnCount = 8; // Очакваният брой колони

            if (values != null && values.Count > 0)
            {
                for (int i = 0; i < values.Count; i++)
                {
                    while (values[i].Count < expectedColumnCount)
                    {
                        values[i].Add(null); // Добавяне null или празен низ за празни клетки
                    }
                }
                // Превръщане на values в модел и подаване към view
                return View(values);
            }
            else
            {
                // Обработка на случая, когато няма данни
                return View("Error");
            }
        }

        public IActionResult TableView()
        {
            // Retrieve the newTableModel data from TempData
            var model = TempData["TableModelData"] as TableModel;

            if (model == null)
            {
                // Handle the case when TempData does not contain the expected data
                return RedirectToPage("Error");
            }

            return View(model);
        }


    }
}
