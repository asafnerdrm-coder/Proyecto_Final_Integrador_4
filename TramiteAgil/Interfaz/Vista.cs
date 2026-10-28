using TramiteAgil.Estructuras;
using TramiteAgil.Modelos;
using TramiteAgil.Servicios;

namespace TramiteAgil.Consola;

/// <summary>Funciones de impresión reutilizadas por el menú, la demo y las pruebas.</summary>
public static class Vista
{
    public static void Titulo(string texto)
    {
        Console.WriteLine();
        Console.WriteLine("==================================================================");
        Console.WriteLine("  " + texto);
        Console.WriteLine("==================================================================");
    }

    public static void ListaTramites(TipoTramite[] catalogo)
    {
        for (int i = 0; i < catalogo.Length; i++)
        {
            TipoTramite t = catalogo[i];
            Console.WriteLine($"  {i + 1}. [{t.Codigo}] {t.Nombre} - {t.Pasos.Length} pasos - {t.Duracion}");
        }
    }

    public static void DetalleTramite(TipoTramite t)
    {
        Console.WriteLine($"[{t.Codigo}] {t.Nombre}");
        Console.WriteLine($"  {t.Descripcion}");
        Console.WriteLine($"  Duración: {t.Duracion}");
        Console.WriteLine($"  Base: {t.Fuente}");
        Console.WriteLine("  Requisitos (+ compuesto, - documento):");
        foreach (Requisito r in t.Requisitos) Console.Write(ValidadorRequisitos.Arbol(r, 2));
        Console.WriteLine("  Pasos:");
        foreach (Paso p in t.Pasos) Console.WriteLine($"    {p.Numero}. {p.Nombre}  ({p.Responsable})");
    }

    public static void Catalogo(SistemaTramites sis)
    {
        Console.WriteLine("CATÁLOGO DE TRÁMITES - Derechos Reales, La Paz");
        foreach (TipoTramite t in sis.Catalogo)
        {
            Console.WriteLine();
            DetalleTramite(t);
        }
    }

    public static void Cola(SistemaTramites sis)
    {
        Console.WriteLine($"COLA DE TURNOS ({sis.ColaTurnos.Cantidad} en espera, orden de llegada):");
        if (sis.ColaTurnos.EstaVacia) Console.WriteLine("  (vacía)");
        int pos = 1;
        foreach (Turno t in sis.ColaTurnos.Elementos())
        {
            Console.WriteLine($"  {pos++}. {t.Codigo}  solicitud #{t.SolicitudId} ({t.TramiteCodigo})  llegó {t.Llegada:HH:mm:ss}");
        }
        Console.WriteLine();
        Console.WriteLine("ROTACIÓN DE VENTANILLAS (lista circular, la próxima aparece primero):");
        foreach (Ventanilla v in sis.Ventanillas.UnaVuelta())
        {
            Console.WriteLine($"  {v.Nombre}  - atendidos: {v.Atendidos}");
        }
    }

    public static void Atencion(Turno turno, Solicitud sol, Ventanilla ven)
    {
        Console.WriteLine($"Turno {turno.Codigo} -> {ven.Nombre}: {sol.Ciudadano.Nombre} ({sol.Tipo.Codigo}, solicitud #{sol.Id})");
    }

    public static void Historial(Solicitud sol)
    {
        Console.WriteLine($"HISTORIAL DE ESTADOS - solicitud #{sol.Id} (pila: el tope es lo más reciente)");
        int n = sol.Historial.Cantidad;
        foreach (EntradaHistorial e in sol.Historial.DesdeElTope())
        {
            string marca = n == sol.Historial.Cantidad ? "TOPE >" : "      ";
            Console.WriteLine($"  {marca} {e.Fecha:HH:mm:ss}  {e.Estado,-10} paso {e.PosPaso}  {e.Detalle}");
            n--;
        }
    }

    private static string Corto(Grafo g, int nodo)
    {
        return nodo == g.NumNodos - 1 ? "FIN" : "P" + (nodo + 1);
    }

    public static void GrafoTramite(TipoTramite t)
    {
        Grafo g = ConstructorGrafo.Crear(t);
        Console.WriteLine($"GRAFO DE FLUJO - [{t.Codigo}] {t.Nombre}");
        Console.WriteLine("Lista de adyacencia (nodo -> transiciones):");
        for (int v = 0; v < g.NumNodos; v++)
        {
            Console.WriteLine($"  {g.Nombre(v)}");
            foreach (Arista a in g.Vecinos(v))
            {
                Console.WriteLine($"      --[{a.Etiqueta}]--> {g.Nombre(a.Destino)}");
            }
        }

        int[] ruta = g.RutaMasCorta(0, g.NumNodos - 1);
        Console.WriteLine();
        Console.WriteLine("Ruta más corta P1 -> FIN (BFS con Cola + Pila): " +
                          string.Join(" -> ", ruta.Select(i => Corto(g, i))));

        int[] dfs = g.RecorridoDFS(0);
        Console.WriteLine("Recorrido en profundidad (DFS con Pila):        " +
                          string.Join(" -> ", dfs.Select(i => Corto(g, i))));

        Console.WriteLine("¿Hay ciclos (reprocesos por observaciones)?:    " + (g.TieneCiclos() ? "Sí" : "No"));
        Console.WriteLine("¿Se puede llegar de P1 a FIN?:                  " + (g.ExisteCamino(0, g.NumNodos - 1) ? "Sí" : "No"));
    }
}
