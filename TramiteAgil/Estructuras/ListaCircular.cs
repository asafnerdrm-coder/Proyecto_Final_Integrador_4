namespace TramiteAgil.Estructuras;

/// <summary>
/// Lista circular simplemente enlazada: el último nodo apunta al primero.
/// Se usa para rotar las ventanillas de atención de forma equitativa.
/// </summary>
public class ListaCircular<T>
{
    private Nodo<T> _ultimo;  // _ultimo.Siguiente es el primero
    private Nodo<T> _actual;  // próximo elemento a entregar

    public int Cantidad { get; private set; }

    public void Agregar(T dato)
    {
        var nuevo = new Nodo<T>(dato);
        if (_ultimo == null)
        {
            nuevo.Siguiente = nuevo; // se apunta a sí mismo
            _ultimo = nuevo;
            _actual = nuevo;
        }
        else
        {
            nuevo.Siguiente = _ultimo.Siguiente;
            _ultimo.Siguiente = nuevo;
            _ultimo = nuevo;
        }
        Cantidad++;
    }

    /// <summary>Devuelve el elemento actual y avanza al siguiente (al terminar vuelve al primero).</summary>
    public T Rotar()
    {
        if (_actual == null) throw new InvalidOperationException("La lista circular está vacía.");
        T dato = _actual.Dato;
        _actual = _actual.Siguiente;
        return dato;
    }

    /// <summary>Recorre una vuelta completa empezando por el elemento actual.</summary>
    public IEnumerable<T> UnaVuelta()
    {
        if (_actual == null) yield break;
        Nodo<T> n = _actual;
        for (int i = 0; i < Cantidad; i++)
        {
            yield return n.Dato;
            n = n.Siguiente;
        }
    }
}
