using System;
using SAT_ControlDTE.Models;

namespace SAT_ControlDTE.Models
{
    public class Factura
    {
        private readonly List<ErrorValidacion> errores = new List<ErrorValidacion>();
        private string? codigoAprobacion;

        public Factura(string referencia, DateTime fecha, string nitEmisor, string nitReceptor,
                    decimal valor, decimal iva, decimal total)
        {
            Referencia = referencia;
            Fecha = fecha;
            NitEmisor = nitEmisor;
            NitReceptor = nitReceptor;
            Valor = valor;
            Iva = iva;
            Total = total;
        }

        public string Referencia { get; }
        public DateTime Fecha { get; }
        public string NitEmisor { get; }
        public string NitReceptor { get; }
        public decimal Valor { get; }
        public decimal Iva { get; }
        public decimal Total { get; }

        public string? CodigoAprobacion => codigoAprobacion;
        public bool Aprobada => codigoAprobacion != null;
        public bool TieneErrores => errores.Count > 0;
        public IReadOnlyList<ErrorValidacion> Errores => errores;

        public void AgregarError(TipoError tipo, string descripcion)
        {
            errores.Add(new ErrorValidacion(tipo, descripcion));
        }

        public bool TieneError(TipoError tipo)
        {
            return errores.Any(e => e.Tipo == tipo);
        }

        public void Aprobar(string codigo)
        {
            if (TieneErrores)
                throw new InvalidOperationException($"La factura {Referencia} tiene errores y no se puede aprobar.");

            codigoAprobacion = codigo;
        }
    }
    
}