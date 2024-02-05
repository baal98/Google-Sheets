using Google_Sheets.Data;
using Google_Sheets.Data.Models;
using Google_Sheets.Models;
using Google_Sheets.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Google_Sheets.Controllers
{
    public class DataController : Controller
    {
        private readonly GoogleSheetsAPIService _googleSheetsService;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<GoogleSheetsAPIController> _logger;


        public DataController(GoogleSheetsAPIService googleSheetsService, IConfiguration configuration, ApplicationDbContext context, ILogger<GoogleSheetsAPIController> logger)
        {
            _googleSheetsService = googleSheetsService;
            _configuration = configuration;
            _context = context;
            _logger = logger;
        }

        //public IActionResult Index()
        //{
        //    var spreadsheetId = _configuration["GoogleSheets:SpreadsheetId"];
        //    ViewBag.SpreadsheetId = spreadsheetId;
        //    var range = "Лист1!A1:H";
        //    var values = _googleSheetsService.GetValues(spreadsheetId, range);
        //    int expectedColumnCount = 8; // Очакваният брой колони

        //    if (values != null && values.Count > 0)
        //    {
        //        for (int i = 0; i < values.Count; i++)
        //        {
        //            while (values[i].Count < expectedColumnCount)
        //            {
        //                values[i].Add(null); // Добавяне null или празен низ за празни клетки
        //            }
        //        }
        //        // Превръщане на values в модел и подаване към view
        //        return View(values);
        //    }
        //    else
        //    {
        //        // Обработка на случая, когато няма данни
        //        return View("Error");
        //    }
        //}

        public IActionResult Index(string id)
        {
            // Намиране на таблицата въз основа на подаденото ID
            var table = _context.Tables.FirstOrDefault(t => t.Id.ToString() == id);
            if (table == null)
            {
                // Обработка на случая, когато таблицата не е намерена
                return View("Error");
            }

            var spreadsheetId = table.SpreadsheetId;
            var spreadsheetName = table.TableName;
            ViewBag.SpreadsheetId = spreadsheetId;
            ViewBag.SpreadsheetName = spreadsheetName;

            var range = $"{table.TableName}!A1:H";
            var values = _googleSheetsService.GetValues(spreadsheetId, range);
            int expectedColumnCount = table.NumberOfColumns; // Очакваният брой колони

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
                var model = new SheetUpdateModel { Values = values, Name = spreadsheetName };

                return View(model);
            }
            else
            {
                // Обработка на случая, когато няма данни
                return View("Error");
            }
        }

        public async Task<IActionResult> AllTables()
        {
            var tables = await _context.Tables.ToListAsync();
            return View(tables);
        }

        // Добавете метод за показване на формата за създаване на нова таблица
        public IActionResult CreateTable()
        {
            return View();
        }

        [HttpPost("Data/CreateTable")]
        public async Task<IActionResult> CreateTable([FromBody] CreateTableModel model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError($"Validation errors: {string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))}");
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Received a request to create a new Google Sheets table.");

            try
            {
                // Създаване на нов Google Sheet и получаване на SpreadsheetId
                var spreadsheetId = await _googleSheetsService.CreateNewSpreadsheet(model.TableName, model.NumberOfColumns, model.Description);

                // Създаване на нов TableModel за съхранение в базата данни
                var newTableModel = new TableModel
                {
                    TableName = model.TableName,
                    NumberOfColumns = model.NumberOfColumns,
                    SpreadsheetId = spreadsheetId,
                    Description = model.Description
                };

                await _context.Tables.AddAsync(newTableModel);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Google Sheets table created successfully with ID: {spreadsheetId}.");

                // Връщане на успешен резултат с информация за създадената таблица
                return Ok(new { SpreadsheetId = spreadsheetId, TableModel = newTableModel });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Google Sheets table.");
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }


    }
}
