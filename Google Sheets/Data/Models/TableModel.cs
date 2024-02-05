using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Google_Sheets.Data.Models
{
    public class TableModel
    {
        public Guid Id { get; set; }
        public string TableName { get; set; }

        public int NumberOfColumns { get; set; }

        public string SpreadsheetId { get; set; }

        public string Description { get; set; }
    }

}
