namespace TramiteAgil.Estructuras;

/// <summary>
/// Lista doblemente enlazada con un cursor ("actual").
/// Modela el flujo de pasos de un trámite: se puede navegar hacia adelante y hacia atrás.
/// </summary>
public class ListaDoble<T>
{
    private Nodo<T> _cabeza;
    private Nodo<T> _cola;
    private Nodo<T> _actual;

    public int Cantidad { get; private set; }

    /// <summary>Posición del cursor (empieza en 1). 0 si la lista está vacía.</summary>
    public int PosicionActual { get; private set; }

    public T Actual => _actual != null ? _actual.Dato : default;
    public bool HayAnterior => _actual != null && _actual.Anterior != null;
    public bool HaySiguiente => _actual != null && _actual.Siguiente != null;

    public void AgregarAlFinal(T dato)
    {
        var nuevo = new Nodo<T>(dato);
        if (_cabeza == null)
        {
            _cabeza = nuevo;
            _cola = nuevo;
            _actual = nuevo;
            PosicionActual = 1;
        }
        else
        {
            _cola.Siguiente = nuevo;
            nuevo.Anterior = _cola;
            _cola = nuevo;
        }
        Cantidad++;
    }

    public bool MoverSiguiente()
    {
        if (!HaySiguiente) return false;
        _actual = _actual.Siguiente;
        PosicionActual++;
        return true;
    }

    public bool MoverAnterior()
    {
        if (!HayAnterior) return false;
        _actual = _actual.Anterior;
        PosicionActual--;
        return true;
    }

    public void IrAlInicio()
    {
        _actual = _cabeza;
        PosicionActual = _cabeza == null ? 0 : 1;
    }

    /// <summary>Lleva el cursor a la posición indicada (1..Cantidad).</summary>
    public bool IrA(int posicion)
    {
        if (posicion < 1 || posicion > Cantidad) return false;
        IrAlInicio();
        while (PosicionActual < posicion) MoverSiguiente();
        return true;
    }

    /// <summary>Recorre de la cabeza a la cola sin mover el cursor.</summary>
    public IEnumerable<T> Elementos()
    {
        Nodo<T> n = _cabeza;
        while (n != null)
        {
            yield return n.Dato;
            n = n.Siguiente;
        }
    }
}
