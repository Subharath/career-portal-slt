using JobApp.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using System.Data;

namespace JobApp.Repository
{
    public interface IDBOperations
    {
        DataTable SelectRows(string tableName, string fieldSet, string keyField, string keyValue, string whereClause, string keyFieldDataType = "");
        DataTable SelectRows(string sql);
        DataTable SelectRows(string sql, DataTable paraTable);
        List<SelectListItem> AnyDataList(string tableName, string valueField, string textField, string whereClause, string sortOrder);
        string UpdateRecords(string tableName, DataTable tempTable, string keyField, string keyValue, string keyFieldDataType = "");
        string UpdateRecords(string sql);
        string UpdateRecords(string sql, params SqlParameter[] parameters);
        string InsertRecords(string tableName, DataTable tempTable, bool isIdentity, out decimal generatedId, string identityField = "");
        DataTable GetFilteringCriteriaOfJobPosition(string intakeCode);
    }
}
