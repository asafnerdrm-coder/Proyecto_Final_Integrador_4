using TramiteAgil.Modelos;
using TramiteAgil.Servicios;

namespace TramiteAgil.Consola;

/// <summary>Menú interactivo de consola.</summary>
public class Menu
{
    private readonly SistemaTramites _sis;

    public Menu(SistemaTramites sis)
    {
        _sis = sis;
    }

    // ---------- Utilidades de entrada ----------

    public static string Leer(string mensaje)
    {
        Console.Write(mensaje);
        string linea = Console.ReadLine();
        if (linea == null) throw new EndOfStreamException();
        return linea.Trim();
    }

    private static string LeerNoVacio(string mensaje)
    {
        while (true)
        {
            string s = Leer(mensaje);
            if (s.Length > 0) return s;
            Console.WriteLine("  Este dato es obligatorio.");
        }
    }

    public static int LeerEntero(string mensaje, int min, int max)
    {
        while (true)
        {
            string s = Leer(mensaje);
            if (int.TryParse(s, out int n) && n >= min && n <= max) return n;
            Console.WriteLine($"  Ingrese un número entre {min} y {max}.");
        }
    }

    public static List<string> ParsearSeleccion(string seleccion, List<string> hojas)
    {
        var resultado = new List<string>();
        if (seleccion.ToLower() == "todos")
        {
            resultado.AddRange(hojas);
            return resultado;
        }
        foreach (string parte in seleccion.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries))
        {
            if (int.TryParse(parte, out int n) && n >= 1 && n <= hojas.Count && !resultado.Contains(hojas[n - 1]))
            {
                resultado.Add(hojas[n - 1]);
            }
        }
        return resultado;
    }

    // ---------- Bucle principal ----------

    public void Ejecutar()
    {
        Console.WriteLine("==================================================================");
        Console.WriteLine("  TRAMITEAGIL - Gestión de Trámites y Turnos (Derechos Reales)");
        Console.WriteLine("  Simulación académica registral | Estructura de Datos - UPDS");
        Console.WriteLine("==================================================================");

        bool salir = false;
        while (!salir)
        {
            MostrarMenu();
            string opcion = Leer("Opción: ");
            Console.WriteLine();
            switch (opcion)
            {
                case "1": Vista.Catalogo(_sis); break;
                case "2": NuevaSolicitud(); break;
                case "3": AtenderTurno(); break;
                case "4": AvanzarPaso(); break;
                case "5": ObservarPaso(); break;
                case "6": Seguimiento(); break;
                case "7": Historial(); break;
                case "8": VerGrafo(); break;
                case "9": Vista.Cola(_sis); break;
                case "10": Demo.Ejecutar(_sis); break;
                case "11": Subsanar(); break;
                case "0": salir = true; break;
                default: Console.WriteLine("Opción no válida."); break;
            }
            if (!salir) Leer("\nPresione Enter para continuar...");
        }
        Console.WriteLine("Hasta pronto.");
    }

    private static void MostrarMenu()
    {
        Console.WriteLine();
        Console.WriteLine("--------------------------- MENÚ ---------------------------");
        Console.WriteLine(" 1. Ver catálogo de trámites (arreglo)");
        Console.WriteLine(" 2. Nueva solicitud + validar requisitos + generar turno");
        Console.WriteLine(" 3. Atender siguiente turno (cola + ventanillas circulares)");
        Console.WriteLine(" 4. Avanzar un paso del trámite (lista doble)");
        Console.WriteLine(" 5. Observar trámite / retroceder un paso (lista doble)");
        Console.WriteLine(" 6. Seguimiento de una solicitud");
        Console.WriteLine(" 7. Historial de estados (pila) y deshacer");
        Console.WriteLine(" 8. Ver grafo de flujo de un trámite");
        Console.WriteLine(" 9. Ver cola de turnos y ventanillas");
        Console.WriteLine("10. Demostración automática completa");
        Console.WriteLine("11. Presentar documentos pendientes / subsanar");
        Console.WriteLine(" 0. Salir");
        Console.WriteLine("------------------------------------------------------------");
    }

    // ---------- Opciones ----------

    private void NuevaSolicitud()
    {
        Vista.ListaTramites(_sis.Catalogo);
        int indice = LeerEntero("Elija el trámite (número): ", 1, _sis.Catalogo.Length) - 1;
        TipoTramite tipo = _sis.Catalogo[indice];
        Console.WriteLine();
        Vista.DetalleTramite(tipo);
        Console.WriteLine();

        string nombre = LeerNoVacio("Nombre completo del ciudadano: ");
        string ci = LeerNoVacio("Cédula de identidad: ");

        var hojas = new List<string>();
        foreach (Requisito req in tipo.Requisitos) ValidadorRequisitos.ListarHojas(req, hojas);
        Console.WriteLine("\nDocumentos que se pueden presentar:");
        for (int i = 0; i < hojas.Count; i++) Console.WriteLine($"  {i + 1}. {hojas[i]}");

        string seleccion = Leer("Documentos que presenta (números separados por coma, 'todos' o Enter para ninguno): ");
        List<string> presentados = ParsearSeleccion(seleccion, hojas);

        var faltantes = new List<string>();
        bool cumple = ValidadorRequisitos.ValidarLista(tipo.Requisitos, 0, presentados, faltantes);
        Solicitud sol = _sis.RegistrarSolicitud(new Ciudadano(nombre, ci), tipo);
        sol.RegistrarDocumentos(presentados);
        if (!cumple)
        {
            Console.WriteLine("\nSolicitud registrada con observaciones:");
            foreach (string f in faltantes) Console.WriteLine("  - " + f);
            Console.WriteLine($"Solicitud #{sol.Id}. Use opción 11 para subsanar antes de generar turno.");
            return;
        }
        Turno turno = _sis.GenerarTurno(sol);
        Console.WriteLine($"Solicitud #{sol.Id} registrada. SU TURNO: {turno.Codigo}");
        Console.WriteLine($"Personas delante suyo: {_sis.ColaTurnos.Cantidad - 1}");
    }

    private void Subsanar()
    {
        Solicitud sol = PedirSolicitud();
        if (sol == null) return;
        var hojas = new List<string>();
        foreach (Requisito req in sol.Tipo.Requisitos) ValidadorRequisitos.ListarHojas(req, hojas);
        var faltantes = hojas.Where(h => !sol.DocumentosPresentados.Contains(h)).ToList();
        if (faltantes.Count == 0)
        {
            Console.WriteLine("No hay documentos pendientes.");
            return;
        }
        Console.WriteLine("Documentos pendientes:");
        for (int i = 0; i < faltantes.Count; i++) Console.WriteLine($"{i + 1}. {faltantes[i]}");
        string seleccion = Leer("Documentos entregados ahora (números separados por coma): ");
        sol.RegistrarDocumentos(ParsearSeleccion(seleccion, faltantes));
        if (sol.RequisitosPendientes) Console.WriteLine("Continúan existiendo requisitos pendientes.");
        else
        {
            Console.WriteLine("Documentos completos.");
            if (sol.Ventanilla != null)
                Console.WriteLine("El trámite puede continuar.");
            else if (sol.Estado == EstadoSolicitud.Registrada)
            {
                Turno turno = _sis.GenerarTurno(sol);
                Console.WriteLine($"Turno generado: {turno.Codigo}");
            }
        }
    }

    private void AtenderTurno()
    {
        if (!_sis.AtenderSiguiente(out Turno turno, out Solicitud sol, out Ventanilla ven))
        {
            Console.WriteLine("No hay turnos en espera.");
            return;
        }
        Vista.Atencion(turno, sol, ven);
        Console.WriteLine();
        Console.WriteLine(sol.Seguimiento());
    }

    private Solicitud PedirSolicitud()
    {
        if (_sis.Solicitudes.Count == 0)
        {
            Console.WriteLine("Aún no hay solicitudes registradas.");
            return null;
        }
        Console.WriteLine("Solicitudes:");
        foreach (Solicitud s in _sis.Solicitudes)
        {
            Console.WriteLine($"  #{s.Id}  {s.Ciudadano.Nombre}  -  {s.Tipo.Codigo}  -  {s.Estado}");
        }
        int id = LeerEntero("Número de solicitud: ", 1, int.MaxValue);
        Solicitud sol = _sis.Buscar(id);
        if (sol == null) Console.WriteLine("No existe esa solicitud.");
        return sol;
    }

    private void AvanzarPaso()
    {
        Solicitud sol = PedirSolicitud();
        if (sol == null) return;
        sol.Avanzar(out string mensaje);
        Console.WriteLine(mensaje);
        Console.WriteLine();
        Console.WriteLine(sol.Seguimiento());
    }

    private void ObservarPaso()
    {
        Solicitud sol = PedirSolicitud();
        if (sol == null) return;
        string motivo = Leer("Motivo de la observación (Enter = documentación inconsistente): ");
        if (motivo.Length == 0) motivo = "documentación inconsistente";
        sol.Retroceder(motivo, out string mensaje);
        Console.WriteLine(mensaje);
        Console.WriteLine();
        Console.WriteLine(sol.Seguimiento());
    }

    private void Seguimiento()
    {
        Solicitud sol = PedirSolicitud();
        if (sol == null) return;
        Console.WriteLine();
        Console.WriteLine(sol.Seguimiento());
    }

    private void Historial()
    {
        Solicitud sol = PedirSolicitud();
        if (sol == null) return;
        Console.WriteLine();
        Vista.Historial(sol);
        string resp = Leer("\n¿Deshacer la última transición (Pop)? (s/n): ").ToLower();
        if (resp == "s")
        {
            sol.Deshacer(out string mensaje);
            Console.WriteLine(mensaje);
            Console.WriteLine();
            Vista.Historial(sol);
        }
    }

    private void VerGrafo()
    {
        Vista.ListaTramites(_sis.Catalogo);
        int indice = LeerEntero("Elija el trámite (número): ", 1, _sis.Catalogo.Length) - 1;
        Console.WriteLine();
        Vista.GrafoTramite(_sis.Catalogo[indice]);
    }
}
