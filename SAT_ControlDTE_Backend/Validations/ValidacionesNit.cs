using SAT_ControlDTE.Models;

namespace SAT_ControlDTE.Validations
{
    public class ValidacionNitEmisorInvalido : Validacion
    {
        public override TipoError Tipo => TipoError.NitEmisorInvalido;

        protected override string? Revisar(Factura factura, ContextoValidacion contexto)
        {
            if (contexto.NitService.EsValido(factura.NitEmisor))
                return null;
            return $"NIT emisor invalido: {factura.NitEmisor}";
        }
    }

    public class ValidacionNitReceptorInvalido : Validacion
    {
        public override TipoError Tipo => TipoError.NitReceptorInvalido;

        protected override string? Revisar(Factura factura, ContextoValidacion contexto)
        {
            if (contexto.NitService.EsValido(factura.NitReceptor))
                return null;
            return $"NIT receptor invalido: {factura.NitReceptor}";
        }
    }

    public class ValidacionNitEmisorInexistente : Validacion
    {
        public override TipoError Tipo => TipoError.NitEmisorInexistente;

        protected override string? Revisar(Factura factura, ContextoValidacion contexto)
        {
            if (!contexto.NitService.EsValido(factura.NitEmisor) || contexto.ExisteEnRtu(factura.NitEmisor))
                return null;
            return $"NIT emisor no existe en el RTU: {factura.NitEmisor}";
        }
    }

    public class ValidacionNitReceptorInexistente : Validacion
    {
        public override TipoError Tipo => TipoError.NitReceptorInexistente;

        protected override string? Revisar(Factura factura, ContextoValidacion contexto)
        {
            if (!contexto.NitService.EsValido(factura.NitReceptor) || contexto.ExisteEnRtu(factura.NitReceptor))
                return null;
            return $"NIT receptor no existe en el RTU: {factura.NitReceptor}";
        }
    }
}