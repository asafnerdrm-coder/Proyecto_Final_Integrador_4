using System.Text;
using TramiteAgil.Consola;
using TramiteAgil.Servicios;

namespace TramiteAgil;

public static class Program
{
    public static int Main(string[] args)
    {
        try
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;
        }
        catch (Exception)
        {
            // Si la consola no permite cambiar la codificación, se continúa igualmente.
        }

        string modo = args.Length > 0 ? args[0].ToLower() : "";

        if (modo == "--test") return Pruebas.Ejecutar();

        var sistema = new SistemaTramites();
        if (modo == "--demo")
        {
            Demo.Ejecutar(sistema);
            return 0;
        }

        try
        {
            new Menu(sistema).Ejecutar();
        }
        catch (EndOfStreamException)
        {
            Console.WriteLine();
            Console.WriteLine("Entrada finalizada. Hasta pronto.");
        }
        return 0;
    }
}
