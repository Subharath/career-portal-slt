using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;

namespace JobApp.Repository
{
    public interface IUtilityFn
    {
        List<T> ConvertToList<T>(DataTable dataTable);
        DataTable ConvertToDataTable<T>(IEnumerable<T> self);
        List<string> IntegerList(int min, int max);
        List<int> CalulateDateDifference(DateTime startDate, DateTime endDate);
        string GenerateCode(int serial, int length, string prefix, string delimiter);
        string[] FindBirthDate(string NICNo);
        string ConvertToTitleCase(string text);
        void CreateLog(string message, string type);
    }
}
