using TramiteAgil.Estructuras;
using TramiteAgil.Modelos;
using TramiteAgil.Servicios;

namespace TramiteAgil.Consola;

/// <summary>
/// Pruebas automáticas sin librerías externas. Ejecutar con:  dotnet run -- --test
/// Devuelve 0 si todas pasan y 1 si alguna falla.
/// </summary>
public static class Pruebas
{
    private static int _ok;
    private static int _fallas;

    private static void Verificar(bool condicion, string nombre)
    {
        if (condicion)
        {
            _ok++;
            Console.WriteLine($"  [OK]    {nombre}");
        }
        else
        {
            _fallas++;
            Console.WriteLine($"  [FALLA] {nombre}");
        }
    }

    private static List<string> TodosLosDocumentos(TipoTramite tipo)
    {
        var hojas = new List<string>();
        foreach (Requisito r in tipo.Requisitos) ValidadorRequisitos.ListarHojas(r, hojas);
        return hojas;
    }

    private static Solicitud NuevaConTurno(SistemaTramites sis, TipoTramite tipo, string nombre)
    {
        Solicitud sol = sis.RegistrarSolicitud(new Ciudadano(nombre, "1234567"), tipo);
        sis.GenerarTurno(sol);
        return sol;
    }

    public static int Ejecutar()
    {
        Console.WriteLine("PRUEBAS AUTOMÁTICAS - TramiteAgil");

        Console.WriteLine("\n[Pila]");
        var pila = new Pila<int>();
        pila.Push(1); pila.Push(2); pila.Push(3);
        Verificar(pila.Cantidad == 3, "Push x3 -> cantidad 3");
        Verificar(pila.Peek() == 3, "Peek devuelve el último ingresado");
        Verificar(pila.Pop() == 3 && pila.Pop() == 2 && pila.Pop() == 1, "Pop respeta el orden LIFO");
        Verificar(pila.EstaVacia, "La pila queda vacía");

        Console.WriteLine("\n[Cola]");
        var cola = new Cola<string>();
        cola.Encolar("A"); cola.Encolar("B"); cola.Encolar("C");
        Verificar(cola.Frente() == "A", "Frente es el primero en llegar");
        Verificar(cola.Desencolar() == "A" && cola.Desencolar() == "B" && cola.Desencolar() == "C", "Desencolar respeta el orden FIFO");
        Verificar(cola.EstaVacia && cola.Cantidad == 0, "La cola queda vacía");
        cola.Encolar("X");
        Verificar(cola.Desencolar() == "X", "La cola funciona tras vaciarse");

        Console.WriteLine("\n[Lista doblemente enlazada]");
        var lista = new ListaDoble<string>();
        lista.AgregarAlFinal("A"); lista.AgregarAlFinal("B"); lista.AgregarAlFinal("C");
        Verificar(lista.Actual == "A" && lista.PosicionActual == 1, "El cursor empieza en el primer elemento");
        Verificar(!lista.HayAnterior && lista.HaySiguiente, "En el inicio no hay anterior pero sí siguiente");
        lista.MoverSiguiente(); lista.MoverSiguiente();
        Verificar(lista.Actual == "C" && !lista.HaySiguiente, "Se llega al último elemento");
        Verificar(!lista.MoverSiguiente(), "No se puede avanzar más allá del último");
        lista.MoverAnterior();
        Verificar(lista.Actual == "B" && lista.PosicionActual == 2, "Retroceder funciona (navegación hacia atrás)");
        Verificar(lista.IrA(1) && lista.Actual == "A", "IrA(1) vuelve al inicio");
        Verificar(!lista.IrA(9), "IrA fuera de rango devuelve false");

        Console.WriteLine("\n[Lista circular]");
        var circular = new ListaCircular<int>();
        circular.Agregar(1); circular.Agregar(2); circular.Agregar(3);
        Verificar(circular.Rotar() == 1 && circular.Rotar() == 2 && circular.Rotar() == 3 && circular.Rotar() == 1,
            "La rotación da 1,2,3 y vuelve a 1");

        Console.WriteLine("\n[Catálogo y requisitos]");
        var sis = new SistemaTramites();
        Verificar(sis.Catalogo.Length == 3, "El catálogo tiene 3 trámites");
        Verificar(sis.Catalogo[0].Pasos.Length == 3 && sis.Catalogo[1].Pasos.Length == 4 && sis.Catalogo[2].Pasos.Length == 6,
            "Los trámites tienen 3, 4 y 6 pasos (duraciones distintas)");
        foreach (TipoTramite t in sis.Catalogo)
        {
            var faltantes = new List<string>();
            bool ok = ValidadorRequisitos.ValidarLista(t.Requisitos, 0, TodosLosDocumentos(t), faltantes);
            Verificar(ok && faltantes.Count == 0, $"[{t.Codigo}] con todos los documentos CUMPLE");
        }
        TipoTramite transf = sis.Catalogo[2];
        var presentados = TodosLosDocumentos(transf);
        presentados.Remove("Cédula del comprador");
        var falt = new List<string>();
        bool cumple = ValidadorRequisitos.ValidarLista(transf.Requisitos, 0, presentados, falt);
        Verificar(!cumple && falt.Count == 1 && falt[0] == "Cédula del comprador",
            "La validación recursiva detecta un documento faltante anidado");

        Console.WriteLine("\n[Grafo de flujo]");
        foreach (TipoTramite t in sis.Catalogo)
        {
            Grafo g = ConstructorGrafo.Crear(t);
            int fin = g.NumNodos - 1;
            Verificar(g.NumNodos == t.Pasos.Length + 1, $"[{t.Codigo}] el grafo tiene {t.Pasos.Length} pasos + nodo FINALIZADO");
            Verificar(g.RutaMasCorta(0, fin).Length == t.Pasos.Length + 1, $"[{t.Codigo}] la ruta más corta recorre todos los pasos");
            Verificar(g.RecorridoDFS(0).Length == g.NumNodos, $"[{t.Codigo}] el DFS visita todos los nodos");
            Verificar(g.TieneCiclos(), $"[{t.Codigo}] se detectan reprocesos (ciclos) por observaciones");
            Verificar(!g.ExisteCamino(fin, 0), $"[{t.Codigo}] desde FINALIZADO no se puede volver");
        }

        Console.WriteLine("\n[Flujo completo de una solicitud]");
        var s1 = new SistemaTramites();
        Solicitud sol = s1.RegistrarSolicitud(new Ciudadano("Prueba Uno", "1111111"), s1.Catalogo[0]);
        Verificar(sol.Estado == EstadoSolicitud.Registrada, "Estado inicial: Registrada");
        Verificar(!sol.Avanzar(out _), "No se puede avanzar sin haber sido atendida");
        s1.GenerarTurno(sol);
        Verificar(sol.Estado == EstadoSolicitud.EnEspera && s1.ColaTurnos.Cantidad == 1, "Generar turno: EnEspera y 1 en cola");
        Verificar(s1.AtenderSiguiente(out Turno tu, out Solicitud atendida, out Ventanilla ven), "Se atiende el primer turno");
        Verificar(atendida.Id == sol.Id && ven.Nombre == "Ventanilla 1", "Se atendió la solicitud correcta en la Ventanilla 1");
        Verificar(sol.Estado == EstadoSolicitud.EnProceso && sol.Flujo.PosicionActual == 2, "Tras atender queda EnProceso en el paso 2");
        Verificar(sol.Avanzar(out _) && sol.Flujo.PosicionActual == 3, "Avanza al paso 3");
        Verificar(sol.Avanzar(out _) && sol.Estado == EstadoSolicitud.Finalizada, "Al completar el último paso queda Finalizada");
        Verificar(!sol.Avanzar(out _), "Una solicitud finalizada no avanza");
        Verificar(sol.Historial.Cantidad == 6, "El historial (pila) tiene 6 transiciones");
        Verificar(sol.Historial.Peek().Estado == EstadoSolicitud.Finalizada, "El tope de la pila es el estado más reciente");
        Verificar(!s1.AtenderSiguiente(out _, out _, out _), "Con la cola vacía no hay a quién atender");

        Console.WriteLine("\n[Observar y deshacer]");
        var s2 = new SistemaTramites();
        Solicitud sol2 = NuevaConTurno(s2, s2.Catalogo[1], "Prueba Dos");
        s2.AtenderSiguiente(out _, out _, out _);
        Verificar(sol2.Retroceder("prueba", out _) && sol2.Estado == EstadoSolicitud.Observada && sol2.Flujo.PosicionActual == 1,
            "Observación regresa por arista del grafo (estado Observada)");
        Verificar(sol2.Deshacer(out _) && sol2.Estado == EstadoSolicitud.EnProceso && sol2.Flujo.PosicionActual == 2,
            "Deshacer restaura el estado y el paso anteriores");
        Verificar(!sol2.Deshacer(out _), "No se puede deshacer la atención ya realizada");

        Console.WriteLine("\n[Cola de turnos y rotación de ventanillas]");
        var s3 = new SistemaTramites();
        Solicitud a = NuevaConTurno(s3, s3.Catalogo[0], "A");
        Solicitud b = NuevaConTurno(s3, s3.Catalogo[1], "B");
        Solicitud c = NuevaConTurno(s3, s3.Catalogo[2], "C");
        Solicitud d = NuevaConTurno(s3, s3.Catalogo[0], "D");
        s3.AtenderSiguiente(out Turno t1, out _, out Ventanilla v1);
        s3.AtenderSiguiente(out Turno t2, out _, out Ventanilla v2);
        s3.AtenderSiguiente(out Turno t3, out _, out Ventanilla v3);
        s3.AtenderSiguiente(out Turno t4, out _, out Ventanilla v4);
        Verificar(t1.SolicitudId == a.Id && t2.SolicitudId == b.Id && t3.SolicitudId == c.Id && t4.SolicitudId == d.Id,
            "Los turnos se atienden en orden de llegada (FIFO)");
        Verificar(v1.Numero == 1 && v2.Numero == 2 && v3.Numero == 3 && v4.Numero == 1,
            "Las ventanillas rotan 1,2,3 y vuelven a la 1");

        Console.WriteLine();
        Console.WriteLine($"Resultado: {_ok} correctas, {_fallas} con falla.");
        return _fallas == 0 ? 0 : 1;
    }
}
