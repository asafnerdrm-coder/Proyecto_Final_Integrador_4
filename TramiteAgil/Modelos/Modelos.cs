using System.Text;
using TramiteAgil.Estructuras;

namespace TramiteAgil.Modelos;

public enum EstadoSolicitud
{
    Registrada,
    EnEspera,
    EnAtencion,
    EnProceso,
    Observada,
    Finalizada
}

/// <summary>Requisito de un trámite. Puede ser compuesto (tiene sub-requisitos) y se valida recursivamente.</summary>
public class Requisito
{
    public string Nombre { get; }
    public Requisito[] Sub { get; }
    public bool EsHoja => Sub.Length == 0;

    public Requisito(string nombre, params Requisito[] sub)
    {
        Nombre = nombre;
        Sub = sub;
    }
}

/// <summary>Paso del flujo de un trámite.</summary>
public class Paso
{
    public int Numero { get; }
    public string Nombre { get; }
    public string Responsable { get; }
    public bool EsRevision { get; }
    /// <summary>Pasos a los que puede regresar si el trámite es observado (decisión en el grafo).</summary>
    public int[] VuelveA { get; }

    public Paso(int numero, string nombre, string responsable, bool esRevision = false, params int[] vuelveA)
    {
        Numero = numero;
        Nombre = nombre;
        Responsable = responsable;
        EsRevision = esRevision;
        VuelveA = vuelveA;
    }
}

/// <summary>Tipo de trámite del catálogo (Derechos Reales).</summary>
public class TipoTramite
{
    public string Codigo { get; }
    public string Nombre { get; }
    public string Descripcion { get; }
    public string Duracion { get; }
    public string Fuente { get; }
    public Requisito[] Requisitos { get; }
    public Paso[] Pasos { get; }

    public TipoTramite(string codigo, string nombre, string descripcion, string duracion, string fuente,
                       Requisito[] requisitos, Paso[] pasos)
    {
        Codigo = codigo;
        Nombre = nombre;
        Descripcion = descripcion;
        Duracion = duracion;
        Fuente = fuente;
        Requisitos = requisitos;
        Pasos = pasos;
    }
}

public class Ciudadano
{
    public string Nombre { get; }
    public string CI { get; }

    public Ciudadano(string nombre, string ci)
    {
        Nombre = nombre;
        CI = ci;
    }
}

public class Turno
{
    public int Numero { get; }
    public string Codigo => $"T-{Numero:000}";
    public int SolicitudId { get; }
    public string TramiteCodigo { get; }
    public DateTime Llegada { get; }

    public Turno(int numero, int solicitudId, string tramiteCodigo)
    {
        Numero = numero;
        SolicitudId = solicitudId;
        TramiteCodigo = tramiteCodigo;
        Llegada = DateTime.Now;
    }
}

public class Ventanilla
{
    public int Numero { get; }
    public string Nombre => $"Ventanilla {Numero}";
    public int Atendidos { get; set; }

    public Ventanilla(int numero)
    {
        Numero = numero;
    }
}

/// <summary>Una transición guardada en la pila de historial.</summary>
public class EntradaHistorial
{
    public EstadoSolicitud Estado { get; }
    public int PosPaso { get; }
    public string Detalle { get; }
    public DateTime Fecha { get; }

    public EntradaHistorial(EstadoSolicitud estado, int posPaso, string detalle)
    {
        Estado = estado;
        PosPaso = posPaso;
        Detalle = detalle;
        Fecha = DateTime.Now;
    }
}

/// <summary>
/// Solicitud de un ciudadano. Combina dos estructuras:
///  - Flujo: lista doblemente enlazada con los pasos (navegar adelante/atrás).
///  - Historial: pila con las transiciones de estado (último estado arriba).
/// </summary>
public class Solicitud
{
    public int Id { get; }
    public Ciudadano Ciudadano { get; }
    public TipoTramite Tipo { get; }
    public ListaDoble<Paso> Flujo { get; } = new ListaDoble<Paso>();
    public Pila<EntradaHistorial> Historial { get; } = new Pila<EntradaHistorial>();
    public EstadoSolicitud Estado { get; private set; }
    public string Ventanilla { get; set; }
    public List<string> DocumentosPresentados { get; } = new();
    public bool RequisitosPendientes { get; private set; }
    public string MotivoObservacion { get; private set; } = "";

    public void RegistrarDocumentos(IEnumerable<string> documentos)
    {
        foreach (string d in documentos)
            if (!DocumentosPresentados.Contains(d)) DocumentosPresentados.Add(d);
        ActualizarRequisitos();
    }

    public void ActualizarRequisitos()
    {
        var faltantes = new List<string>();
        RequisitosPendientes = !TramiteAgil.Servicios.ValidadorRequisitos.ValidarLista(
            Tipo.Requisitos, 0, DocumentosPresentados, faltantes);
        if (RequisitosPendientes)
        {
            MotivoObservacion = "Documentos pendientes: " + string.Join(", ", faltantes);
            CambiarEstado(EstadoSolicitud.Observada, MotivoObservacion);
        }
        else if (Estado == EstadoSolicitud.Observada)
        {
            MotivoObservacion = "";
            CambiarEstado(Ventanilla == null ? EstadoSolicitud.Registrada : EstadoSolicitud.EnProceso, "Documentación subsanada y validada");
        }
    }

    public Solicitud(int id, Ciudadano ciudadano, TipoTramite tipo)
    {
        Id = id;
        Ciudadano = ciudadano;
        Tipo = tipo;
        foreach (Paso p in tipo.Pasos) Flujo.AgregarAlFinal(p);
        CambiarEstado(EstadoSolicitud.Registrada, "Solicitud registrada y requisitos validados");
    }

    public void CambiarEstado(EstadoSolicitud nuevo, string detalle)
    {
        Estado = nuevo;
        Historial.Push(new EntradaHistorial(nuevo, Flujo.PosicionActual, detalle));
    }

    /// <summary>Completa el paso actual y pasa al siguiente (o finaliza si era el último).</summary>
    public bool Avanzar(out string mensaje)
    {
        if (Estado != EstadoSolicitud.EnProceso && Estado != EstadoSolicitud.Observada)
        {
            mensaje = $"No se puede avanzar: la solicitud está en estado {Estado}.";
            return false;
        }

        if (RequisitosPendientes)
        {
            mensaje = "No se puede avanzar: hay documentos pendientes.";
            return false;
        }
        if (Estado == EstadoSolicitud.Observada)
        {
            mensaje = "No se puede avanzar: primero debe subsanar la observación.";
            return false;
        }
        string completado = Flujo.Actual.Nombre;
        if (Flujo.HaySiguiente)
        {
            Flujo.MoverSiguiente();
            CambiarEstado(EstadoSolicitud.EnProceso,
                $"Completado: {completado}. Ahora en paso {Flujo.PosicionActual}: {Flujo.Actual.Nombre}");
            mensaje = $"Avanzó al paso {Flujo.PosicionActual} de {Flujo.Cantidad}: {Flujo.Actual.Nombre}.";
            return true;
        }

        CambiarEstado(EstadoSolicitud.Finalizada, $"Completado: {completado}. Trámite finalizado");
        mensaje = "Último paso completado. El trámite quedó FINALIZADO.";
        return true;
    }

    /// <summary>El funcionario observa el trámite y lo devuelve al paso anterior.</summary>
    public bool Retroceder(string motivo, out string mensaje)
    {
        if (Estado != EstadoSolicitud.EnProceso)
        {
            mensaje = $"No se puede observar: la solicitud está en estado {Estado}.";
            return false;
        }
        var grafo = TramiteAgil.Servicios.ConstructorGrafo.Crear(Tipo);
        int origen = Flujo.PosicionActual - 1;
        int destino = -1;
        foreach (var arista in grafo.Vecinos(origen))
            if (arista.Etiqueta.StartsWith("observado")) { destino = arista.Destino; break; }
        if (destino < 0)
        {
            mensaje = "Este paso no admite devolución por observación en el grafo.";
            return false;
        }
        Flujo.IrA(destino + 1);
        MotivoObservacion = motivo;
        CambiarEstado(EstadoSolicitud.Observada,
            $"Observado ({motivo}). Vuelve al paso {Flujo.PosicionActual}: {Flujo.Actual.Nombre}");
        mensaje = $"Trámite observado. Regresó al paso {Flujo.PosicionActual}: {Flujo.Actual.Nombre}.";
        return true;
    }

    /// <summary>Deshace la última transición (Pop) y restaura el estado y el paso anteriores.</summary>
    public bool Deshacer(out string mensaje)
    {
        if (Historial.Cantidad < 2)
        {
            mensaje = "No hay transiciones que deshacer.";
            return false;
        }

        EntradaHistorial debajo = Historial.PeekSegundo();
        if (debajo.Estado != EstadoSolicitud.EnProceso && debajo.Estado != EstadoSolicitud.Observada)
        {
            mensaje = "No se puede deshacer más: el turno y la atención ya ocurrieron.";
            return false;
        }

        EntradaHistorial quitada = Historial.Pop();
        EntradaHistorial previa = Historial.Peek();
        Estado = previa.Estado;
        Flujo.IrA(previa.PosPaso);
        mensaje = $"Se deshizo «{quitada.Estado}». Vuelve a {Estado} en el paso {Flujo.PosicionActual}.";
        return true;
    }

    /// <summary>Texto de seguimiento: paso actual, completados y pendientes.</summary>
    public string Seguimiento()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Solicitud #{Id} | {Tipo.Codigo} - {Tipo.Nombre}");
        sb.AppendLine($"Ciudadano: {Ciudadano.Nombre} (CI {Ciudadano.CI})");
        sb.AppendLine($"Estado: {Estado}" + (Ventanilla != null ? $" | Atendida en: {Ventanilla}" : ""));

        if (RequisitosPendientes) sb.AppendLine(MotivoObservacion);
        int completados = 0;
        foreach (Paso p in Flujo.Elementos())
        {
            string marca;
            if (Estado == EstadoSolicitud.Finalizada || p.Numero < Flujo.PosicionActual)
            {
                marca = "[X]";
                completados++;
            }
            else if (p.Numero == Flujo.PosicionActual)
            {
                marca = "[>]";
            }
            else
            {
                marca = "[ ]";
            }
            sb.AppendLine($"  {marca} Paso {p.Numero}: {p.Nombre}  ({p.Responsable})");
        }
        sb.Append($"Completados: {completados}/{Flujo.Cantidad} | Faltan: {Flujo.Cantidad - completados}");
        return sb.ToString();
    }
}
