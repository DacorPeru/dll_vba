using System.Data;

namespace VBAConnector.Interfaces
{
    public interface IDataAccess
    {
        DataTable ExecuteSelect(string query);
        int ExecuteNonQuery(string query);
        int Insert(string table, string columns, string values);
        int Update(string table, string setValues, string condition);
        int Delete(string table, string condition);
    }
}
