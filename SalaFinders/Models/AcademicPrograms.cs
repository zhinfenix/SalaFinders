namespace SalaFinders.Models;

public static class AcademicPrograms
{
    public const string OpenToAll = "Todos";

    public static readonly string[] Selectable =
    [
        "Ingeniería",
        "Administración",
        "Deportes",
        "Educación Física",
        "Diseño"
    ];

    public static bool IsValidSelectable(string? program) =>
        !string.IsNullOrWhiteSpace(program) &&
        Selectable.Any(p => string.Equals(p, program.Trim(), StringComparison.OrdinalIgnoreCase));

    public static bool CanAccessSpace(IEnumerable<string> allowedPrograms, string? userProgram)
    {
        var list = allowedPrograms?.ToList() ?? [];
        if (list.Count == 0) return true;
        if (list.Any(p => string.Equals(p.Trim(), OpenToAll, StringComparison.OrdinalIgnoreCase)))
            return true;
        if (string.IsNullOrWhiteSpace(userProgram)) return false;
        return list.Any(p => string.Equals(p.Trim(), userProgram.Trim(), StringComparison.OrdinalIgnoreCase));
    }
}
