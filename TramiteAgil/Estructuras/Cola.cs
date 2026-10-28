namespace TramiteAgil.Estructuras;

/// <summary>Cola (FIFO) con nodos enlazados. Se usa para la fila de turnos de atención.</summary>
public class Cola<T>
{
    private Nodo<T> _frente;
    private Nodo<T> _final;

    public int Cantidad { get; private set; }
    public bool EstaVacia => _frente == null;

    public void Encolar(T dato)
    {
        var nuevo = new Nodo<T>(dato);
        if (_final == null)
        {
            _frente = nuevo;
            _final = nuevo;
        }
        else
        {
            _final.Siguiente = nuevo;
            _final = nuevo;
        }
        Cantidad++;
    }

    public T Desencolar()
    {
        if (_frente == null) throw new InvalidOperationException("La cola está vacía.");
        T dato = _frente.Dato;
        _frente = _frente.Siguiente;
        if (_frente == null) _final = null;
        Cantidad--;
        return dato;
    }

    public T Frente()
    {
        if (_frente == null) throw new InvalidOperationException("La cola está vacía.");
        return _frente.Dato;
    }

    /// <summary>Recorre del frente al final sin modificar la cola.</summary>
    public IEnumerable<T> Elementos()
    {
        Nodo<T> actual = _frente;
        while (actual != null)
        {
            yield return actual.Dato;
            actual = actual.Siguiente;
        }
    }
}
