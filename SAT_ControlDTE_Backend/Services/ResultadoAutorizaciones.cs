using SAT_ControlDTE.Models;

namespace SAT_ControlDTE.Services
{
    public class ResultadoAutorizaciones
    {
        private readonly List<Factura> facturas;
        private readonly int descartadas;

        public ResultadoAutorizaciones(List<Factura> facturas, int descartadas)
        {
            this.facturas = facturas;
            this.descartadas = descartadas;
        }

        private int Contar(TipoError tipo)
        {
            return facturas.Count(f => f.TieneError(tipo));
        }

        public object CrearRespuesta()
        {
            return new
            {
                resultadoAutorizaciones = new
                {
                    cantidadAutorizacionesAprobadas = facturas.Count(f => f.Aprobada),
                    cantidadAutorizacionesRechazadasPorNitEmisorInexistente = Contar(TipoError.NitEmisorInexistente),
                    cantidadAutorizacionesRechazadasPorNitReceptorInexistente = Contar(TipoError.NitReceptorInexistente),
                    cantidadAutorizacionesRechazadasPorNitEmisorInvalido = Contar(TipoError.NitEmisorInvalido),
                    cantidadAutorizacionesRechazadasPorNitReceptorInvalido = Contar(TipoError.NitReceptorInvalido),
                    cantidadAutorizacionesRechazadasPorIvaMalCalculado = Contar(TipoError.IvaMalCalculado),
                    cantidadAutorizacionesRechazadasPorTotalMalCalculado = Contar(TipoError.TotalMalCalculado),
                    cantidadAutorizacionesRechazadasPorReferenciaDuplicada = Contar(TipoError.ReferenciaDuplicada),
                    cantidadAutorizacionesRechazadas = facturas.Count(f => f.TieneErrores),
                    cantidadDescartadasPorFormato = descartadas,
                    detalleAprobaciones = facturas
                        .Where(f => f.Aprobada)
                        .Select(f => new { referencia = f.Referencia, codigoAprobacion = long.Parse(f.CodigoAprobacion!) }),
                    detalleRechazos = facturas
                        .Where(f => f.TieneErrores)
                        .Select(f => new { referencia = f.Referencia, errores = f.Errores.Select(e => e.Descripcion) })
                }
            };
        }
    }
}