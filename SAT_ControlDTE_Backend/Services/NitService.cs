using System.Text.RegularExpressions;

namespace SAT_ControlDTE.Services
{
    public class NitService
    {
        // De 1 a 20 digitos seguidos del verificador (un digito o K)
        private static readonly Regex FormatoNit = new Regex(@"^[0-9]{1,20}[0-9K]$");

        // Quita espacios y pasa la k a mayuscula, asi "6k" y "6K" son el mismo NIT
        public string Normalizar(string? nit)
        {
            return (nit ?? "").Trim().ToUpperInvariant();
        }

        public bool EsValido(string? nit)
        {
            nit = Normalizar(nit);

            if (!FormatoNit.IsMatch(nit))
                return false;

            string cuerpo = nit.Substring(0, nit.Length - 1);
            char verificador = nit[nit.Length - 1];

            // 1. Cada digito por su posicion, contando desde la derecha.
            //    La posicion 1 es el verificador y no se multiplica, asi que el cuerpo empieza en 2.
            // 2. Se suman los resultados.
            int suma = 0;
            int posicion = 2;
            for (int i = cuerpo.Length - 1; i >= 0; i--)
            {
                suma += (cuerpo[i] - '0') * posicion;
                posicion++;
            }

            // 3, 4 y 5. (11 - suma % 11) % 11; si da 10 el verificador es K
            int resultado = (11 - (suma % 11)) % 11;
            char esperado = resultado == 10 ? 'K' : (char)('0' + resultado);

            return verificador == esperado;
        }
    }
}