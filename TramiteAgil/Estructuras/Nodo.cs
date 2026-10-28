namespace TramiteAgil.Estructuras;

/// <summary>
/// Nodo genérico creado dinámicamente con "new" (memoria dinámica en el heap).
/// Lo reutilizan Pila, Cola, ListaDoble y ListaCircular.
/// </summary>
public class Nodo<T>
{
    public T Dato;
    public Nodo<T> Siguiente;
    public Nodo<T> Anterior; // solo lo usa la lista doblemente enlazada

    public Nodo(T dato)
    {
        Dato = dato;
    }
}
