using SAT_ControlDTE.Models;

namespace SAT_ControlDTE.Validations
{
    public abstract class Validacion
    {
        public abstract TipoError Tipo { get; }

        protected abstract string? Revisar(Factura factura, ContextoValidacion contexto);

        public void Aplicar(Factura factura, ContextoValidacion contexto)
        {
            string? error = Revisar(factura, contexto);
            if (error != null)
                factura.AgregarError(Tipo, error);
        }
    }
}