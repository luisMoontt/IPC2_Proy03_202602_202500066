using System;

namespace SAT_ControlDTE.Models
{
    public enum TipoError
    {
        NitEmisorInvalido = 0,
        NitReceptorInvalido = 1,
        IvaMalCalculado = 3,
        TotalMalCalculado = 4,
        ReferenciaDuplicada = 5,
        NitEmisorInexistente = 6,
        NitReceptorInexistente = 7
    }
}