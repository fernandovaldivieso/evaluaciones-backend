namespace EvalSystem.Application.DTOs.Evaluaciones;

public record PreguntaDto(Guid Id, string Texto, int Tipo, string TipoNombre, int Puntaje,
    int TiempoSegundos, int Orden, string? Explicacion, Guid SeccionId, List<OpcionDto>? Opciones, DateTime CreatedAt);

public record CreatePreguntaDto(string Texto, int Tipo, int Puntaje, int TiempoSegundos, int Orden,
    string? Explicacion, List<CreateOpcionDto>? Opciones);

public record UpdatePreguntaDto(string? Texto, int? Tipo, int? Puntaje, int? TiempoSegundos,
    int? Orden, string? Explicacion);

public record CreateOpcionDto(string Texto, bool EsCorrecta, int Orden);
