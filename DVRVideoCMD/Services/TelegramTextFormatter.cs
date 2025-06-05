using System.Collections.Generic;
using System.Text;

public static class TelegramTextFormatter
{
    public static string FormatLastValuesAsTable(List<ItemLastValue> items)
    {
        if (items == null || items.Count == 0)
            return "No data found.";

        var sb = new StringBuilder();
        sb.AppendLine("```\nName                       | Value   | Prev   | Time");
        sb.AppendLine("---------------------------|---------|--------|-------------------");
        foreach (var item in items)
        {
            var name = item.Name.Length > 25 ? item.Name.Substring(0, 23) + ".." : item.Name;
            sb.AppendLine($"{name.PadRight(27)}| {item.LastValue.PadLeft(7)} | {item.PrevValue.PadLeft(6)} | {item.Time}");
        }
        sb.AppendLine("```");
        return sb.ToString();
    }

    public static string FormatLastValuesAsHtml(List<ItemLastValue> items)
    {
        if (items == null || items.Count == 0)
            return "No data found.";

        var sb = new StringBuilder();
        sb.Append("<pre>"); // открываем pre для всей таблицы
        sb.Append("Name                            Value   Prev    Time\n");
        sb.Append("----------------------------------------------------------\n");
        foreach (var item in items)
        {
            // Имя не должно быть слишком длинным (иначе таблица "поедет")
            var name = item.Name.Length > 30 ? item.Name.Substring(0, 28) + ".." : item.Name;
            sb.Append($"{name.PadRight(32)}{item.LastValue.PadRight(8)}{item.PrevValue.PadRight(8)}{item.Time}\n");
        }
        sb.Append("</pre>");
        return sb.ToString();
    }
    public static string FormatLastValuesAsBlocks(List<ItemLastValue> items)
    {

        if (items == null || items.Count == 0)
            return "No data found.";

        var sb = new StringBuilder();

        foreach (var item in items)
        {
            sb.AppendLine($"{System.Net.WebUtility.HtmlEncode(item.Name)}");
            sb.AppendLine($"Value: {item.LastValue}");
            sb.AppendLine($"Prev:  {item.PrevValue}");
            sb.AppendLine($"Time:  {item.Time}");
            sb.AppendLine("------------------------------");
        }
        return sb.ToString();
    }


}
