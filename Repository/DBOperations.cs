using JobApp.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Common;
using System.Security.Principal;

namespace JobApp.Repository
{
    public class DBOperations : IDBOperations
    {
        private readonly DBConnection _dbConnection = new();
        
        public DataTable SelectRows(string tableName, string fieldSet, string keyField, string keyValue, string whereClause, string keyFieldDataType)
        {
            DataTable dtblResult = new DataTable();
            using SqlConnection con = _dbConnection.GetDbConnection();

            using SqlCommand sqlCmd = con.CreateCommand();
            {
                con.Open();
                sqlCmd.Connection = con;

                whereClause = whereClause != string.Empty? whereClause.Trim() : string.Empty;

                if (whereClause.ToUpper().StartsWith("WHERE"))
                    whereClause = whereClause.Remove(0, 5);

                if (whereClause != string.Empty && keyField != string.Empty && keyValue != string.Empty)
                {
                    if (keyFieldDataType == "")
                        whereClause = "WHERE " + whereClause + " AND " + keyField + " = '" + keyValue + "'";
                    else
                        whereClause = "WHERE " + whereClause + " AND " + keyField + " = " + keyValue;
                }
                else if (whereClause == string.Empty && keyField != string.Empty && keyValue != string.Empty)
                {
                    if (keyFieldDataType == "")
                        whereClause = "WHERE " + keyField + " = '" + keyValue + "'";
                    else
                        whereClause = "WHERE " + keyField + " = " + keyValue;
                }
                else if (whereClause != string.Empty)
                    whereClause = "WHERE " + whereClause;

                sqlCmd.CommandText = "SELECT " + fieldSet + " FROM " + tableName + " " + whereClause;

                SqlDataAdapter sqlDa = new SqlDataAdapter(sqlCmd);

                //SqlDataAdapter sqlDa = new SqlDataAdapter("SELECT " + fieldSet + " FROM " + tableName + " " + whereClause, DbCon.myConnection);
                sqlDa.Fill(dtblResult);
            }

            return dtblResult;
        }

        public DataTable SelectRows(string sql)
        {
            DataTable dtblResult = new DataTable();
            using SqlConnection con = _dbConnection.GetDbConnection();

            using SqlCommand sqlCmd = con.CreateCommand();
            {
                sqlCmd.CommandText = sql;

                SqlDataAdapter sqlDa = new SqlDataAdapter(sqlCmd);

                sqlDa.Fill(dtblResult);
            }

            return dtblResult;
        }

        public DataTable SelectRows(string sql, DataTable paraTable)
        {
            DataTable dtblResult = new DataTable();
            using SqlConnection con = _dbConnection.GetDbConnection();

            string paratype = "";
            string paraname = "";
            string paravalue = "";

            using SqlCommand sqlCmd = con.CreateCommand();
            {
                sqlCmd.CommandText = sql;

                foreach (DataRow DR in paraTable.Rows)
                {
                    paraname = DR[0].ToString();
                    paratype = DR[1].ToString().ToLower();
                    paravalue = DR[2].ToString();

                    if (! paraname.StartsWith("@"))
                        paraname = "@" + paraname;

                    if (paratype == "datetime")
                        sqlCmd.Parameters.Add(new SqlParameter(paraname, SqlDbType.DateTime)).Value = Convert.ToDateTime(paravalue);
                    else if (paratype == "int")
                        sqlCmd.Parameters.Add(new SqlParameter(paraname, SqlDbType.Int)).Value = Convert.ToInt32(paravalue);
                    else
                        sqlCmd.Parameters.Add(new SqlParameter(paraname, SqlDbType.VarChar)).Value = paravalue;
                }

                SqlDataAdapter sqlDa = new SqlDataAdapter(sqlCmd);

                sqlDa.Fill(dtblResult);
            }

            return dtblResult;
        }

        public List<SelectListItem> AnyDataList(string tableName, string valueField, string textField, string whereClause, string sortOrder)
        {
            DataTable dtblData = new DataTable();
            List<SelectListItem> dataList = new List<SelectListItem>();

            string sql = "SELECT " + valueField;

            if (valueField != textField)
                sql = sql + "," + textField;

            sql = sql + " FROM " + tableName;

            if (!string.IsNullOrEmpty(whereClause))
                sql = sql + " WHERE " + whereClause;

            if (!string.IsNullOrEmpty(sortOrder))
                sql = sql + " ORDER BY " + sortOrder;

            using SqlConnection con = _dbConnection.GetDbConnection();

            using SqlCommand sqlCmd = con.CreateCommand();
            {
                con.Open();
                sqlCmd.Connection = con;
                sqlCmd.CommandText = sql;

                SqlDataAdapter sqlDa = new SqlDataAdapter(sqlCmd);
                    
                sqlDa = new SqlDataAdapter(sqlCmd);
                sqlDa.Fill(dtblData);

                foreach (DataRow dr in dtblData.Rows)
                {
                    SelectListItem selListItem = new SelectListItem() { Value = dr[valueField].ToString(), Text = dr[textField].ToString() };
                    dataList.Add(selListItem);
                }

                //dataList = dtblData.AsEnumerable().Select(r => r.Field<string>(fieldList)).ToList();
            }

            return dataList;

        }

        public string UpdateRecords(string tableName, DataTable tempTable, string keyField, string keyValue, string keyFieldDataType = "")
        {
            string retrunMsg = "";
            string sql = "";
            string paralist = "";
            string fldname = "";
            string whereClause = "";
            int i = 0;

            //remove the Keyfield (i.e primary key) column as it is not updatable.
            foreach (DataColumn col in tempTable.Columns)
            {
                if (col.ColumnName.ToLower() == keyField.ToLower()) { 
                    tempTable.Columns.Remove(col);
                    break;
                }
            }

            using SqlConnection con = _dbConnection.GetDbConnection();
            con.Open();

            using SqlCommand sqlCmd = con.CreateCommand();
            {
                while (i < tempTable.Columns.Count)
                {
                    fldname = tempTable.Columns[i].ColumnName;
                    paralist = paralist + fldname + " = @" + fldname + ",";
                    i++;
                }

                //fldlist = " (" + fldlist.Remove(fldlist.Length - 1, 1) + ") ";
                paralist = paralist.Remove(paralist.Length - 1, 1);

                foreach (DataRow DR in tempTable.Rows)
                {
                    foreach (DataColumn DC in tempTable.Columns)
                    {
                        fldname = DC.ColumnName;
                        if (DC.DataType == typeof(DateTime))
                            sqlCmd.Parameters.Add(new SqlParameter(fldname, SqlDbType.DateTime)).Value = Convert.ToDateTime(DR[fldname]);
                        else if (DC.DataType == typeof(int))
                            sqlCmd.Parameters.Add(new SqlParameter(fldname, SqlDbType.Int)).Value = Convert.ToInt32(DR[fldname]);
                        else
                            sqlCmd.Parameters.Add(new SqlParameter(fldname, SqlDbType.VarChar)).Value = DR[fldname].ToString();
                    }

                    if (keyFieldDataType == "")
                        whereClause = " WHERE " + keyField + " = '" + keyValue + "'";
                    else
                        whereClause = " WHERE " + keyField + " = " + keyValue;

                    sql = "UPDATE " + tableName + " SET " + paralist + whereClause;

                    sqlCmd.CommandType = CommandType.Text;
                    sqlCmd.CommandText = sql;

                    try
                    {
                        if (sqlCmd.ExecuteNonQuery() > 0)
                            retrunMsg = "SUCCESS";
                    }
                    catch (Exception exc)
                    {
                        retrunMsg = exc.ToString();
                    }
                }
            }
            return retrunMsg;
        }

        public string UpdateRecords(string sql)
        {
            string retrunMsg = "";

            using SqlConnection con = _dbConnection.GetDbConnection();
            con.Open();

            using SqlCommand sqlCmd = con.CreateCommand();
            {
                sqlCmd.CommandType = CommandType.Text;
                sqlCmd.CommandText = sql;

                try
                {
                    if (sqlCmd.ExecuteNonQuery() > 0)
                        retrunMsg = "SUCCESS";
                }
                catch (Exception exc)
                {
                    retrunMsg = exc.ToString();
                }
            }
            return retrunMsg;
        }

        public string InsertRecords(string tableName, DataTable tempTable, bool isIdentity, out decimal generatedId, string identityField = "")
        {
            string retrunMsg = "";
            string sql = "";
            string fldlist = "";
            string paralist = "";
            string fldname = "";
            int i = 0;
            string valueset = "";
            generatedId = 0;

            //remove the identity field (i.e primary key) if table has a identity column.
            if (isIdentity)
            {
                foreach (DataColumn col in tempTable.Columns)
                {
                    if (col.ColumnName.ToLower() == identityField.ToLower())
                    {
                        tempTable.Columns.Remove(col);
                        break;
                    }
                }
            }

            using SqlConnection con = _dbConnection.GetDbConnection();
            con.Open();

            using SqlCommand sqlCmd = con.CreateCommand();
            {             
                while (i < tempTable.Columns.Count)
                {
                    fldname = tempTable.Columns[i].ColumnName;
                    fldlist = fldlist + fldname + ",";
                    paralist = paralist + "@" + fldname + ",";
                    i++;
                }

                fldlist = " (" + fldlist.Remove(fldlist.Length - 1, 1) + ") ";
                paralist = " (" + paralist.Remove(paralist.Length - 1, 1) + ") ";

                foreach (DataRow DR in tempTable.Rows)
                {
                    sqlCmd.Parameters.Clear();

                    foreach (DataColumn DC in tempTable.Columns)
                    {
                        fldname = DC.ColumnName;
                        if (DC.DataType == typeof(DateTime) && DR[fldname] != DBNull.Value)
                            sqlCmd.Parameters.Add(new SqlParameter(fldname, SqlDbType.DateTime)).Value = Convert.ToDateTime(DR[fldname]);
                        else if (DC.DataType == typeof(int))
                            sqlCmd.Parameters.Add(new SqlParameter(fldname, SqlDbType.Int)).Value = Convert.ToInt32(DR[fldname]);
                        else
                            sqlCmd.Parameters.Add(new SqlParameter(fldname, SqlDbType.VarChar)).Value = DR[fldname].ToString();

                        valueset = valueset + DR[fldname].ToString() + "--";
                    }

                    sql = "INSERT INTO " + tableName + fldlist + " VALUES " + paralist;

                    if (isIdentity)
                        sql += "; SELECT SCOPE_IDENTITY()";

                    sqlCmd.CommandType = CommandType.Text;
                    sqlCmd.CommandText = sql;

                    try
                    {
                        if (isIdentity)
                        {
                            generatedId = (decimal)sqlCmd.ExecuteScalar();
                            if (generatedId > 0)
                                retrunMsg = "SUCCESS";
                        }
                        else if (sqlCmd.ExecuteNonQuery() > 0)
                            retrunMsg = "SUCCESS";
                    }
                    catch (Exception exc)
                    {
                        retrunMsg = exc.ToString() + "-" + valueset;
                    }
                }
            }

            return retrunMsg;
        }

        public DataTable GetFilteringCriteriaOfJobPosition(string intakeCode)
        {
            //string sql = "SELECT B.JobPositionCode, B.ALRequired, B.OLRequired  FROM Intake A INNER JOIN JobPosition B ON A.JobPositionID = B.JobPositionID WHERE A.IntakeCode = '" + intakeCode + "'";
            string sql = "SELECT IntakeCode, ALRequired, OLRequired, HERequired FROM Intake WHERE IntakeCode = '" + intakeCode + "'";
            DataTable dataTable = SelectRows(sql);

            return dataTable;
        }
    }
}
