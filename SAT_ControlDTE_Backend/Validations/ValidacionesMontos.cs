using SAT_ControlDTE.Models;

namespace SAT_ControlDTE.Validations
{
    public class ValidacionIva : Validacion
    {
        public override TipoError Tipo => TipoError.IvaMalCalculado;

        protected override string? Revisar(Factura factura, ContextoValidacion contexto)
        {
            decimal esperado = Math.Round(factura.Valor * 0.12m, 2, MidpointRounding.AwayFromZero);
            if (factura.Iva == esperado)
                return null;
            return $"IVA mal calculado: se recibio {factura.Iva}, se esperaba {esperado}";
        }
    }

    public class ValidacionTotal : Validacion
    {
        public override TipoError Tipo => TipoError.TotalMalCalculado;

        protected override string? Revisar(Factura factura, ContextoValidacion contexto)
        {
            decimal esperado = factura.Valor + factura.Iva;
            if (factura.Total == esperado)
                return null;
            return $"TOTAL mal calculado: se recibio {factura.Total}, se esperaba {esperado}";
        }
    }

    public class ValidacionReferencia : Validacion
    {
        public override TipoError Tipo => TipoError.ReferenciaDuplicada;

        protected override string? Revisar(Factura factura, ContextoValidacion contexto)
        {
            if (!contexto.ReferenciaUsada(factura.Referencia))
                return null;
            return $"REFERENCIA duplicada: {factura.Referencia}";
        }
    }
}