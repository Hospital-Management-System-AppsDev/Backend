using MySql.Data.MySqlClient;

public class DatabaseConnection 
{
    private readonly string _connectionString;

    public DatabaseConnection(string connectionString) 
    {
        _connectionString = connectionString; // ✅ Use only the passed connection string
    }

    // ✅ Get an Open Connection (Ensures pooling works)
    public MySqlConnection GetOpenConnection()
    {
        var conn = new MySqlConnection(_connectionString);
        conn.Open();
        return conn;
    }
}
