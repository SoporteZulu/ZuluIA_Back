using FluentAssertions;
using Xunit;
using ZuluIA_Back.Application.Features.Reportes.DTOs;
using ZuluIA_Back.Application.Features.Reportes.Enums;
using ZuluIA_Back.Application.Features.Reportes.Services;

namespace ZuluIA_Back.UnitTests.Application;

public class ReporteExportacionServiceTests
{
    private readonly ReporteExportacionService _service = new();

    [Fact]
    public void Exportar_Csv_DebeGenerarArchivoCsv()
    {
        var reporte = new ReporteTabularDto
        {
            Titulo = "Reporte",
            Columnas = ["A", "B"],
            Filas = [["1", "2"]]
        };

        var result = _service.Exportar(reporte, FormatoExportacionReporte.Csv, "reporte");
        result.NombreArchivo.Should().Be("reporte.csv");
        result.ContentType.Should().Be("text/csv");
        result.Contenido.Should().NotBeEmpty();
    }

    [Fact]
    public void Exportar_Pdf_DebeGenerarCabeceraPdf()
    {
        var reporte = new ReporteTabularDto
        {
            Titulo = "Reporte",
            Columnas = ["A"],
            Filas = [["1"]]
        };

        var result = _service.Exportar(reporte, FormatoExportacionReporte.Pdf, "reporte");
        System.Text.Encoding.ASCII.GetString(result.Contenido).Should().StartWith("%PDF-");
        result.LayoutProfile.Should().Be("DEFAULT");
    }

    [Fact]
    public void Exportar_PdfConPerfil_DebeRegistrarLayoutProfile()
    {
        var reporte = new ReporteTabularDto
        {
            Titulo = "Reporte",
            Columnas = ["A"],
            Filas = [["1"]]
        };

        var result = _service.Exportar(reporte, FormatoExportacionReporte.Pdf, "reporte", "legacy_text");
        result.LayoutProfile.Should().Be("LEGACY_TEXT");
        System.Text.Encoding.ASCII.GetString(result.Contenido).Should().Contain("LayoutProfile: LEGACY_TEXT");
    }
}
