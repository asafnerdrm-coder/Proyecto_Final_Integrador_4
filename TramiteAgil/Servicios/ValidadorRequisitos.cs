using System.Text;
using TramiteAgil.Modelos;

namespace TramiteAgil.Servicios;

/// <summary>Validación recursiva de requisitos (CV1: recursividad).</summary>
public static class ValidadorRequisitos
{
    /// <summary>
    /// Un requisito hoja se cumple si el ciudadano presentó el documento.
    /// Un requisito compuesto se cumple si TODOS sus sub-requisitos se cumplen.
    /// Los documentos que faltan se acumulan en "faltantes".
    /// </summary>
    public static bool Validar(Requisito req, List<string> presentados, List<string> faltantes)
    {
        if (req.EsHoja) // caso base
        {
            bool cumple = presentados.Contains(req.Nombre);
            if (!cumple) faltantes.Add(req.Nombre);
            return cumple;
        }

        bool todos = true;
        foreach (Requisito sub in req.Sub) // caso recursivo
        {
            if (!Validar(sub, presentados, faltantes)) todos = false;
        }
        return todos;
    }

    /// <summary>Recorre el arreglo de requisitos de forma recursiva (por índice).</summary>
    public static bool ValidarLista(Requisito[] reqs, int indice, List<string> presentados, List<string> faltantes)
    {
        if (indice >= reqs.Length) return true; // caso base
        bool actual = Validar(reqs[indice], presentados, faltantes);
        bool resto = ValidarLista(reqs, indice + 1, presentados, faltantes);
        return actual && resto;
    }

    /// <summary>Junta (recursivamente) los documentos concretos que se deben presentar.</summary>
    public static void ListarHojas(Requisito req, List<string> destino)
    {
        if (req.EsHoja)
        {
            destino.Add(req.Nombre);
            return;
        }
        foreach (Requisito sub in req.Sub) ListarHojas(sub, destino);
    }

    /// <summary>Dibuja el árbol de requisitos con sangría (recursivo).</summary>
    public static string Arbol(Requisito req, int nivel)
    {
        var sb = new StringBuilder();
        sb.Append(new string(' ', nivel * 2)).Append(req.EsHoja ? "- " : "+ ").AppendLine(req.Nombre);
        foreach (Requisito sub in req.Sub) sb.Append(Arbol(sub, nivel + 1));
        return sb.ToString();
    }
}
