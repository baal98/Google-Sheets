namespace Google_Sheets.Models
{
    public class CreateTableModel
    {
        public string TableName { get; set; }
        public List<string> ColumnNames { get; set; }
        public string SpreadsheetId { get; set; } // Add this property
    }
}