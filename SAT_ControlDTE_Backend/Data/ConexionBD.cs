using Microsoft.Data.Sqlite;

namespace SAT_ControlDTE.DataBase
{
    public class ConexionBD
    {
        private readonly string cadenaConexion;

        public ConexionBD(IConfiguration configuracion, IWebHostEnvironment entorno)
        {
            var datos = new SqliteConnectionStringBuilder(
                configuracion.GetConnectionString("SQLite") ?? "Data Source=Database/sat.db");

            // La ruta del appsettings es relativa a la carpeta del proyecto (no a donde se ejecute dotnet run)
            if (!Path.IsPathRooted(datos.DataSource))
                datos.DataSource = Path.Combine(entorno.ContentRootPath, datos.DataSource);

            Directory.CreateDirectory(Path.GetDirectoryName(datos.DataSource)!);
            cadenaConexion = datos.ToString();
        }

        public SqliteConnection ObtenerConexion()
        {
            var conexion = new SqliteConnection(cadenaConexion);
            conexion.Open();

            // SQLite no revisa las llaves foraneas si no se activa en cada conexion
            using var comando = conexion.CreateCommand();
            comando.CommandText = "PRAGMA foreign_keys = ON;";
            comando.ExecuteNonQuery();

            return conexion;
        }

        public void CrearTablas()
        {
            using var conexion = ObtenerConexion();
            using var comando = conexion.CreateCommand();
            comando.CommandText = Esquema;
            comando.ExecuteNonQuery();
        }

        // Borra todos los datos y deja el sistema como recien instalado
        public void Inicializar()
        {
            using var conexion = ObtenerConexion();
            using var comando = conexion.CreateCommand();
            comando.CommandText = @"
                DELETE FROM Error_Validacion;
                DELETE FROM Factura;
                DELETE FROM Contribuyente;
                DELETE FROM Correlativo;
                DELETE FROM sqlite_sequence;";
            comando.ExecuteNonQuery();
        }

        // Fechas guardadas como texto 'yyyy-MM-dd HH:mm:ss' para que date() y BETWEEN funcionen.
        private const string Esquema = @"
            -- Contribuyentes validos del RTU
            CREATE TABLE IF NOT EXISTS Contribuyente (
                id_contribuyente INTEGER PRIMARY KEY AUTOINCREMENT,
                nit              TEXT NOT NULL UNIQUE,
                nombre           TEXT NOT NULL
            );

            -- Todas las facturas recibidas con formato correcto.
            -- codigo_apr NULL = factura rechazada (sus errores estan en Error_Validacion).
            CREATE TABLE IF NOT EXISTS Factura (
                id_factura      INTEGER PRIMARY KEY AUTOINCREMENT,
                referencia      TEXT NOT NULL,
                fecha           TEXT NOT NULL,
                nit_emisor      TEXT NOT NULL,
                nit_receptor    TEXT NOT NULL,
                valor           REAL NOT NULL,
                iva             REAL NOT NULL,
                total           REAL NOT NULL,
                codigo_apr      TEXT UNIQUE,
                fecha_recepcion TEXT NOT NULL
            );

            -- Una referencia no se puede repetir entre las facturas aprobadas
            CREATE UNIQUE INDEX IF NOT EXISTS UX_Factura_Referencia
                ON Factura(referencia) WHERE codigo_apr IS NOT NULL;

            CREATE INDEX IF NOT EXISTS IX_Factura_Fecha ON Factura(fecha);

            -- Errores de cada factura rechazada (una factura puede tener varios)
            CREATE TABLE IF NOT EXISTS Error_Validacion (
                id_error    INTEGER PRIMARY KEY AUTOINCREMENT,
                id_factura  INTEGER NOT NULL,
                tipo        INTEGER NOT NULL,
                descripcion TEXT NOT NULL,
                FOREIGN KEY (id_factura) REFERENCES Factura(id_factura) ON DELETE CASCADE
            );

            -- Ultimo correlativo usado en cada dia
            CREATE TABLE IF NOT EXISTS Correlativo (
                fecha  TEXT PRIMARY KEY,
                ultimo INTEGER NOT NULL DEFAULT 0
            );";
    }
}