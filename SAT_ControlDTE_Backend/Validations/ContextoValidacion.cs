using SAT_ControlDTE.Models;
using SAT_ControlDTE.Services;

namespace SAT_ControlDTE.Validations
{
    public class ContextoValidacion
    {
        private readonly HashSet<string> nitsRtu;
        private readonly HashSet<string> referenciasAprobadas;

        public ContextoValidacion(NitService nitService, HashSet<string> nitsRtu, HashSet<string> referenciasAprobadas)
        {
            NitService = nitService;
            this.nitsRtu = nitsRtu;
            this.referenciasAprobadas = referenciasAprobadas;
        }

        public NitService NitService { get; }

        public bool ExisteEnRtu(string nit)
        {
            return nitsRtu.Contains(nit);
        }

        public bool ReferenciaUsada(string referencia)
        {
            return referenciasAprobadas.Contains(referencia);
        }

        // Se llama al aprobar una factura, para detectar referencias repetidas dentro del mismo mensaje
        public void RegistrarReferencia(string referencia)
        {
            referenciasAprobadas.Add(referencia);
        }
    }
}