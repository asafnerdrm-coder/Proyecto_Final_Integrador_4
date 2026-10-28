namespace TramiteAgil.Estructuras;

/// <summary>Pila (LIFO) con nodos enlazados. Se usa para el historial de estados del trámite.</summary>
public class Pila<T>
{
    private Nodo<T> _tope;

    public int Cantidad { get; private set; }
    public bool EstaVacia => _tope == null;

    public void Push(T dato)
    {
        var nuevo = new Nodo<T>(dato);
        nuevo.Siguiente = _tope;
        _tope = nuevo;
        Cantidad++;
    }

    public T Pop()
    {
        if (_tope == null) throw new InvalidOperationException("La pila está vacía.");
        T dato = _tope.Dato;
        _tope = _tope.Siguiente;
        Cantidad--;
        return dato;
    }

    public T Peek()
    {
        if (_tope == null) throw new InvalidOperationException("La pila está vacía.");
        return _tope.Dato;
    }

    /// <summary>Elemento inmediatamente debajo del tope (sin sacarlo).</summary>
    public T PeekSegundo()
    {
        if (Cantidad < 2) throw new InvalidOperationException("La pila no tiene un segundo elemento.");
        return _tope.Siguiente.Dato;
    }

    /// <summary>Recorre del tope a la base sin modificar la pila.</summary>
    public IEnumerable<T> DesdeElTope()
    {
        Nodo<T> actual = _tope;
        while (actual != null)
        {
            yield return actual.Dato;
            actual = actual.Siguiente;
        }
    }
}
