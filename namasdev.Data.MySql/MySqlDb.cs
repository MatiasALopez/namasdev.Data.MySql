using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using MySqlConnector;

namespace namasdev.Data.MySql
{
    public class MySqlDb : IDisposable
    {
        public MySqlDb(string connectionString)
        {
            if (String.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentNullException(nameof(connectionString));

            ConnectionString = connectionString;
        }

        public string ConnectionString { get; set; }

        public DataTable ObtenerDataTable(string cmd, params MySqlParametro[] parametros)
        {
            using (var conn = CrearConnection())
            {
                conn.Open();

                using (var command = CrearCommand(conn, cmd, parametros))
                {
                    using (var adapter = CrearDataAdapter(command))
                    {
                        var resultados = new DataTable();
                        adapter.Fill(resultados);

                        return resultados;
                    }
                }
            }
        }

        public IList<T> ObtenerListaObjetos<T>(string cmd, Func<MySqlLector, T> mapeo, params MySqlParametro[] parametros)
            where T : class
        {
            using (var conn = CrearConnection())
            {
                conn.Open();

                using (var command = CrearCommand(conn, cmd, parametros))
                using (var reader = command.ExecuteReader())
                using (var lector = new MySqlLector(reader))
                {
                    try
                    {
                        var res = new List<T>();
                        while (reader.Read())
                        {
                            res.Add(mapeo(lector));
                        }
                        return res;
                    }
                    finally
                    {
                        reader.Close();
                    }
                }
            }
        }

        public T ObtenerObjeto<T>(string cmd, Func<MySqlLector, T> mapeo, params MySqlParametro[] parametros)
            where T : class
        {
            using (var conn = CrearConnection())
            {
                conn.Open();

                using (var command = CrearCommand(conn, cmd, parametros))
                using (var reader = command.ExecuteReader())
                using (var lector = new MySqlLector(reader))
                {
                    try
                    {
                        if (reader.Read())
                        {
                            return mapeo(lector);
                        }

                        return default(T);
                    }
                    finally
                    {
                        reader.Close();
                    }
                }
            }
        }

        public object ObtenerEscalar(string cmd, params MySqlParametro[] parametros)
        {
            using (var conn = CrearConnection())
            {
                conn.Open();

                using (var command = CrearCommand(conn, cmd, parametros))
                {
                    return command.ExecuteScalar();
                }
            }
        }

        public int Ejecutar(string cmd, params MySqlParametro[] parametros)
        {
            using (var conn = CrearConnection())
            {
                conn.Open();

                using (var command = CrearCommand(conn, cmd, parametros))
                {
                    return command.ExecuteNonQuery();
                }
            }
        }

        public void Dispose()
        {
            //  por ahora nada
        }

        private MySqlConnection CrearConnection()
        {
            return new MySqlConnection(ConnectionString);
        }

        private MySqlCommand CrearCommand(MySqlConnection conn, string cmd, IEnumerable<MySqlParametro> parametros)
        {
            var comando = conn.CreateCommand();
            comando.CommandText = cmd;
            comando.Parameters.AddRange(
                parametros
                .Select(p => CrearParameter(p.Nombre, p.Valor))
                .ToArray()
                );

            return comando;
        }

        private MySqlParameter CrearParameter(string nombre, object valor)
        {
            return new MySqlParameter(nombre, valor);
        }

        private MySqlDataAdapter CrearDataAdapter(MySqlCommand cmdSelect)
        {
            return new MySqlDataAdapter(cmdSelect);
        }
    }
}
