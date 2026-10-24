using System;

namespace SAT_ControlDTE.Models
{

    public class Contribuyente
    {
        public Contribuyente(string nit, string nombre)
        {
            Nit = nit;
            Nombre = nombre;
        }

        public string Nit { get; }
        public string Nombre { get; }
    }

}
