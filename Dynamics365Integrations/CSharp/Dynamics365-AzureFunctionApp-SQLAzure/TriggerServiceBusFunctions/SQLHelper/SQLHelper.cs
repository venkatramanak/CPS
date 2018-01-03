using System;
using System.Data.SqlClient;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;

namespace TriggerServiceBusFunction
{
    public class SQLHelper
    {
        private string SQLConnectionstring { get; set; }
        private SqlConnection _sqlcon;
        private string _entityPrimaryKeyColumn = string.Empty;
        private string _entityIdValue = string.Empty;

        public SQLHelper(string primarykeycolumn)
        {
            //Use Azure Key Vault to store all your credentials
            SQLConnectionstring = ConfigurationManager.ConnectionStrings["sqlazure_connection"].ConnectionString;            
            _entityPrimaryKeyColumn = primarykeycolumn;
        }

        private SqlConnection SQLAzureConnection(string sqlconstring)
        {
            _sqlcon = new SqlConnection(sqlconstring);
            _sqlcon.Open();
            return _sqlcon;
        }
        
        private Int32 IsEntityRecordExist(Tuple<string,string,string> ss)
        {
            string selectquery = $"SELECT COUNT(0) FROM [Crm].[{ss.Item1}] WHERE {ss.Item2}=@EntityId";
            SqlCommand cmd = new SqlCommand(selectquery, _sqlcon);

            cmd.CommandType = System.Data.CommandType.Text;
            cmd.Parameters.AddWithValue("@EntityId", ss.Item3);

            Int32 count = (Int32)cmd.ExecuteScalar();
            return count;
        }

        public void Execute(object EntityObject, string EntityName)
        {            
            _entityIdValue = EntityObject.GetType().GetProperty(this._entityPrimaryKeyColumn).GetValue(EntityObject, null).ToString();
            SqlConnection sqlcon = SQLAzureConnection(SQLConnectionstring);            
            var entitytpl = Tuple.Create(EntityName, this._entityPrimaryKeyColumn, _entityIdValue);

            if (IsEntityRecordExist(entitytpl) == 0)
            {
                
                InsertRecordIntoSQL(PrepareQueries(EntityObject, EntityName, SQLOperationType.Insert));
            }
            else
            {                
                UpdateRecordIntoSQL(PrepareQueries(EntityObject, EntityName, SQLOperationType.Update));
            }

            _sqlcon.Close();
        }
        
        private void InsertRecordIntoSQL(string query)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand(query, _sqlcon))
                {
                    // Execute the command and log the # rows affected.
                    var rows = cmd.ExecuteNonQuery();
                    // log.Info($"{rows} rows were updated");
                }

            }
            catch (Exception)
            {

                throw;
            }
           
        }

        private void UpdateRecordIntoSQL(string query)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand(query, _sqlcon))
                {
                    // Execute the command and log the # rows affected.
                    var rows = cmd.ExecuteNonQuery();
                    // log.Info($"{rows} rows were updated");
                }
            }
            catch (Exception)
            {
                throw;
            }
           
        }

        private string PrepareQueries(object entityobj,  string entityname, SQLOperationType type)
        {  
            string finalsqlquery = string.Empty;            

            if (type == SQLOperationType.Insert)
            {
                string InsertQuery = $"INSERT INTO [Crm].[{entityname}]";
                string ColumnsQuery = string.Empty;
                string ValuesQuery = string.Empty;

                foreach (PropertyInfo pinfo in entityobj.GetType().GetProperties())
                {
                    var getMethod = pinfo.GetGetMethod();

                    if (getMethod.ReturnType.Name == "EntityProp")
                    {
                        var customprop = getMethod.Invoke(entityobj, null);

                        IEnumerable<PropertyInfo> query = from o in customprop.GetType().GetProperties()
                                                          where customprop.GetType().GetProperty("IsEmpty").GetValue(customprop, null).ToString() == "True"
                                                          select o;

                        foreach (PropertyInfo pi in query)
                        {
                            if (pi.Name != "IsEmpty")
                            {
                                ColumnsQuery = ColumnsQuery != string.Empty ? ColumnsQuery + ",[" + pinfo.Name + "]" : "[" + pinfo.Name + "]";
                                ValuesQuery = ValuesQuery != string.Empty ? ValuesQuery + ",'" + pi.GetValue(customprop, null) + "'" : "'" + pi.GetValue(customprop, null) + "'";
                            }                            
                        }

                    }
                    else
                    {
                        ColumnsQuery = ColumnsQuery != string.Empty ? ColumnsQuery + ",[" + pinfo.Name + "]" : "[" + pinfo.Name + "]";
                        ValuesQuery = ValuesQuery != string.Empty ? ValuesQuery + ",'" + pinfo.GetValue(entityobj, null) + "'" : "'" + pinfo.GetValue(entityobj, null) + "'";
                    }
                }

                //foreach (PropertyInfo pi in entityobj.GetType().GetProperties())
                //{
                //    ColumnsQuery = ColumnsQuery != string.Empty ? ColumnsQuery + ",[" + pi.Name + "]" : "[" + pi.Name + "]";
                //    ValuesQuery = ValuesQuery != string.Empty ? ValuesQuery + ",'" + pi.GetValue(entityobj, null) + "'" : "'" + pi.GetValue(entityobj, null) + "'";
                //}
                
                finalsqlquery = $"{InsertQuery} ({ColumnsQuery}) VALUES({ValuesQuery})";
            }
            else if (type == SQLOperationType.Update)
            {
                //string UpdateQuery = "UPDATE [Crm].[Lead] SET FullName = '' WHERE LeadId = '296afbff-1b0b-3ff5-9d6c-4e7e599f8b57';
                string UpdateQuery = $"UPDATE [Crm].[{entityname}] SET ";                
                string ColumnsValueQuery = string.Empty;
                string WhereQuery = $"WHERE [{this._entityPrimaryKeyColumn}] = '{this._entityIdValue}'";

                foreach (PropertyInfo pinfo in entityobj.GetType().GetProperties())
                {
                    var getMethod = pinfo.GetGetMethod();

                    if (getMethod.ReturnType.Name == "EntityProp")
                    {
                        var customprop = getMethod.Invoke(entityobj, null);

                        IEnumerable<PropertyInfo> query = from o in customprop.GetType().GetProperties()
                                                          where customprop.GetType().GetProperty("IsEmpty").GetValue(customprop, null).ToString() == "True"
                                                          select o;

                        foreach (PropertyInfo pi in query)
                        {
                            if(pi.Name != "IsEmpty")
                                ColumnsValueQuery = ColumnsValueQuery != string.Empty ? ColumnsValueQuery + $",[{pinfo.Name}] = '{pi.GetValue(customprop, null)}'" : $"[{pinfo.Name}] = '{pi.GetValue(customprop, null)}'";
                        }

                    }                    
                }

                //foreach (PropertyInfo pi in entityobj.GetType().GetProperties())
                //{
                //    ColumnsValueQuery = ColumnsValueQuery != string.Empty ? ColumnsValueQuery + $",[{pi.Name}] = '{pi.GetValue(entityobj, null)}'" : $"[{pi.Name}] = '{pi.GetValue(entityobj, null)}'";                    
                //}

                finalsqlquery = UpdateQuery + ColumnsValueQuery + " " + WhereQuery;
            }

            return finalsqlquery;
        }

        public void GetMyProperties(object obj)
        {
            foreach (PropertyInfo pinfo in obj.GetType().GetProperties())
            {
                var getMethod = pinfo.GetGetMethod();

                if (getMethod.ReturnType.Name == "EntityProp")
                {
                    var customprop = getMethod.Invoke(obj, null);
                    
                    IEnumerable<PropertyInfo> query = from o in customprop.GetType().GetProperties()
                                                      where customprop.GetType().GetProperty("IsEmpty").GetValue(customprop, null).ToString() == "True"
                                                      select o;                 

                    foreach (PropertyInfo p in query)
                    {
                        string name = p.Name;
                        string value = p.GetValue(customprop, null).ToString();
                    }

                }
            }
        }


        enum SQLOperationType
        {
           Insert = 1,
           Update = 2
        }
    }   
}
