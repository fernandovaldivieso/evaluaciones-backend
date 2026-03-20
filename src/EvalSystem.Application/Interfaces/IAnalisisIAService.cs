namespace EvalSystem.Application.Interfaces;

public interface IAnalisisIAService
{
    Task<string> GenerarAnalisisAsync(AnalisisContexto contexto);
}

public record SeccionScoreInfo(string Nombre, int Obtenido, int Maximo, decimal Porcentaje);

public record RespuestaAbiertaInfo(string PreguntaTexto, string RespuestaCandidato, int PuntajeObtenido, int PuntajeMaximo);

public record AnalisisContexto(
    string NombreCandidato,
    string NombreEvaluacion,
    string TecnologiaNombre,
    string NivelDificultad,
    decimal ScoreTotal,
    List<SeccionScoreInfo> ScoresPorSeccion,
    List<string> Fortalezas,
    List<string> Brechas,
    List<RespuestaAbiertaInfo> RespuestasAbiertas,
    int TotalPreguntas,
    int PreguntasRespondidas,
    int TiempoTotalSegundos
);
