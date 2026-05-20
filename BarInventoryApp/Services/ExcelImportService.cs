using BarInventoryApp.Models;
using ClosedXML.Excel;
using Microsoft.Win32;
using System.Windows;
using BarInventoryApp.Constants;

namespace BarInventoryApp.Services;

/// <summary>
/// Сервис для импорта данных из Excel.
/// </summary>
public class ExcelImportService
{
    #region Константы

    private const string ExcelFileFilter = "Excel files (*.xlsx)|*.xlsx";
    private const string ImportInstructionsTitle = "Инструкция по импорту";
    private const string ImportInstructions = @"Для корректного импорта данных из Excel файла следуйте этим правилам:

• Файл должен содержать данные начиная с первой строки
• Первая колонка (A): Название ингредиента (текст)
• Вторая колонка (B): Количество (число)
• Третья колонка (C): Единица измерения (текст)
• Четвертая колонка (D): Категория (текст) - НЕОБЯЗАТЕЛЬНО, по умолчанию 'Без категории'

Пример формата файла:
┌─────────────┬─────────────┬─────────────┬─────────────┐
│ Название    │ Количество  │ Ед.изм.     │ Категория   │
├─────────────┼─────────────┼─────────────┼─────────────┤
│ Водка       │ 10.5        │ л           │ Алкоголь    │
│ Лимон       │ 25          │ шт          │ Фрукты      │
│ Сироп       │ 5.2         │ л           │ Добавки     │
└─────────────┴─────────────┴─────────────┴─────────────┘

Все существующие ингредиенты с совпадающими названиями будут обновлены.
Новые ингредиенты будут добавлены в указанные категории (если категория не существует, она будет создана).";

    #endregion

    #region Методы

    /// <summary>
    /// Показывает инструкции по импорту данных.
    /// </summary>
    public void ShowImportInstructions()
    {
        MessageBox.Show(
            ImportInstructions,
            ImportInstructionsTitle,
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    /// <summary>
    /// Импортирует ингредиенты из файла Excel.
    /// </summary>
    /// <returns>Список импортированных ингредиентов или null, если импорт отменен.</returns>
    public List<Ingredient>? ImportIngredients()
    {
        // Диалог выбора файла
        var dialog = new OpenFileDialog
        {
            Filter = ExcelFileFilter,
            Title = "Выберите файл Excel для импорта",
            InitialDirectory = AppDomain.CurrentDomain.BaseDirectory
        };

        if (dialog.ShowDialog() != true)
            return null;

        try
        {
            var ingredients = new List<Ingredient>();
            using (var workbook = new XLWorkbook(dialog.FileName))
            {
                var worksheet = workbook.Worksheets.FirstOrDefault();
                if (worksheet == null)
                {
                    MessageBox.Show("Файл Excel не содержит листов с данными.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }

                var rows = worksheet.RowsUsed();
                if (!rows.Any())
                {
                    MessageBox.Show("Файл Excel не содержит данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }

                int processedRows = 0;
                int skippedRows = 0;
                string debugInfo = $"Файл: {System.IO.Path.GetFileName(dialog.FileName)}\n";

                foreach (var row in rows)
                {
                    processedRows++;
                    debugInfo += $"Строка {row.RowNumber()}: ";

                    try
                    {
                        // Пропускаем заголовок, если он есть
                        if (row.RowNumber() == 1 && IsHeaderRow(row))
                        {
                            debugInfo += "пропущена (заголовок)\n";
                            skippedRows++;
                            continue;
                        }

                        var ingredient = ParseIngredientFromRow(row);
                        if (ingredient != null)
                        {
                            ingredients.Add(ingredient);
                            debugInfo += $"OK - {ingredient.Name}, {ingredient.Quantity}, {ingredient.Unit}\n";
                        }
                        else
                        {
                            debugInfo += "пропущена (ошибка парсинга)\n";
                            skippedRows++;
                        }
                    }
                    catch (Exception ex)
                    {
                        debugInfo += $"ошибка: {ex.Message}\n";
                        skippedRows++;
                    }
                }

                // Отладочная информация
                MessageBox.Show(
                    debugInfo +
                    $"\nВсего обработано строк: {processedRows}\nПропущено строк: {skippedRows}\nУспешно прочитано ингредиентов: {ingredients.Count}",
                    "Информация об импорте",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }

            if (ingredients.Count == 0)
            {
                MessageBox.Show("Не удалось прочитать ни одного корректного ингредиента из файла.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }

            return ingredients;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Ошибка при чтении файла Excel: {ex.Message}\n\nStackTrace: {ex.StackTrace}",
                "Ошибка импорта",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return null;
        }
    }

    /// <summary>
    /// Определяет, является ли строка заголовком.
    /// </summary>
    private bool IsHeaderRow(IXLRow row)
    {
        try
        {
            var cell1 = row.Cell(1).GetValue<string>();
            var cell2 = row.Cell(2).GetValue<string>();
            var cell3 = row.Cell(3).GetValue<string>();

            // Если первая ячейка содержит "название" или похожие слова
            return cell1.Contains("название", StringComparison.OrdinalIgnoreCase) ||
                   cell1.Contains("ингредиент", StringComparison.OrdinalIgnoreCase) ||
                   cell1.Contains("товар", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Парсит ингредиент из строки Excel.
    /// </summary>
    private Ingredient? ParseIngredientFromRow(IXLRow row)
    {
        try
        {
            // Название (колонка A)
            var nameCell = row.Cell(1);
            if (nameCell.IsEmpty())
                return null;

            var name = nameCell.GetValue<string>().Trim();
            if (string.IsNullOrWhiteSpace(name))
                return null;

            // Количество (колонка B)
            var quantityCell = row.Cell(2);
            if (quantityCell.IsEmpty())
                return null;

            decimal quantity;
            try
            {
                // Пробуем различные способы получения числа
                if (quantityCell.DataType == XLDataType.Number)
                {
                    quantity = quantityCell.GetValue<decimal>();
                }
                else
                {
                    var quantityStr = quantityCell.GetValue<string>().Trim();
                    if (string.IsNullOrWhiteSpace(quantityStr))
                        return null;

                    // Заменяем запятую на точку для корректного парсинга
                    quantityStr = quantityStr.Replace(',', '.');
                    quantity = Convert.ToDecimal(quantityStr, System.Globalization.CultureInfo.InvariantCulture);
                }
            }
            catch (Exception ex)
            {
                // Логируем ошибку парсинга количества
                System.Diagnostics.Debug.WriteLine($"Ошибка парсинга количества в строке {row.RowNumber()}: {ex.Message}");
                return null;
            }

            // Единица измерения (колонка C)
            var unitCell = row.Cell(3);
            if (unitCell.IsEmpty())
                return null;

            var unit = unitCell.GetValue<string>().Trim();
            if (string.IsNullOrWhiteSpace(unit))
                return null;

            // Категория (колонка D)
            var categoryCell = row.Cell(4);
            string categoryName = "Без категории";
            if (!categoryCell.IsEmpty())
            {
                var val = categoryCell.GetValue<string>()?.Trim();
                if (!string.IsNullOrWhiteSpace(val))
                {
                    categoryName = val;
                }
            }

            return new Ingredient
            {
                Name = name,
                Quantity = quantity,
                Unit = unit,
                Category = new Category { Name = categoryName }
            };
        }
        catch (Exception ex)
        {
            // Логируем общую ошибку парсинга
            System.Diagnostics.Debug.WriteLine($"Ошибка парсинга строки {row.RowNumber()}: {ex.Message}");
            return null;
        }
    }

    #endregion
}
