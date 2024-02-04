namespace Google_Sheets.Data.Models
{
    public class TableModel
    {
        public string TableName { get; set; }
        public List<string> ColumnNames { get; set; }
        public string SpreadsheetId { get; set; }
    }
}
