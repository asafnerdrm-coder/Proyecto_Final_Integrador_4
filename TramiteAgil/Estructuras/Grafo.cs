namespace TramiteAgil.Estructuras;

/// <summary>Arista de la lista de adyacencia (lista enlazada de destinos).</summary>
public class Arista
{
    public int Destino;
    public string Etiqueta;
    public Arista Siguiente;

    public Arista(int destino, string etiqueta)
    {
        Destino = destino;
        Etiqueta = etiqueta;
    }
}

/// <summary>
/// Grafo dirigido con lista de adyacencia. Los nodos son los pasos de un trámite y
/// las aristas son las transiciones (continuar / observado: subsanar).
/// </summary>
public class Grafo
{
    private readonly string[] _nombres;
    private readonly Arista[] _adyacencia;

    public int NumNodos => _nombres.Length;

    public Grafo(string[] nombres)
    {
        _nombres = nombres;
        _adyacencia = new Arista[nombres.Length];
    }

    public string Nombre(int nodo) => _nombres[nodo];

    public void AgregarArista(int origen, int destino, string etiqueta)
    {
        var nueva = new Arista(destino, etiqueta);
        if (_adyacencia[origen] == null)
        {
            _adyacencia[origen] = nueva;
            return;
        }
        Arista a = _adyacencia[origen];
        while (a.Siguiente != null) a = a.Siguiente;
        a.Siguiente = nueva;
    }

    public IEnumerable<Arista> Vecinos(int nodo)
    {
        Arista a = _adyacencia[nodo];
        while (a != null)
        {
            yield return a;
            a = a.Siguiente;
        }
    }

    /// <summary>
    /// Ruta con menos transiciones (BFS). Usa una Cola para explorar y una Pila para
    /// reconstruir el camino en orden. Devuelve un arreglo vacío si no hay camino.
    /// </summary>
    public int[] RutaMasCorta(int origen, int destino)
    {
        var padre = new int[NumNodos];
        for (int i = 0; i < padre.Length; i++) padre[i] = -2; // -2 = no visitado
        padre[origen] = -1;                                   // -1 = raíz

        var cola = new Cola<int>();
        cola.Encolar(origen);
        while (!cola.EstaVacia)
        {
            int v = cola.Desencolar();
            if (v == destino) break;
            foreach (Arista a in Vecinos(v))
            {
                if (padre[a.Destino] == -2)
                {
                    padre[a.Destino] = v;
                    cola.Encolar(a.Destino);
                }
            }
        }

        if (padre[destino] == -2) return new int[0];

        var pila = new Pila<int>();
        for (int x = destino; x != -1; x = padre[x]) pila.Push(x);
        var ruta = new int[pila.Cantidad];
        int k = 0;
        while (!pila.EstaVacia) ruta[k++] = pila.Pop();
        return ruta;
    }

    public bool ExisteCamino(int origen, int destino) => RutaMasCorta(origen, destino).Length > 0;

    /// <summary>Recorrido en profundidad (DFS iterativo con Pila). Devuelve el orden de visita.</summary>
    public int[] RecorridoDFS(int inicio)
    {
        var visitado = new bool[NumNodos];
        var orden = new List<int>();
        var pila = new Pila<int>();
        pila.Push(inicio);
        while (!pila.EstaVacia)
        {
            int v = pila.Pop();
            if (visitado[v]) continue;
            visitado[v] = true;
            orden.Add(v);
            var vecinos = new List<Arista>(Vecinos(v));
            for (int i = vecinos.Count - 1; i >= 0; i--)
            {
                if (!visitado[vecinos[i].Destino]) pila.Push(vecinos[i].Destino);
            }
        }
        return orden.ToArray();
    }

    /// <summary>Detecta ciclos (reprocesos por observaciones) con DFS recursivo.</summary>
    public bool TieneCiclos()
    {
        var color = new int[NumNodos]; // 0 = sin visitar, 1 = en curso, 2 = terminado
        for (int v = 0; v < NumNodos; v++)
        {
            if (color[v] == 0 && VisitarCiclo(v, color)) return true;
        }
        return false;
    }

    private bool VisitarCiclo(int v, int[] color)
    {
        color[v] = 1;
        foreach (Arista a in Vecinos(v))
        {
            if (color[a.Destino] == 1) return true;
            if (color[a.Destino] == 0 && VisitarCiclo(a.Destino, color)) return true;
        }
        color[v] = 2;
        return false;
    }
}
