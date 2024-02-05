namespace Google_Sheets.Models
{
    public class CreateTableModel
    {
        public string TableName { get; set; }
        public int NumberOfColumns { get; set; }
        public string SpreadsheetId { get; set; } 
        public string Description { get; set; }
    }
}