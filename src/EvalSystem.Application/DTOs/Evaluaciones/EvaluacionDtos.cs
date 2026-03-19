namespace EvalSystem.Application.DTOs.Evaluaciones;

public record EvaluacionDto(Guid Id, string Nombre, string? Descripcion, int Nivel, string NivelNombre,
    int TiempoLimiteMinutos, bool Activa, Guid TecnologiaId, string TecnologiaNombre, DateTime CreatedAt);

public record EvaluacionDetalleDto(Guid Id, string Nombre, string? Descripcion, int Nivel, string NivelNombre,
    int TiempoLimiteMinutos, bool Activa, Guid TecnologiaId, string TecnologiaNombre,
    List<SeccionDetalleDto> Secciones, DateTime CreatedAt);

public record SeccionDetalleDto(Guid Id, string Nombre, string? Descripcion, int Orden,
    List<PreguntaDetalleDto> Preguntas);

public record PreguntaDetalleDto(Guid Id, string Texto, int Tipo, string TipoNombre,
    int Puntaje, int TiempoSegundos, int Orden, string? Explicacion,
    List<OpcionDto>? Opciones);

public record OpcionDto(Guid Id, string Texto, bool EsCorrecta, int Orden);

public record CreateEvaluacionDto(string Nombre, string? Descripcion, int Nivel,
    int TiempoLimiteMinutos, Guid TecnologiaId);

public record UpdateEvaluacionDto(string? Nombre, string? Descripcion, int? Nivel,
    int? TiempoLimiteMinutos, Guid? TecnologiaId, bool? Activa);
