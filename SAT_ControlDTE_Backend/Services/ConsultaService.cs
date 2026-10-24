using Microsoft.Data.Sqlite;
using SAT_ControlDTE.DataBase;
using SAT_ControlDTE.Validations;
using SAT_ControlDTE.Models;

namespace SAT_ControlDTE.Services
{
    public class ConsultaService
    {
        private readonly ConexionBD conexionBD;

        public ConsultaService(ConexionBD conexionBD)
        {
            this.conexionBD = conexionBD;
        }

        public object ConsultarContribuyente(string nit, bool emitidas, DateTime desde, DateTime hasta)
        {
            string columna = emitidas ? "nit_emisor" : "nit_receptor";

            using var conexion = conexionBD.ObtenerConexion();
            using var comando = conexion.CreateCommand();
            comando.CommandText = $@"
                SELECT referencia, fecha, nit_emisor, nit_receptor, valor, iva, total, codigo_apr
                FROM Factura
                WHERE codigo_apr IS NOT NULL
                AND {columna} = $nit
                AND date(fecha) BETWEEN $desde AND $hasta
                ORDER BY fecha, codigo_apr;";
            comando.Parameters.AddWithValue("$nit", nit);
            AgregarRango(comando, desde, hasta);

            var facturas = new List<FacturaConsulta>();
            using (SqliteDataReader lector = comando.ExecuteReader())
            {
                while (lector.Read())
                {
                    facturas.Add(new FacturaConsulta
                    {
                        Referencia = lector.GetString(0),
                        Fecha = DateTime.Parse(lector.GetString(1)),
                        NitEmisor = lector.GetString(2),
                        NitReceptor = lector.GetString(3),
                        Valor = LeerMonto(lector, 4),
                        Iva = LeerMonto(lector, 5),
                        Total = LeerMonto(lector, 6),
                        CodigoAprobacion = lector.GetString(7)
                    });
                }
            }

            var porFecha = facturas
                .GroupBy(f => f.Fecha.Date)
                .Select(g => new
                {
                    fecha = FormatoFecha(g.Key),
                    cantidadFacturas = g.Count(),
                    montoTotal = g.Sum(f => f.Total)
                })
                .ToList();

            return new
            {
                nit,
                nombre = BuscarNombre(conexion, nit),
                tipo = emitidas ? "emitidas" : "recibidas",
                desde = FormatoFecha(desde),
                hasta = FormatoFecha(hasta),
                fechas = porFecha,
                cantidadFacturas = facturas.Count,
                montoTotal = facturas.Sum(f => f.Total),
                facturas = facturas.Select(f => new
                {
                    referencia = f.Referencia,
                    fecha = f.Fecha.ToString("dd/MM/yyyy HH:mm"),
                    nitEmisor = f.NitEmisor,
                    nitReceptor = f.NitReceptor,
                    valor = f.Valor,
                    iva = f.Iva,
                    total = f.Total,
                    codigoAprobacion = long.Parse(f.CodigoAprobacion)
                })
            };
        }

        public object ResumenIva(DateTime fecha)
        {
            using var conexion = conexionBD.ObtenerConexion();
            using var comando = conexion.CreateCommand();
            
            comando.CommandText = @"
                SELECT m.nit, c.nombre, SUM(m.emitido), SUM(m.recibido)
                FROM (
                    SELECT nit_emisor AS nit, iva AS emitido, 0 AS recibido
                    FROM Factura WHERE codigo_apr IS NOT NULL AND date(fecha) = $fecha
                    UNION ALL
                    SELECT nit_receptor AS nit, 0 AS emitido, iva AS recibido
                    FROM Factura WHERE codigo_apr IS NOT NULL AND date(fecha) = $fecha
                ) m
                LEFT JOIN Contribuyente c ON c.nit = m.nit
                GROUP BY m.nit, c.nombre
                ORDER BY m.nit;";
            comando.Parameters.AddWithValue("$fecha", fecha.ToString("yyyy-MM-dd"));

            var contribuyentes = new List<object>();
            using SqliteDataReader lector = comando.ExecuteReader();
            while (lector.Read())
            {
                contribuyentes.Add(new
                {
                    nit = lector.GetString(0),
                    nombre = lector.IsDBNull(1) ? "" : lector.GetString(1),
                    ivaEmitido = LeerMonto(lector, 2),
                    ivaRecibido = LeerMonto(lector, 3)
                });
            }

            return new { fecha = FormatoFecha(fecha), contribuyentes };
        }

        public object ResumenRango(DateTime desde, DateTime hasta)
        {
            using var conexion = conexionBD.ObtenerConexion();
            using var comando = conexion.CreateCommand();
            comando.CommandText = @"
                SELECT date(fecha) AS dia, COUNT(*), SUM(total), SUM(valor)
                FROM Factura
                WHERE codigo_apr IS NOT NULL AND date(fecha) BETWEEN $desde AND $hasta
                GROUP BY dia
                ORDER BY dia;";
            AgregarRango(comando, desde, hasta);

            var fechas = new List<ResumenDia>();
            using (SqliteDataReader lector = comando.ExecuteReader())
            {
                while (lector.Read())
                {
                    fechas.Add(new ResumenDia
                    {
                        Fecha = DateTime.Parse(lector.GetString(0)),
                        Cantidad = lector.GetInt32(1),
                        Total = LeerMonto(lector, 2),
                        SinIva = LeerMonto(lector, 3)
                    });
                }
            }

            return new
            {
                desde = FormatoFecha(desde),
                hasta = FormatoFecha(hasta),
                fechas = fechas.Select(d => new
                {
                    fecha = FormatoFecha(d.Fecha),
                    cantidadFacturas = d.Cantidad,
                    total = d.Total,
                    sinIva = d.SinIva
                }),
                totalGeneral = fechas.Sum(d => d.Total),
                totalSinIva = fechas.Sum(d => d.SinIva)
            };
        }

        public object ReporteErrores(DateTime desde, DateTime hasta)
        {
            using var conexion = conexionBD.ObtenerConexion();

            var dias = new SortedDictionary<string, ReporteDia>();
            using (var comando = conexion.CreateCommand())
            {
                comando.CommandText = @"
                    SELECT date(fecha) AS dia,
                        COUNT(*),
                        SUM(CASE WHEN codigo_apr IS NOT NULL THEN 1 ELSE 0 END),
                        COUNT(DISTINCT nit_emisor),
                        COUNT(DISTINCT nit_receptor)
                    FROM Factura
                    WHERE date(fecha) BETWEEN $desde AND $hasta
                    GROUP BY dia;";
                AgregarRango(comando, desde, hasta);

                using SqliteDataReader lector = comando.ExecuteReader();
                while (lector.Read())
                {
                    dias[lector.GetString(0)] = new ReporteDia
                    {
                        Recibidas = lector.GetInt32(1),
                        SinErrores = lector.GetInt32(2),
                        Emisores = lector.GetInt32(3),
                        Receptores = lector.GetInt32(4)
                    };
                }
            }

            using (var comando = conexion.CreateCommand())
            {
                comando.CommandText = @"
                    SELECT date(f.fecha) AS dia, e.tipo, COUNT(DISTINCT f.id_factura)
                    FROM Error_Validacion e
                    JOIN Factura f ON f.id_factura = e.id_factura
                    WHERE date(f.fecha) BETWEEN $desde AND $hasta
                    GROUP BY dia, e.tipo;";
                AgregarRango(comando, desde, hasta);

                using SqliteDataReader lector = comando.ExecuteReader();
                while (lector.Read())
                    dias[lector.GetString(0)].Errores[(TipoError)lector.GetInt32(1)] = lector.GetInt32(2);
            }

            return new
            {
                desde = FormatoFecha(desde),
                hasta = FormatoFecha(hasta),
                fechas = dias.Select(d => new
                {
                    fecha = FormatoFecha(DateTime.Parse(d.Key)),
                    cantidadFacturasRecibidas = d.Value.Recibidas,
                    errores = new
                    {
                        nitEmisorInvalido = d.Value.ContarError(TipoError.NitEmisorInvalido),
                        nitReceptorInvalido = d.Value.ContarError(TipoError.NitReceptorInvalido),
                        nitEmisorInexistente = d.Value.ContarError(TipoError.NitEmisorInexistente),
                        nitReceptorInexistente = d.Value.ContarError(TipoError.NitReceptorInexistente),
                        ivaMalCalculado = d.Value.ContarError(TipoError.IvaMalCalculado),
                        totalMalCalculado = d.Value.ContarError(TipoError.TotalMalCalculado),
                        referenciaDuplicada = d.Value.ContarError(TipoError.ReferenciaDuplicada)
                    },
                    cantidadFacturasSinErrores = d.Value.SinErrores,
                    cantidadEmisores = d.Value.Emisores,
                    cantidadReceptores = d.Value.Receptores
                })
            };
        }

        public object Totales()
        {
            using var conexion = conexionBD.ObtenerConexion();
            using var comando = conexion.CreateCommand();
            comando.CommandText = @"
                SELECT (SELECT COUNT(*) FROM Contribuyente),
                    (SELECT COUNT(*) FROM Factura WHERE codigo_apr IS NOT NULL),
                    (SELECT COUNT(*) FROM Factura WHERE codigo_apr IS NULL),
                    (SELECT SUM(total) FROM Factura WHERE codigo_apr IS NOT NULL);";

            using SqliteDataReader lector = comando.ExecuteReader();
            lector.Read();
            return new
            {
                contribuyentes = lector.GetInt32(0),
                facturasAutorizadas = lector.GetInt32(1),
                facturasRechazadas = lector.GetInt32(2),
                montoAutorizado = LeerMonto(lector, 3)
            };
        }

        private static string BuscarNombre(SqliteConnection conexion, string nit)
        {
            using var comando = conexion.CreateCommand();
            comando.CommandText = "SELECT nombre FROM Contribuyente WHERE nit = $nit;";
            comando.Parameters.AddWithValue("$nit", nit);
            return comando.ExecuteScalar() as string ?? "";
        }

        private static void AgregarRango(SqliteCommand comando, DateTime desde, DateTime hasta)
        {
            comando.Parameters.AddWithValue("$desde", desde.ToString("yyyy-MM-dd"));
            comando.Parameters.AddWithValue("$hasta", hasta.ToString("yyyy-MM-dd"));
        }

        private static decimal LeerMonto(SqliteDataReader lector, int columna)
        {
            if (lector.IsDBNull(columna))
                return 0;
            return Math.Round(Convert.ToDecimal(lector.GetDouble(columna)), 2);
        }

        private static string FormatoFecha(DateTime fecha)
        {
            return fecha.ToString("dd/MM/yyyy");
        }

        private class FacturaConsulta
        {
            public string Referencia { get; set; } = "";
            public DateTime Fecha { get; set; }
            public string NitEmisor { get; set; } = "";
            public string NitReceptor { get; set; } = "";
            public decimal Valor { get; set; }
            public decimal Iva { get; set; }
            public decimal Total { get; set; }
            public string CodigoAprobacion { get; set; } = "";
        }

        private class ResumenDia
        {
            public DateTime Fecha { get; set; }
            public int Cantidad { get; set; }
            public decimal Total { get; set; }
            public decimal SinIva { get; set; }
        }

        private class ReporteDia
        {
            public int Recibidas { get; set; }
            public int SinErrores { get; set; }
            public int Emisores { get; set; }
            public int Receptores { get; set; }
            public Dictionary<TipoError, int> Errores { get; } = new Dictionary<TipoError, int>();

            public int ContarError(TipoError tipo)
            {
                return Errores.TryGetValue(tipo, out int cantidad) ? cantidad : 0;
            }
        }
    }
}