using Google.Apis.Sheets.v4;
using Google_Sheets.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Google_Sheets.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ISheetsService _sheetsService;

        public ProductsController(ISheetsService sheetsService)
        {
            _sheetsService = sheetsService;
        }

        public IActionResult Index()
        {
            var products = _sheetsService.GetSheetData();
            return View(products);
        }
    }

}
