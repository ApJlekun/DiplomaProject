using BarInventoryApp.Models;
using ClosedXML.Excel;
using Microsoft.Win32;
using System.Windows;
using BarInventoryApp.Constants;
using System.Collections;

namespace BarInventoryApp.Services;

/// <summary>
/// Сервис для экспорта данных в Excel.
/// </summary>
public class ExcelExportService
{
    #region Константы

    private const string ExcelFileFilter = "Excel files (*.xlsx)|*.xlsx";
    private const string WorksheetName = "Данные";
    private const string SuccessTitle = "Успех";

    #endregion

    #region Методы

    /// <summary>
    /// Универсальный метод экспорта коллекции объектов в Excel.
    /// </summary>
    /// <param name="data">Коллекция объектов (анонимные типы или модели).</param>
    /// <param name="filePath">Путь к файлу.</param>
    public void ExportToExcel(IEnumerable data, string filePath)
    {
        var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(WorksheetName);

        var list = data.Cast<object>().ToList();
        if (list.Count == 0) return;

        // Заголовки на основе свойств первого объекта
        var properties = list[0].GetType().GetProperties();
        for (int i = 0; i < properties.Length; i++)
        {
            worksheet.Cell(1, i + 1).Value = properties[i].Name;
        }

        // Данные
        for (int rowIdx = 0; rowIdx < list.Count; rowIdx++)
        {
            for (int colIdx = 0; colIdx < properties.Length; colIdx++)
            {
                var val = properties[colIdx].GetValue(list[rowIdx]);
                worksheet.Cell(rowIdx + 2, colIdx + 1).Value = val?.ToString() ?? string.Empty;
            }
        }

        worksheet.Columns().AdjustToContents();
        workbook.SaveAs(filePath);
    }

    #endregion
}
