using System;

namespace SAT_ControlDTE.Models
{
    public class ErrorValidacion
    {
        public ErrorValidacion(TipoError tipo, string descripcion)
        {
            Tipo = tipo;
            Descripcion = descripcion;
        }

        public TipoError Tipo { get; }
        public string Descripcion { get; }
    }
}