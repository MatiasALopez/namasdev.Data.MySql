using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace namasdev.Data.MySql
{
    public class MySqlLector : IDisposable
    {
        private DbDataReader _reader;

        public MySqlLector(DbDataReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            _reader = reader;
        }

        public object this[string nombre]
        {
            get
            {
                var valor = _reader[nombre];
                if (valor is DBNull)
                    return null;

                return valor;
            }
        }

        public void Dispose()
        {
            _reader.Dispose();
        }
    }
}
