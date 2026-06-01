using JobApp.Controllers;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;
using System.Globalization;

namespace JobApp.Repository
{   
    public class UtilityFn : IUtilityFn
    {
        private readonly ILogger<UtilityFn> _logger;

        public UtilityFn(ILogger<UtilityFn> logger)
        {
            _logger = logger;
        }

        public List<T> ConvertToList<T>(DataTable dataTable)
        {
            var columnNames = dataTable.Columns.Cast<DataColumn>().Select(c => c.ColumnName.ToLower()).ToList();
            var properties = typeof(T).GetProperties();
            return dataTable.AsEnumerable().Select(row => {
                var objT = Activator.CreateInstance<T>();
                foreach (var pro in properties)
                {
                    if (columnNames.Contains(pro.Name.ToLower()))
                    {
                        try
                        {
                            if (pro.PropertyType.FullName == "System.Int32" || pro.PropertyType.FullName == "System.Int16")
                                pro.SetValue(objT, Convert.ToInt32(row[pro.Name]));
                            else
                                pro.SetValue(objT, row[pro.Name]);
                        }
                        catch (Exception ex) { }
                    }
                }
                return objT;
            }).ToList();
        }

        public DataTable ConvertToDataTable<T>(IEnumerable<T> self)
        {
            var properties = typeof(T).GetProperties();

            var dataTable = new DataTable();
            foreach (var info in properties)
                dataTable.Columns.Add(info.Name, Nullable.GetUnderlyingType(info.PropertyType)
                   ?? info.PropertyType);

            foreach (var entity in self)
                dataTable.Rows.Add(properties.Select(p => p.GetValue(entity)).ToArray());

            return dataTable;
        }

        public List<string> IntegerList(int min, int max)
        {
            var list = new List<string>();

            for(int i = min; i <= max; i++)
            {
                list.Add(Convert.ToString(i));
            }

            return list;
        }

        public List<int> CalulateDateDifference(DateTime startDate, DateTime endDate)
        {
            //var StartDate = new DateTime(1985, 11, 20);
            //var EndDate = DateTime.Now;

            int years;
            int months;
            int days;

            for (var i = 1; ; ++i)
            {
                if (startDate.AddYears(i) > endDate)
                {
                    years = i - 1;

                    break;
                }
            }

            for (var i = 1; ; ++i)
            {
                if (startDate.AddYears(years).AddMonths(i) > endDate)
                {
                    months = i - 1;

                    break;
                }
            }

            for (var i = 1; ; ++i)
            {
                if (startDate.AddYears(years).AddMonths(months).AddDays(i) > endDate)
                {
                    days = i - 1;

                    break;
                }
            }

            List<int> list = new List<int>();
            list.Add(years);
            list.Add(months);
            list.Add(days);

            return list;
        }

        public string GenerateCode(int serial, int length, string prefix, string delimiter)
        {
            string generatedCode = "";
            generatedCode = prefix + delimiter + serial.ToString().PadLeft(length, '0');

            return generatedCode;
        }

        public string[] FindBirthDate(string NICNo)
        {
            var message = "";
            int dayText = 0;
            var year = "";
            var monthName = "";
            int monthValue = 0;
            int day = 0;
            var gender = "";
            int nic;
            string[] birthdate = new string[3];

            if (NICNo.Length != 10 && NICNo.Length != 12)
            {
                message = "Invalid NIC No";
            }
            else if (NICNo.Length == 10 && int.TryParse(NICNo.Substring(0, 9), out nic))
            {
                message = "Invalid NIC No";
            }
            else
            {
                // Year
                if (NICNo.Length == 10)
                {
                    year = "19" + NICNo.Substring(0, 2);
                    dayText = int.Parse(NICNo.Substring(2, 3));
                }
                else
                {
                    year = NICNo.Substring(0, 4);
                    dayText = int.Parse(NICNo.Substring(4, 3));
                }

                // Gender
                if (dayText > 500)
                {
                    gender = "Female";
                    dayText = dayText - 500;
                }
                else
                {
                    gender = "Male";
                }

                // Day Digit Validation
                if (dayText < 1 || dayText > 366)
                {
                    message = "Invalid NIC No";
                }
                else
                {
                    //Month
                    if (dayText > 335)
                    {
                        day = dayText - 335;
                        monthName = "December";
                        monthValue = 12;
                    }
                    else if (dayText > 305)
                    {
                        day = dayText - 305;
                        monthName = "November";
                        monthValue = 11;
                    }
                    else if (dayText > 274)
                    {
                        day = dayText - 274;
                        monthName = "October";
                        monthValue = 10;
                    }
                    else if (dayText > 244)
                    {
                        day = dayText - 244;
                        monthName = "September";
                        monthValue = 9;
                    }
                    else if (dayText > 213)
                    {
                        day = dayText - 213;
                        monthName = "August";
                        monthValue = 8;
                    }
                    else if (dayText > 182)
                    {
                        day = dayText - 182;
                        monthName = "July";
                        monthValue = 7;
                    }
                    else if (dayText > 152)
                    {
                        day = dayText - 152;
                        monthName = "June";
                        monthValue = 6;
                    }
                    else if (dayText > 121)
                    {
                        day = dayText - 121;
                        monthName = "May";
                        monthValue = 5;
                    }
                    else if (dayText > 91)
                    {
                        day = dayText - 91;
                        monthName = "April";
                        monthValue = 4;
                    }
                    else if (dayText > 60)
                    {
                        day = dayText - 60;
                        monthName = "March";
                        monthValue = 3;
                    }
                    else if (dayText < 32)
                    {
                        monthName = "January";
                        monthValue = 1;
                        day = dayText;
                    }
                    else if (dayText > 31)
                    {
                        day = dayText - 31;
                        monthName = "Febuary";
                        monthValue = 2;
                    }

                    birthdate[0] = year;
                    birthdate[1] = monthValue.ToString();
                    birthdate[2] = day.ToString();
                }
            }

            return birthdate;
        }

        public string ConvertToTitleCase(string text)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text.ToLower());
        }

        public void CreateLog(string message, string type)
        {
            switch(type)
            {
                case "error":
                {
                    _logger.LogError(message);
                    break;
                }
                case "info":
                {
                    _logger.LogInformation(message);
                    break;
                }
                default: { break; }
            }
        }
    }
}
