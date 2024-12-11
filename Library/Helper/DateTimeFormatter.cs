namespace Library.Helper
{
    public class DateTimeFormatter
    {
        public static string FormatDate(DateTime? date)
        {
            if(date == null)
                return "";
            return date.Value.Year.ToString()+"-"+date.Value.Month.ToString()+"-"+date.Value.Day.ToString();
        }   
    }
}
