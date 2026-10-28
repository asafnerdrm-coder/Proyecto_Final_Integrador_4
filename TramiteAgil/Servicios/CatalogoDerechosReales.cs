using TramiteAgil.Modelos;

namespace TramiteAgil.Servicios;

/// <summary>
/// Catálogo (arreglo) con 3 trámites de Derechos Reales de duraciones distintas: 3, 4 y 6 pasos.
/// Es un modelo académico simplificado a partir de la normativa y guías públicas; los plazos
/// "referenciales" son simulados y deben confirmarse en la oficina de Derechos Reales.
/// </summary>
public static class CatalogoDerechosReales
{
    public static TipoTramite[] Crear()
    {
        // 1) Trámite corto: 3 pasos
        var certificado = new TipoTramite(
            "CNP",
            "Certificado de No Propiedad Urbana",
            "Certificación emitida por Derechos Reales que acredita si una persona tiene o no inmuebles urbanos registrados.",
            "1 día hábil (referencial, simulado)",
            "Ley N° 803 y DS N° 4377: exigen la certificación de no propiedad emitida por Derechos Reales.",
            new Requisito[]
            {
                new Requisito("Cédula de identidad vigente"),
                new Requisito("Formulario de solicitud de certificación"),
                new Requisito("Comprobante de pago del arancel registral")
            },
            new Paso[]
            {
                new Paso(1, "Presentar solicitud y documentos en ventanilla", "Ventanilla de atención"),
                new Paso(2, "Búsqueda y verificación en el sistema registral", "Técnico registrador", true, 1),
                new Paso(3, "Emisión y entrega del certificado", "Ventanilla de entrega")
            });

        // 2) Trámite medio: 4 pasos
        var correccion = new TipoTramite(
            "COR",
            "Corrección de Datos en el Folio Real",
            "Subinscripción de correcciones de datos de identidad o técnicos del inmueble en el Folio Real.",
            "3 días hábiles (plazo máximo del DS 4273)",
            "DS N° 4273 (2020): requisitos de corrección y Folio Real corregido en máx. 3 días hábiles.",
            new Requisito[]
            {
                new Requisito("Folio Real actualizado del inmueble"),
                new Requisito("Cédula de identidad vigente del propietario"),
                new Requisito("Escritura Pública Unilateral de aclaración",
                    new Requisito("Minuta de aclaración firmada"),
                    new Requisito("Protocolización ante Notario de Fe Pública"))
            },
            new Paso[]
            {
                new Paso(1, "Presentar requisitos en Derechos Reales", "Ventanilla de atención"),
                new Paso(2, "Revisión documental", "Técnico registrador", true, 1),
                new Paso(3, "Subinscripción de la Escritura Pública Unilateral", "Registrador"),
                new Paso(4, "Emisión del Folio Real corregido", "Ventanilla de entrega")
            });

        // 3) Trámite largo: 6 pasos
        var transferencia = new TipoTramite(
            "TRA",
            "Transferencia de Inmueble (compraventa) e Inscripción de Folio Real",
            "Registro del nuevo propietario de un inmueble tras una compraventa.",
            "≈ 20 días calendario (referencial)",
            "Procedimiento general: pago del impuesto, protocolización de la minuta e inscripción en Derechos Reales; Folio Real en ≈ 20 días (guía de la Embajada de España en La Paz).",
            new Requisito[]
            {
                new Requisito("Folio Real actualizado del inmueble"),
                new Requisito("Testimonio de propiedad del vendedor"),
                new Requisito("Plano de catastro del inmueble"),
                new Requisito("Comprobantes de impuestos al día"),
                new Requisito("Comprobante de pago del Impuesto a la Transferencia"),
                new Requisito("Cédulas de identidad de las partes",
                    new Requisito("Cédula del vendedor"),
                    new Requisito("Cédula del comprador")),
                new Requisito("Minuta de compraventa",
                    new Requisito("Minuta firmada por las partes"),
                    new Requisito("Protocolización ante Notario"))
            },
            new Paso[]
            {
                new Paso(1, "Verificar la titularidad del vendedor", "Ventanilla de atención"),
                new Paso(2, "Pago del Impuesto Municipal a la Transferencia", "Gestión externa: Gobierno Municipal"),
                new Paso(3, "Protocolización de la minuta ante Notario", "Gestión externa: Notaría de Fe Pública"),
                new Paso(4, "Presentar el testimonio en Derechos Reales", "Ventanilla de atención"),
                new Paso(5, "Calificación registral (revisión legal)", "Registrador", true, 4, 3),
                new Paso(6, "Inscripción y entrega del Folio Real al comprador", "Ventanilla de entrega")
            });

        return new TipoTramite[] { certificado, correccion, transferencia };
    }
}
