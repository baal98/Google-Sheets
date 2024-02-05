namespace Google_Sheets.Models
{
    public class SheetUpdateModel
    {
        public IList<IList<object>> Values { get; set; }
        public string Name { get; set; }
    }

}
