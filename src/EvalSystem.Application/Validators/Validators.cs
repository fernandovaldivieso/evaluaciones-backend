using EvalSystem.Application.DTOs.Auth;
using EvalSystem.Application.DTOs.Evaluaciones;
using EvalSystem.Application.DTOs.Procesos;
using EvalSystem.Application.DTOs.Sesiones;
using EvalSystem.Application.DTOs.Tecnologias;
using EvalSystem.Application.DTOs.Usuarios;
using FluentValidation;

namespace EvalSystem.Application.Validators;

// ── Auth ──────────────────────────────────────────────────

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6).MaximumLength(128);
    }
}

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6).MaximumLength(128);
        RuleFor(x => x.Rol).InclusiveBetween(1, 3);
    }
}

public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.Token).NotEmpty();
    }
}

// ── Usuarios ──────────────────────────────────────────────

public class CreateUsuarioDtoValidator : AbstractValidator<CreateUsuarioDto>
{
    public CreateUsuarioDtoValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6).MaximumLength(128);
        RuleFor(x => x.Rol).InclusiveBetween(1, 3);
    }
}

public class UpdateUsuarioDtoValidator : AbstractValidator<UpdateUsuarioDto>
{
    public UpdateUsuarioDtoValidator()
    {
        RuleFor(x => x.Nombre).MaximumLength(200).When(x => x.Nombre is not null);
        RuleFor(x => x.Email).EmailAddress().MaximumLength(256).When(x => x.Email is not null);
        RuleFor(x => x.Rol).InclusiveBetween(1, 3).When(x => x.Rol.HasValue);
    }
}

// ── Tecnologías ───────────────────────────────────────────

public class CreateTecnologiaDtoValidator : AbstractValidator<CreateTecnologiaDto>
{
    public CreateTecnologiaDtoValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(150);
    }
}

public class UpdateTecnologiaDtoValidator : AbstractValidator<UpdateTecnologiaDto>
{
    public UpdateTecnologiaDtoValidator()
    {
        RuleFor(x => x.Nombre).MaximumLength(150).When(x => x.Nombre is not null);
    }
}

// ── Evaluaciones ──────────────────────────────────────────

public class CreateEvaluacionDtoValidator : AbstractValidator<CreateEvaluacionDto>
{
    public CreateEvaluacionDtoValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Nivel).InclusiveBetween(1, 4);
        RuleFor(x => x.TiempoLimiteMinutos).GreaterThan(0);
        RuleFor(x => x.TecnologiaId).NotEmpty();
    }
}

public class UpdateEvaluacionDtoValidator : AbstractValidator<UpdateEvaluacionDto>
{
    public UpdateEvaluacionDtoValidator()
    {
        RuleFor(x => x.Nombre).MaximumLength(200).When(x => x.Nombre is not null);
        RuleFor(x => x.Nivel).InclusiveBetween(1, 4).When(x => x.Nivel.HasValue);
        RuleFor(x => x.TiempoLimiteMinutos).GreaterThan(0).When(x => x.TiempoLimiteMinutos.HasValue);
    }
}

// ── Secciones ─────────────────────────────────────────────

public class CreateSeccionDtoValidator : AbstractValidator<CreateSeccionDto>
{
    public CreateSeccionDtoValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Orden).GreaterThanOrEqualTo(0);
    }
}

public class UpdateSeccionDtoValidator : AbstractValidator<UpdateSeccionDto>
{
    public UpdateSeccionDtoValidator()
    {
        RuleFor(x => x.Nombre).MaximumLength(200).When(x => x.Nombre is not null);
        RuleFor(x => x.Orden).GreaterThanOrEqualTo(0).When(x => x.Orden.HasValue);
    }
}

// ── Preguntas ─────────────────────────────────────────────

public class CreatePreguntaDtoValidator : AbstractValidator<CreatePreguntaDto>
{
    public CreatePreguntaDtoValidator()
    {
        RuleFor(x => x.Texto).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.Tipo).InclusiveBetween(1, 4);
        RuleFor(x => x.Puntaje).GreaterThan(0);
        RuleFor(x => x.TiempoSegundos).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Orden).GreaterThanOrEqualTo(0);
        RuleForEach(x => x.Opciones).SetValidator(new CreateOpcionDtoValidator()).When(x => x.Opciones is not null);
    }
}

public class UpdatePreguntaDtoValidator : AbstractValidator<UpdatePreguntaDto>
{
    public UpdatePreguntaDtoValidator()
    {
        RuleFor(x => x.Texto).MaximumLength(2000).When(x => x.Texto is not null);
        RuleFor(x => x.Tipo).InclusiveBetween(1, 4).When(x => x.Tipo.HasValue);
        RuleFor(x => x.Puntaje).GreaterThan(0).When(x => x.Puntaje.HasValue);
    }
}

public class CreateOpcionDtoValidator : AbstractValidator<CreateOpcionDto>
{
    public CreateOpcionDtoValidator()
    {
        RuleFor(x => x.Texto).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.Orden).GreaterThanOrEqualTo(0);
    }
}

// ── Procesos ──────────────────────────────────────────────

public class CreateProcesoDtoValidator : AbstractValidator<CreateProcesoDto>
{
    public CreateProcesoDtoValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(200);
    }
}

public class UpdateProcesoDtoValidator : AbstractValidator<UpdateProcesoDto>
{
    public UpdateProcesoDtoValidator()
    {
        RuleFor(x => x.Nombre).MaximumLength(200).When(x => x.Nombre is not null);
        RuleFor(x => x.Estado).InclusiveBetween(1, 4).When(x => x.Estado.HasValue);
    }
}

public class AsignarCandidatosDtoValidator : AbstractValidator<AsignarCandidatosDto>
{
    public AsignarCandidatosDtoValidator()
    {
        RuleFor(x => x.CandidatoIds).NotEmpty().WithMessage("Debe proporcionar al menos un candidato.");
    }
}

public class AsignarEvaluacionesDtoValidator : AbstractValidator<AsignarEvaluacionesDto>
{
    public AsignarEvaluacionesDtoValidator()
    {
        RuleFor(x => x.EvaluacionIds).NotEmpty().WithMessage("Debe proporcionar al menos una evaluación.");
    }
}

// ── Sesiones ──────────────────────────────────────────────

public class IniciarSesionDtoValidator : AbstractValidator<IniciarSesionDto>
{
    public IniciarSesionDtoValidator()
    {
        RuleFor(x => x.EvaluacionId).NotEmpty();
    }
}

public class ResponderPreguntaDtoValidator : AbstractValidator<ResponderPreguntaDto>
{
    public ResponderPreguntaDtoValidator()
    {
        RuleFor(x => x.PreguntaId).NotEmpty();
        RuleFor(x => x.Respuesta).NotEmpty().MaximumLength(10000);
        RuleFor(x => x.TiempoRespuestaSegundos).GreaterThanOrEqualTo(0);
    }
}
