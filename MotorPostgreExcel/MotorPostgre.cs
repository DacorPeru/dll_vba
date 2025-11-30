using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Data;
using Npgsql;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MotorPostgreExcel
{
    // Clase COM principal que verá VBA
    [ComVisible(true)]
    [Guid("F5330812-160D-4228-9E57-9F404F98A755")] // <-- CAMBIA este GUID por uno tuyo
    [ClassInterface(ClassInterfaceType.AutoDual)]
    public class MotorPostgre
    {
        // =========================
        //  Estado interno
        // =========================

        private string _connectionString = string.Empty;
        private readonly Dictionary<string, string> _connectionStringsByAlias =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private NpgsqlConnection _connection;
        private NpgsqlTransaction _transaction;

        private bool _hasError;
        private string _lastErrorMessage = string.Empty;
        private string _lastErrorTechnical = string.Empty;
        private string _lastSql = string.Empty;

        // =========================
        //  Propiedades de diagnóstico
        // =========================

        public bool HayError => _hasError;
        public string UltimoErrorMensaje => _lastErrorMessage;
        public string UltimoErrorTecnico => _lastErrorTechnical;
        public string UltimoSql => _lastSql;

        private void ResetError()
        {
            _hasError = false;
            _lastErrorMessage = string.Empty;
            _lastErrorTechnical = string.Empty;
        }

        private void SetError(Exception ex, string sql)
        {
            _hasError = true;
            _lastSql = sql ?? string.Empty;
            _lastErrorTechnical = ex.ToString();
            _lastErrorMessage = ex.Message;
        }

        // =========================
        //  Gestión de conexión simple
        // =========================

        public void ConfigurarConexion(string cadenaConexion)
        {
            _connectionString = cadenaConexion ?? string.Empty;
        }

        public string ProbarConexion()
        {
            ResetError();
            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();
                }
                return "OK";
            }
            catch (Exception ex)
            {
                SetError(ex, string.Empty);
                return "ERROR: " + ex.Message;
            }
        }

        public string CerrarConexion()
        {
            ResetError();
            try
            {
                if (_transaction != null)
                {
                    _transaction.Dispose();
                    _transaction = null;
                }

                if (_connection != null)
                {
                    _connection.Close();
                    _connection.Dispose();
                    _connection = null;
                }

                return "OK";
            }
            catch (Exception ex)
            {
                SetError(ex, string.Empty);
                return "ERROR: " + ex.Message;
            }
        }

        // =========================
        //  Multi-conexión por alias
        // =========================

        public string RegistrarConexion(string alias, string cadenaConexion)
        {
            ResetError();
            try
            {
                if (string.IsNullOrWhiteSpace(alias))
                    throw new ArgumentException("El alias no puede estar vacío.");

                _connectionStringsByAlias[alias] = cadenaConexion ?? string.Empty;
                return "OK";
            }
            catch (Exception ex)
            {
                SetError(ex, string.Empty);
                return "ERROR: " + ex.Message;
            }
        }

        public string SeleccionarConexion(string alias)
        {
            ResetError();
            try
            {
                if (!_connectionStringsByAlias.TryGetValue(alias, out var cs))
                    throw new InvalidOperationException("Alias de conexión no encontrado: " + alias);

                _connectionString = cs;
                return "OK";
            }
            catch (Exception ex)
            {
                SetError(ex, string.Empty);
                return "ERROR: " + ex.Message;
            }
        }

        public object ListarConexiones()
        {
            ResetError();
            try
            {
                var aliases = new string[_connectionStringsByAlias.Count];
                int i = 0;
                foreach (var key in _connectionStringsByAlias.Keys)
                    aliases[i++] = key;

                return aliases; // Se verá como Variant() en VBA
            }
            catch (Exception ex)
            {
                SetError(ex, string.Empty);
                return null;
            }
        }

        // =========================
        //  Helpers internos
        // =========================

        private NpgsqlConnection EnsureConnection()
        {
            if (string.IsNullOrWhiteSpace(_connectionString))
                throw new InvalidOperationException("No hay cadena de conexión configurada.");

            if (_connection == null)
            {
                _connection = new NpgsqlConnection(_connectionString);
                _connection.Open();
            }
            else if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }

            return _connection;
        }

        private static object ConvertJsonElement(JsonElement e)
        {
            switch (e.ValueKind)
            {
                case JsonValueKind.Null:
                case JsonValueKind.Undefined:
                    return DBNull.Value;

                case JsonValueKind.Number:
                    if (e.TryGetInt64(out long l)) return l;
                    if (e.TryGetDecimal(out decimal d)) return d;
                    return e.GetDouble();

                case JsonValueKind.String:
                    if (e.TryGetDateTime(out DateTime dt)) return dt;
                    return e.GetString();

                case JsonValueKind.True:
                case JsonValueKind.False:
                    return e.GetBoolean();

                default:
                    // Arrays u objetos complejos: se mandan como string JSON
                    return e.ToString();
            }
        }

        private NpgsqlCommand CreateCommand(string sql, string parametrosJson)
        {
            var conn = EnsureConnection();
            var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            if (_transaction != null)
                cmd.Transaction = _transaction;

            if (!string.IsNullOrWhiteSpace(parametrosJson))
            {
                try
                {
                    var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(parametrosJson);
                    if (dict != null)
                    {
                        foreach (var kvp in dict)
                        {
                            var valor = ConvertJsonElement(kvp.Value);
                            cmd.Parameters.AddWithValue("@" + kvp.Key, valor ?? DBNull.Value);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new ArgumentException(
                        "Error al interpretar parametrosJson. Debe ser un JSON de objeto simple (por ejemplo: {\"id\":1,\"nombre\":\"Ana\"}).",
                        ex);
                }
            }

            return cmd;
        }

        // =========================
        //  Ejecución de SQL
        // =========================

        public object EjecutarConsultaMatriz(string sql, string parametrosJson)
        {
            ResetError();
            _lastSql = sql ?? string.Empty;

            try
            {
                using (var cmd = CreateCommand(sql, parametrosJson))
                using (var reader = cmd.ExecuteReader())
                {
                    int fieldCount = reader.FieldCount;

                    var rows = new List<object[]>();

                    // Cabecera con nombres de columnas
                    var header = new object[fieldCount];
                    for (int i = 0; i < fieldCount; i++)
                        header[i] = reader.GetName(i);
                    rows.Add(header);

                    // Filas de datos
                    while (reader.Read())
                    {
                        var row = new object[fieldCount];
                        for (int i = 0; i < fieldCount; i++)
                        {
                            row[i] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                        }
                        rows.Add(row);
                    }

                    int rowCount = rows.Count;
                    var result = new object[rowCount, fieldCount];

                    for (int r = 0; r < rowCount; r++)
                    {
                        var src = rows[r];
                        for (int c = 0; c < fieldCount; c++)
                        {
                            result[r, c] = src[c];
                        }
                    }

                    return result; // VBA lo verá como Variant(row, col)
                }
            }
            catch (Exception ex)
            {
                SetError(ex, sql);

                // Si no hay transacción activa, cerramos conexión por seguridad
                if (_transaction == null && _connection != null)
                {
                    _connection.Close();
                    _connection.Dispose();
                    _connection = null;
                }

                return null;
            }
        }

        public object EjecutarEscalar(string sql, string parametrosJson)
        {
            ResetError();
            _lastSql = sql ?? string.Empty;

            try
            {
                using (var cmd = CreateCommand(sql, parametrosJson))
                {
                    var value = cmd.ExecuteScalar();
                    return value == DBNull.Value ? null : value;
                }
            }
            catch (Exception ex)
            {
                SetError(ex, sql);

                if (_transaction == null && _connection != null)
                {
                    _connection.Close();
                    _connection.Dispose();
                    _connection = null;
                }

                return null;
            }
        }

        public string EjecutarComando(string sql, string parametrosJson)
        {
            ResetError();
            _lastSql = sql ?? string.Empty;

            try
            {
                using (var cmd = CreateCommand(sql, parametrosJson))
                {
                    int affected = cmd.ExecuteNonQuery();
                    return "OK:filas=" + affected;
                }
            }
            catch (Exception ex)
            {
                SetError(ex, sql);

                if (_transaction == null && _connection != null)
                {
                    _connection.Close();
                    _connection.Dispose();
                    _connection = null;
                }

                return "ERROR: " + ex.Message;
            }
        }

        // =========================
        //  Transacciones
        // =========================

        public string IniciarTransaccion()
        {
            ResetError();
            try
            {
                if (_transaction != null)
                    throw new InvalidOperationException("Ya hay una transacción activa.");

                var conn = EnsureConnection();
                _transaction = conn.BeginTransaction();
                return "OK";
            }
            catch (Exception ex)
            {
                SetError(ex, string.Empty);
                return "ERROR: " + ex.Message;
            }
        }

        public string ConfirmarTransaccion()
        {
            ResetError();
            try
            {
                if (_transaction == null)
                    throw new InvalidOperationException("No hay una transacción activa.");

                _transaction.Commit();
                _transaction.Dispose();
                _transaction = null;

                if (_connection != null)
                {
                    _connection.Close();
                    _connection.Dispose();
                    _connection = null;
                }

                return "OK";
            }
            catch (Exception ex)
            {
                SetError(ex, string.Empty);
                return "ERROR: " + ex.Message;
            }
        }

        public string RevertirTransaccion()
        {
            ResetError();
            try
            {
                if (_transaction == null)
                    throw new InvalidOperationException("No hay una transacción activa.");

                _transaction.Rollback();
                _transaction.Dispose();
                _transaction = null;

                if (_connection != null)
                {
                    _connection.Close();
                    _connection.Dispose();
                    _connection = null;
                }

                return "OK";
            }
            catch (Exception ex)
            {
                SetError(ex, string.Empty);
                return "ERROR: " + ex.Message;
            }
        }
    }
}
