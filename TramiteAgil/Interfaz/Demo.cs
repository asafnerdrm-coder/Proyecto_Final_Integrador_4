using TramiteAgil.Modelos;
using TramiteAgil.Servicios;

namespace TramiteAgil.Consola;

/// <summary>Demostración automática (sin entrada del usuario) pensada para el video de defensa.</summary>
public static class Demo
{
    private static Solicitud Solicitar(SistemaTramites sis, string nombre, string ci, TipoTramite tipo, params string[] omitir)
    {
        var hojas = new List<string>();
        foreach (Requisito req in tipo.Requisitos) ValidadorRequisitos.ListarHojas(req, hojas);
        var presentados = hojas.Where(h => !omitir.Contains(h)).ToList();

        var faltantes = new List<string>();
        bool cumple = ValidadorRequisitos.ValidarLista(tipo.Requisitos, 0, presentados, faltantes);
        Console.WriteLine($"- {nombre} solicita [{tipo.Codigo}] -> validación recursiva: {(cumple ? "CUMPLE" : "INCOMPLETO")}");
        if (!cumple)
        {
            foreach (string f in faltantes) Console.WriteLine($"      falta: {f}");
            Console.WriteLine("      (el ciudadano subsana presentando lo que faltaba)");
        }

        Solicitud sol = sis.RegistrarSolicitud(new Ciudadano(nombre, ci), tipo);
        Turno turno = sis.GenerarTurno(sol);
        Console.WriteLine($"      Solicitud #{sol.Id} registrada, turno {turno.Codigo} encolado");
        return sol;
    }

    private static void Atender(SistemaTramites sis)
    {
        if (sis.AtenderSiguiente(out Turno turno, out Solicitud sol, out Ventanilla ven))
        {
            Vista.Atencion(turno, sol, ven);
        }
        else
        {
            Console.WriteLine("No hay turnos en espera.");
        }
    }

    private static void Accion(Solicitud sol, bool avanzar, string motivo = "")
    {
        string mensaje;
        if (avanzar) sol.Avanzar(out mensaje);
        else sol.Retroceder(motivo, out mensaje);
        Console.WriteLine($"  #{sol.Id}: {mensaje}");
    }

    public static void Ejecutar(SistemaTramites sis)
    {
        TipoTramite cnp = sis.Catalogo[0];
        TipoTramite cor = sis.Catalogo[1];
        TipoTramite tra = sis.Catalogo[2];

        Vista.Titulo("1. CATÁLOGO (arreglo) - 3 trámites de Derechos Reales");
        Vista.ListaTramites(sis.Catalogo);

        Vista.Titulo("2. REGISTRO, VALIDACIÓN RECURSIVA Y GENERACIÓN DE TURNOS (cola)");
        Solicitud ana = Solicitar(sis, "Ana Mamani", "4567890", cnp);
        Solicitud bruno = Solicitar(sis, "Bruno Quispe", "5678901", cor, "Protocolización ante Notario de Fe Pública");
        Solicitud carla = Solicitar(sis, "Carla Flores", "6789012", tra);
        Solicitud diego = Solicitar(sis, "Diego Condori", "7890123", cnp);
        Console.WriteLine();
        Vista.Cola(sis);

        Vista.Titulo("3. ATENCIÓN POR TURNOS (cola FIFO) CON ROTACIÓN DE VENTANILLAS (lista circular)");
        Atender(sis);
        Atender(sis);
        Atender(sis);
        Atender(sis); // la cuarta vuelve a la ventanilla 1
        Atender(sis); // cola vacía
        Console.WriteLine();
        Vista.Cola(sis);

        Vista.Titulo("4. FLUJO DE PASOS (lista doblemente enlazada): avanzar, observar y retroceder");
        Console.WriteLine("Ana [CNP, 3 pasos]: avanza hasta finalizar");
        Accion(ana, true);
        Accion(ana, true);
        Console.WriteLine();
        Console.WriteLine("Bruno [COR, 4 pasos]: avanza, es observado (retrocede) y vuelve a avanzar");
        Accion(bruno, true);
        Accion(bruno, false, "falta legalizar la minuta");
        Accion(bruno, true);
        Console.WriteLine();
        Console.WriteLine("Carla [TRA, 6 pasos]: avanza dos pasos");
        Accion(carla, true);
        Accion(carla, true);

        Vista.Titulo("5. SEGUIMIENTO DEL TRÁMITE (paso actual y pasos que faltan)");
        Console.WriteLine(ana.Seguimiento());
        Console.WriteLine();
        Console.WriteLine(bruno.Seguimiento());
        Console.WriteLine();
        Console.WriteLine(carla.Seguimiento());
        Console.WriteLine();
        Console.WriteLine(diego.Seguimiento());

        Vista.Titulo("6. HISTORIAL DE ESTADOS (pila) Y DESHACER");
        Vista.Historial(bruno);
        Console.WriteLine();
        bruno.Deshacer(out string msg);
        Console.WriteLine("Deshacer (Pop): " + msg);
        Console.WriteLine();
        Vista.Historial(bruno);

        Vista.Titulo("7. MODELADO DEL FLUJO COMO GRAFO");
        Vista.GrafoTramite(tra);

        Vista.Titulo("FIN DE LA DEMOSTRACIÓN");
    }
}
