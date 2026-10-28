using TramiteAgil.Estructuras;
using TramiteAgil.Modelos;

namespace TramiteAgil.Servicios;

/// <summary>Núcleo del sistema: catálogo, solicitudes, cola de turnos y ventanillas rotativas.</summary>
public class SistemaTramites
{
    private readonly List<Solicitud> _solicitudes = new List<Solicitud>();
    private int _siguienteSolicitud = 1;
    private int _siguienteTurno = 1;

    public TipoTramite[] Catalogo { get; }                       // arreglo
    public Cola<Turno> ColaTurnos { get; } = new Cola<Turno>();   // cola (FIFO)
    public ListaCircular<Ventanilla> Ventanillas { get; } = new ListaCircular<Ventanilla>(); // lista circular
    public IReadOnlyList<Solicitud> Solicitudes => _solicitudes;

    public SistemaTramites()
    {
        Catalogo = CatalogoDerechosReales.Crear();
        for (int i = 1; i <= 3; i++) Ventanillas.Agregar(new Ventanilla(i));
    }

    public Solicitud RegistrarSolicitud(Ciudadano ciudadano, TipoTramite tipo)
    {
        var solicitud = new Solicitud(_siguienteSolicitud++, ciudadano, tipo); // memoria dinámica
        _solicitudes.Add(solicitud);
        return solicitud;
    }

    public Turno GenerarTurno(Solicitud solicitud)
    {
        if (solicitud.RequisitosPendientes) throw new InvalidOperationException("Faltan documentos para emitir turno.");
        var turno = new Turno(_siguienteTurno++, solicitud.Id, solicitud.Tipo.Codigo);
        ColaTurnos.Encolar(turno);
        solicitud.CambiarEstado(EstadoSolicitud.EnEspera, $"Turno {turno.Codigo} generado y encolado");
        return turno;
    }

    /// <summary>
    /// Atiende al primero de la cola: le asigna la siguiente ventanilla (rotación circular),
    /// completa el paso 1 (presentación en ventanilla) y deja el trámite en el paso 2.
    /// </summary>
    public bool AtenderSiguiente(out Turno turno, out Solicitud solicitud, out Ventanilla ventanilla)
    {
        turno = null;
        solicitud = null;
        ventanilla = null;
        if (ColaTurnos.EstaVacia) return false;

        turno = ColaTurnos.Desencolar();
        solicitud = Buscar(turno.SolicitudId);
        ventanilla = Ventanillas.Rotar();
        ventanilla.Atendidos++;
        solicitud.Ventanilla = ventanilla.Nombre;

        solicitud.CambiarEstado(EstadoSolicitud.EnAtencion,
            $"Atendido en {ventanilla.Nombre} (turno {turno.Codigo})");
        if (solicitud.Flujo.MoverSiguiente())
        {
            solicitud.CambiarEstado(EstadoSolicitud.EnProceso,
                $"Paso 1 completado en ventanilla. Ahora en paso {solicitud.Flujo.PosicionActual}: {solicitud.Flujo.Actual.Nombre}");
        }
        return true;
    }

    public Solicitud Buscar(int id)
    {
        foreach (Solicitud s in _solicitudes)
        {
            if (s.Id == id) return s;
        }
        return null;
    }
}
