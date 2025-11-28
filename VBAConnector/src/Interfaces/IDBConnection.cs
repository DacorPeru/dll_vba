namespace VBAConnector.Interfaces
{
    public interface IDBConnection
    {
        string GetConnectionString();
        void LoadConnectionConfig(string configFilePath);
    }
}
