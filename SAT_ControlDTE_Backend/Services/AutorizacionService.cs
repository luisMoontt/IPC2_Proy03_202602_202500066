using Microsoft.Data.Sqlite;
using SAT_ControlDTE.DataBase;
using SAT_ControlDTE.Validations;

namespace SAT_ControlDTE.Services
{
    public class AutorizacionService
    {
        private readonly ConexionBD conexionBD;
        private readonly NitService nitService;

        private readonly List<Validacion> validaciones = new List<Validacion>
        {
            new ValidacionNitEmisorInvalido(),
            new ValidacionNitReceptorInvalido(),
            new ValidacionNitEmisorInexistente(),
            new ValidacionNitReceptorInexistente(),
            new ValidacionIva(),
            new ValidacionTotal(),
            new ValidacionReferencia()
        };

        public AutorizacionService(ConexionBD conexionBD, NitService nitService)
        {
            this.conexionBD = conexionBD;
            this.nitService = nitService;
        }

        public ResultadoAutorizaciones Procesar(LecturaFacturas lectura)
        {
            using var conexion = conexionBD.ObtenerConexion();
            // Todo el mensaje es una transaccion: si algo falla no queda la mitad guardada
            // ni se pierden correlativos
            using var transaccion = conexion.BeginTransaction();

            var contexto = new ContextoValidacion(nitService,
                LeerColumna(conexion, transaccion, "SELECT nit FROM Contribuyente;"),
                LeerColumna(conexion, transaccion, "SELECT referencia FROM Factura WHERE codigo_apr IS NOT NULL;"));

            // El codigo usa la fecha en que se pide la autorizacion (hoy), no la de la factura
            DateTime ahora = DateTime.Now;
            string hoy = ahora.ToString("yyyy-MM-dd");
            long correlativo = LeerCorrelativo(conexion, transaccion, hoy);

            foreach (Factura factura in lectura.Facturas)
            {
                foreach (Validacion validacion in validaciones)
                    validacion.Aplicar(factura, contexto);

                if (!factura.TieneErrores)
                {
                    correlativo++;
                    factura.Aprobar(ahora.ToString("yyyyMMdd") + correlativo.ToString("D8"));
                    contexto.RegistrarReferencia(factura.Referencia);
                }

                GuardarFactura(conexion, transaccion, factura, ahora);
            }

            GuardarCorrelativo(conexion, transaccion, hoy, correlativo);
            transaccion.Commit();

            return new ResultadoAutorizaciones(lectura.Facturas, lectura.Descartadas);
        }

        private static HashSet<string> LeerColumna(SqliteConnection conexion, SqliteTransaction transaccion, string sql)
        {
            var valores = new HashSet<string>();

            using var comando = conexion.CreateCommand();
            comando.Transaction = transaccion;
            comando.CommandText = sql;

            using SqliteDataReader lector = comando.ExecuteReader();
            while (lector.Read())
                valores.Add(lector.GetString(0));

            return valores;
        }

        private static long LeerCorrelativo(SqliteConnection conexion, SqliteTransaction transaccion, string fecha)
        {
            using var comando = conexion.CreateCommand();
            comando.Transaction = transaccion;
            comando.CommandText = "SELECT ultimo FROM Correlativo WHERE fecha = $fecha;";
            comando.Parameters.AddWithValue("$fecha", fecha);

            object? ultimo = comando.ExecuteScalar();
            return ultimo == null ? 0 : Convert.ToInt64(ultimo);
        }

        private static void GuardarCorrelativo(SqliteConnection conexion, SqliteTransaction transaccion, string fecha, long ultimo)
        {
            using var comando = conexion.CreateCommand();
            comando.Transaction = transaccion;
            comando.CommandText = @"
                INSERT INTO Correlativo (fecha, ultimo) VALUES ($fecha, $ultimo)
                ON CONFLICT(fecha) DO UPDATE SET ultimo = $ultimo;";
            comando.Parameters.AddWithValue("$fecha", fecha);
            comando.Parameters.AddWithValue("$ultimo", ultimo);
            comando.ExecuteNonQuery();
        }

        private static void GuardarFactura(SqliteConnection conexion, SqliteTransaction transaccion, Factura factura, DateTime recepcion)
        {
            using var insertar = conexion.CreateCommand();
            insertar.Transaction = transaccion;
            insertar.CommandText = @"
                INSERT INTO Factura (referencia, fecha, nit_emisor, nit_receptor, valor, iva, total, codigo_apr, fecha_recepcion)
                VALUES ($referencia, $fecha, $emisor, $receptor, $valor, $iva, $total, $codigo, $recepcion);
                SELECT last_insert_rowid();";
            insertar.Parameters.AddWithValue("$referencia", factura.Referencia);
            insertar.Parameters.AddWithValue("$fecha", factura.Fecha.ToString("yyyy-MM-dd HH:mm:ss"));
            insertar.Parameters.AddWithValue("$emisor", factura.NitEmisor);
            insertar.Parameters.AddWithValue("$receptor", factura.NitReceptor);
            insertar.Parameters.AddWithValue("$valor", factura.Valor);
            insertar.Parameters.AddWithValue("$iva", factura.Iva);
            insertar.Parameters.AddWithValue("$total", factura.Total);
            insertar.Parameters.AddWithValue("$codigo", (object?)factura.CodigoAprobacion ?? DBNull.Value);
            insertar.Parameters.AddWithValue("$recepcion", recepcion.ToString("yyyy-MM-dd HH:mm:ss"));

            long idFactura = Convert.ToInt64(insertar.ExecuteScalar());

            foreach (ErrorValidacion error in factura.Errores)
            {
                using var insertarError = conexion.CreateCommand();
                insertarError.Transaction = transaccion;
                insertarError.CommandText = @"
                    INSERT INTO Error_Validacion (id_factura, tipo, descripcion)
                    VALUES ($id, $tipo, $descripcion);";
                insertarError.Parameters.AddWithValue("$id", idFactura);
                insertarError.Parameters.AddWithValue("$tipo", (int)error.Tipo);
                insertarError.Parameters.AddWithValue("$descripcion", error.Descripcion);
                insertarError.ExecuteNonQuery();
            }
        }
    }
}