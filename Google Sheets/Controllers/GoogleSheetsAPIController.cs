using Google;
using Google_Sheets.Data.Models;
using Google_Sheets.Models;
using Google_Sheets.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net;
using Google_Sheets.Data;

namespace Google_Sheets.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GoogleSheetsAPIController : ControllerBase
    {
        private readonly GoogleSheetsAPIService _sheetsService;
        private readonly ILogger<GoogleSheetsAPIController> _logger;
        private readonly ApplicationDbContext _context;


        public GoogleSheetsAPIController(GoogleSheetsAPIService sheetsService, ILogger<GoogleSheetsAPIController> logger, ApplicationDbContext context)
        {
            _sheetsService = sheetsService;
            _logger = logger;
            _context = context;
        }


        // GET: api/SheetsApi
        [HttpGet]
        public IActionResult GetSheetData()
        {
            try
            {
                var data = _sheetsService.GetSheetData();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpPost("update/{spreadsheetId}")]
        public async Task<IActionResult> UpdateSheet(string spreadsheetId, [FromBody] SheetUpdateModel model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError($"Validation errors: {string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))}");
                return BadRequest(ModelState);
            }
            _logger.LogInformation("Received a request to update the sheet.");

            try
            {
                await _sheetsService.UpdateSpreadsheet(spreadsheetId, model.Values);
                _logger.LogInformation("Sheet updated successfully.");
                return Ok("Sheet updated successfully.");
            }
            catch (GoogleApiException ex) when (ex.HttpStatusCode == HttpStatusCode.Forbidden)
            {
                _logger.LogWarning("Forbidden: You do not have permission to update this sheet.");
                return Forbid("You do not have permission to update this sheet.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating sheet. Request data: " + JsonConvert.SerializeObject(model.Values));
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpDelete("delete/{spreadsheetId}/{rowIndex}")]
        public async Task<IActionResult> DeleteRow(string spreadsheetId, int rowIndex)
        {
            try
            {
                await _sheetsService.DeleteRow(spreadsheetId, rowIndex);
                _logger.LogInformation("Row deleted successfully.");
                return Ok("Row deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting row.");
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpPost("CreateTable")]
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
                // Create a new TableModel and save it to the database
                var newTableModel = new TableModel
                {
                    TableName = model.TableName,
                    ColumnNames = model.ColumnNames,
                    SpreadsheetId = null // Initialize to null, as it will be updated later
                };

                // Save the new TableModel to your database (you will need to implement this)
                // You can generate a unique Id here or let the database generate it
                await _context.Tables.AddAsync(newTableModel);
                await _context.SaveChangesAsync();

                // Now, you can retrieve the generated Id
                var newTableId = newTableModel.SpreadsheetId;

                // Create a new Google Sheet
                var newSpreadsheetId = await _sheetsService.CreateNewSpreadsheet(model.TableName, model.ColumnNames, newTableId);

                // Update the newTableModel with the generated SpreadsheetId
                newTableModel.SpreadsheetId = newSpreadsheetId;

                // Update the TableModel in your database with the new SpreadsheetId
                // You will need to implement this

                _logger.LogInformation("Google Sheets table created successfully.");

                // Return the newTableModel along with its Id in the response
                return Ok(new { Id = newTableId, TableModel = newTableModel });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Google Sheets table.");
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }


    }
}
