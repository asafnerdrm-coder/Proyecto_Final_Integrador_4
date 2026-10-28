using TramiteAgil.Estructuras;
using TramiteAgil.Modelos;

namespace TramiteAgil.Servicios;

/// <summary>Convierte los pasos de un trámite en un grafo dirigido (paso → paso, con decisiones).</summary>
public static class ConstructorGrafo
{
    public static Grafo Crear(TipoTramite tipo)
    {
        int n = tipo.Pasos.Length;
        var nombres = new string[n + 1];
        for (int i = 0; i < n; i++) nombres[i] = $"P{tipo.Pasos[i].Numero}: {tipo.Pasos[i].Nombre}";
        nombres[n] = "FINALIZADO";

        var grafo = new Grafo(nombres);
        for (int i = 0; i < n; i++)
        {
            Paso p = tipo.Pasos[i];
            grafo.AgregarArista(i, i + 1, p.EsRevision ? "sin observaciones" : "completado");
            foreach (int destino in p.VuelveA)
            {
                grafo.AgregarArista(i, destino - 1, "observado: subsanar");
            }
        }
        return grafo;
    }
}
