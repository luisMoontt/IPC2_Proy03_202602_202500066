using Microsoft.Data.Sqlite;
using SAT_ControlDTE.DataBase;
using SAT_ControlDTE.Models;

namespace SAT_ControlDTE.Services
{
    public class ResultadoRtu
    {
        public int Nuevos { get; set; }
        public int Actualizados { get; set; }
        public int Invalidos { get; set; }
    }

    public class RtuService
    {
        private readonly ConexionBD conexionBD;
        private readonly NitService nitService;

        public RtuService(ConexionBD conexionBD, NitService nitService)
        {
            this.conexionBD = conexionBD;
            this.nitService = nitService;
        }

        public ResultadoRtu Actualizar(List<Contribuyente> contribuyentes)
        {
            var resultado = new ResultadoRtu();

            using var conexion = conexionBD.ObtenerConexion();
            using var transaccion = conexion.BeginTransaction();

            foreach (Contribuyente contribuyente in contribuyentes)
            {
                if (!nitService.EsValido(contribuyente.Nit))
                {
                    resultado.Invalidos++;
                    continue;
                }

                using var actualizar = conexion.CreateCommand();
                actualizar.Transaction = transaccion;
                actualizar.CommandText = "UPDATE Contribuyente SET nombre = $nombre WHERE nit = $nit;";
                actualizar.Parameters.AddWithValue("$nit", contribuyente.Nit);
                actualizar.Parameters.AddWithValue("$nombre", contribuyente.Nombre);

                // Si el UPDATE no cambio ninguna fila, el contribuyente no existia
                if (actualizar.ExecuteNonQuery() > 0)
                {
                    resultado.Actualizados++;
                    continue;
                }

                using var insertar = conexion.CreateCommand();
                insertar.Transaction = transaccion;
                insertar.CommandText = "INSERT INTO Contribuyente (nit, nombre) VALUES ($nit, $nombre);";
                insertar.Parameters.AddWithValue("$nit", contribuyente.Nit);
                insertar.Parameters.AddWithValue("$nombre", contribuyente.Nombre);
                insertar.ExecuteNonQuery();
                resultado.Nuevos++;
            }

            transaccion.Commit();
            return resultado;
        }

        public List<Contribuyente> Listar()
        {
            var contribuyentes = new List<Contribuyente>();

            using var conexion = conexionBD.ObtenerConexion();
            using var comando = conexion.CreateCommand();
            comando.CommandText = "SELECT nit, nombre FROM Contribuyente ORDER BY nit;";

            using SqliteDataReader lector = comando.ExecuteReader();
            while (lector.Read())
                contribuyentes.Add(new Contribuyente(lector.GetString(0), lector.GetString(1)));

            return contribuyentes;
        }
    }
}