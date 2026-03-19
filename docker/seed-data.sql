-- ============================================================================
-- EvalSystem - Seed Data
-- Run AFTER EF migrations have created the schema:
--   dotnet ef database update --project src/EvalSystem.Infrastructure --startup-project src/EvalSystem.API
-- Then:
--   sqlcmd -S localhost -U sa -P "EvalSystem_Dev2024!" -d EvalSystemDb -i docker/seed-data.sql
-- ============================================================================

USE EvalSystemDb;
GO

-- ── Usuarios ────────────────────────────────────────────────────────────────
-- Passwords are BCrypt hashes (all use "Admin123!" or "Test123!")
-- Rol: 1=Admin, 2=Evaluador, 3=Candidato
INSERT INTO Usuarios (Id, Nombre, Email, PasswordHash, Rol, Activo, CreatedAt, IsDeleted)
VALUES
  ('2C91CCBE-3D8D-44F8-5F38-08DE85D4D4D2', 'Admin User', 'admin@eval.com',
   '$2a$11$dkzz7D5dXn9eEuU8qAp6y.H3nZ798G/Pa4vFqELoMXFj14nCy7U1y', 1, 1, GETUTCDATE(), 0),
  ('C19851EE-9B5B-4ECC-4110-08DE85E0E732', 'Chelin Juji Floyd Cantero', 'chelin@juji.com',
   '$2a$11$xZBez4xYLm09olmuIq3IJOPUvNFR5aebrsC4KhJlugzrRsmbqI98i', 2, 1, GETUTCDATE(), 0),
  ('7EE164B5-1289-40D3-4111-08DE85E0E732', 'Oscar Fernando Valdivieso Aguilar', 'fernandoscar04@gmail.com',
   '$2a$11$LEzr.l2z3T/YrE7rnDtpN.Ci0CulEi4c6waAppDALZnGf1FgGLH5a', 3, 1, GETUTCDATE(), 0),
  ('0C2D4E60-10D1-483A-E897-08DE85FC0853', 'Jorge Moya', 'chelinchelin@chelin.chelin',
   '$2a$11$ZzyAozAeh9sdnqfQFR2xpemiZP2D70tNeVBTnMCBwpUy.9x1REt3a', 3, 1, GETUTCDATE(), 0),
  ('87915631-AF4F-47DE-AD63-08DE8600634A', 'Test Candidato', 'candidato@test.com',
   '$2a$11$2OK85dMd.kHM38dpJwuOxemUF7pW.Tw.di9sfkKGw4UWdWzosz24W', 3, 1, GETUTCDATE(), 0),
  ('BED29FD1-0926-4F1B-CFC5-08DE8601CDB8', 'Mario Alejandro Galeas', 'mariogaleas@gmail.com',
   '$2a$11$X/05vMai0TRZLHf1t5PYBOlYEv8iy1FM3zZ4HBs1h3XOTUl1/HNEq', 3, 1, GETUTCDATE(), 0),
  ('F65BCF55-15E3-45BB-CFC6-08DE8601CDB8', 'Stephany Gisselle Pleites', 'toro@gmail.com',
   '$2a$11$l5p1YKpkDjqgKYeaueBdIehPew8NVidN1vMCppao3jbtweBuHZUou', 3, 1, GETUTCDATE(), 0);
GO

-- ── Tecnologias ─────────────────────────────────────────────────────────────
INSERT INTO Tecnologias (Id, Nombre, Descripcion, Activa, CreatedAt, IsDeleted)
VALUES
  ('6AEDB2D9-C234-4D5E-F169-08DE85D573DC', 'C#', 'Lenguaje .NET', 1, GETUTCDATE(), 0),
  ('6A676A64-341E-4083-F16A-08DE85D573DC', 'Ruby', 'Una tecnologia bonita', 1, GETUTCDATE(), 0),
  ('C290161B-139F-4AFA-6C1F-08DE85F42F87', 'AngularJS', 'Framework frontend', 1, GETUTCDATE(), 0);
GO

-- ── Evaluaciones ────────────────────────────────────────────────────────────
-- Nivel: 1=Junior, 2=Mid, 3=Senior, 4=Lead
INSERT INTO Evaluaciones (Id, Nombre, Descripcion, Nivel, TiempoLimiteMinutos, Activa, TecnologiaId, CreatedAt, IsDeleted)
VALUES
  ('4F878C8B-AA75-4A5D-D4A7-08DE85EAEA0E', N'Evaluaci' + NCHAR(243) + N'n de C#', N'Una evaluacion de C#', 2, 30, 1,
   '6AEDB2D9-C234-4D5E-F169-08DE85D573DC', GETUTCDATE(), 0),
  ('B21D9D4B-AD62-4CB7-D4A8-08DE85EAEA0E', N'Evaluaci' + NCHAR(243) + N'n de C# (copia)', N'Una evaluacion de C#', 2, 30, 1,
   '6AEDB2D9-C234-4D5E-F169-08DE85D573DC', GETUTCDATE(), 0),
  ('EC84A8D1-D197-4231-D4A9-08DE85EAEA0E', N'Evaluaci' + NCHAR(243) + N'n de C# (copia 2)', N'Una evaluacion de C#', 2, 30, 1,
   '6AEDB2D9-C234-4D5E-F169-08DE85D573DC', GETUTCDATE(), 0),
  ('53D6BBB2-F82E-4D65-0A9E-08DE85F45835', N'Evaluacion conceptos generales Ruby', NULL, 2, 50, 1,
   '6A676A64-341E-4083-F16A-08DE85D573DC', GETUTCDATE(), 0);
GO

-- ── Secciones ───────────────────────────────────────────────────────────────
INSERT INTO EvaluacionSecciones (Id, Nombre, Descripcion, Orden, EvaluacionId, CreatedAt, IsDeleted)
VALUES
  ('EA0D2FFB-4396-4EC7-6000-08DE85EB033D', 'Arquitectura', 'Para saber el conocimiento', 1,
   '4F878C8B-AA75-4A5D-D4A7-08DE85EAEA0E', GETUTCDATE(), 0),
  ('3F72925E-8902-4BE0-6001-08DE85EB033D', 'Arquitectura', 'Para saber el conocimiento', 1,
   'B21D9D4B-AD62-4CB7-D4A8-08DE85EAEA0E', GETUTCDATE(), 0),
  ('8FEB441B-0298-4C11-6002-08DE85EB033D', 'Arquitectura', 'Para saber el conocimiento', 1,
   'EC84A8D1-D197-4231-D4A9-08DE85EAEA0E', GETUTCDATE(), 0),
  ('28685411-970B-4FDA-7ECD-08DE85F46AE2', 'Conceptos generales', NULL, 1,
   '53D6BBB2-F82E-4D65-0A9E-08DE85F45835', GETUTCDATE(), 0);
GO

-- ── Preguntas ───────────────────────────────────────────────────────────────
-- Tipo: 1=OpcionMultiple, 2=Abierta, 3=Codigo, 4=VerdaderoFalso

-- Eval C# original
INSERT INTO Preguntas (Id, Texto, Tipo, Puntaje, TiempoSegundos, Orden, Explicacion, SeccionId, CreatedAt, IsDeleted)
VALUES
  ('370CDCA9-24A4-4503-ADC3-08DE85EB1BC9', N'¿Que es C#?', 2, 20, 120, 1, 'Responde bien',
   'EA0D2FFB-4396-4EC7-6000-08DE85EB033D', GETUTCDATE(), 0),
  ('F2DBA08F-B7BF-41D3-ADC4-08DE85EB1BC9', N'¿Por que se usa C#?', 1, 20, 120, 2, NULL,
   'EA0D2FFB-4396-4EC7-6000-08DE85EB033D', GETUTCDATE(), 0),
  ('8F396494-5DAB-4E92-ADC5-08DE85EB1BC9', N'Escribe una funcion que reciba un string y retorne un array', 3, 20, 120, 3, NULL,
   'EA0D2FFB-4396-4EC7-6000-08DE85EB033D', GETUTCDATE(), 0);
GO

-- Eval C# copia
INSERT INTO Preguntas (Id, Texto, Tipo, Puntaje, TiempoSegundos, Orden, Explicacion, SeccionId, CreatedAt, IsDeleted)
VALUES
  ('50967576-5796-4C52-ADC6-08DE85EB1BC9', N'¿Que es C#?', 2, 20, 120, 1, 'Responde bien',
   '3F72925E-8902-4BE0-6001-08DE85EB033D', GETUTCDATE(), 0),
  ('D9A5FAD7-141F-42AE-ADC7-08DE85EB1BC9', N'¿Por que se usa C#?', 1, 20, 120, 2, NULL,
   '3F72925E-8902-4BE0-6001-08DE85EB033D', GETUTCDATE(), 0),
  ('CF43EB54-E6A1-497A-ADC8-08DE85EB1BC9', N'Escribe una funcion que reciba un string y retorne un array', 3, 20, 120, 3, NULL,
   '3F72925E-8902-4BE0-6001-08DE85EB033D', GETUTCDATE(), 0);
GO

-- Eval C# copia 2
INSERT INTO Preguntas (Id, Texto, Tipo, Puntaje, TiempoSegundos, Orden, Explicacion, SeccionId, CreatedAt, IsDeleted)
VALUES
  ('763BD2FA-81B7-417D-ADC9-08DE85EB1BC9', N'¿Que es C#?', 2, 20, 120, 1, 'Responde bien',
   '8FEB441B-0298-4C11-6002-08DE85EB033D', GETUTCDATE(), 0),
  ('E8A79F7F-6F0E-4AD8-ADCA-08DE85EB1BC9', N'¿Por que se usa C#?', 1, 20, 120, 2, NULL,
   '8FEB441B-0298-4C11-6002-08DE85EB033D', GETUTCDATE(), 0),
  ('14B38903-D8B7-4C0B-ADCB-08DE85EB1BC9', N'Escribe una funcion que reciba un string y retorne un array', 3, 20, 120, 3, NULL,
   '8FEB441B-0298-4C11-6002-08DE85EB033D', GETUTCDATE(), 0);
GO

-- Eval Ruby
INSERT INTO Preguntas (Id, Texto, Tipo, Puntaje, TiempoSegundos, Orden, Explicacion, SeccionId, CreatedAt, IsDeleted)
VALUES
  ('322C25A7-BC1C-4882-C77D-08DE85F6B02F', N'¿Que devuelve [1,2,3].map { |x| x * 2 }?', 1, 10, 30, 1, NULL,
   '28685411-970B-4FDA-7ECD-08DE85F46AE2', GETUTCDATE(), 0),
  ('309578F7-BFAA-4913-C77E-08DE85F6B02F', N'¿Cual es la diferencia principal entre map y each?', 1, 10, 120, 2, NULL,
   '28685411-970B-4FDA-7ECD-08DE85F46AE2', GETUTCDATE(), 0);
GO

-- ── Opciones de Respuesta ───────────────────────────────────────────────────

-- Opciones: "¿Por que se usa C#?" (eval C# original)
INSERT INTO OpcionesRespuesta (Id, Texto, EsCorrecta, Orden, PreguntaId, CreatedAt, IsDeleted)
VALUES
  ('D5115F27-63B0-4A57-07B1-08DE85EB325B', 'Por su ecosistema .NET', 1, 0, 'F2DBA08F-B7BF-41D3-ADC4-08DE85EB1BC9', GETUTCDATE(), 0),
  ('C2D5C1BD-BBDD-4E17-07B2-08DE85EB325B', 'No tiene uso real', 0, 1, 'F2DBA08F-B7BF-41D3-ADC4-08DE85EB1BC9', GETUTCDATE(), 0),
  ('7D4BE1A7-8F14-4476-07B3-08DE85EB325B', 'No se sabe', 0, 2, 'F2DBA08F-B7BF-41D3-ADC4-08DE85EB1BC9', GETUTCDATE(), 0);
GO

-- Opciones: "¿Por que se usa C#?" (eval C# copia)
INSERT INTO OpcionesRespuesta (Id, Texto, EsCorrecta, Orden, PreguntaId, CreatedAt, IsDeleted)
VALUES
  ('F34110E1-812E-4A63-07B4-08DE85EB325B', 'Por su ecosistema .NET', 1, 0, 'D9A5FAD7-141F-42AE-ADC7-08DE85EB1BC9', GETUTCDATE(), 0),
  ('EB4F1009-BBF1-44C7-07B5-08DE85EB325B', 'No tiene uso real', 0, 1, 'D9A5FAD7-141F-42AE-ADC7-08DE85EB1BC9', GETUTCDATE(), 0),
  ('371B41D5-6B96-47C7-07B6-08DE85EB325B', 'No se sabe', 0, 2, 'D9A5FAD7-141F-42AE-ADC7-08DE85EB1BC9', GETUTCDATE(), 0);
GO

-- Opciones: "¿Por que se usa C#?" (eval C# copia 2)
INSERT INTO OpcionesRespuesta (Id, Texto, EsCorrecta, Orden, PreguntaId, CreatedAt, IsDeleted)
VALUES
  ('B03C9C0F-F41F-41D5-07B7-08DE85EB325B', 'Por su ecosistema .NET', 1, 0, 'E8A79F7F-6F0E-4AD8-ADCA-08DE85EB1BC9', GETUTCDATE(), 0),
  ('312C3637-8A95-49C8-07B8-08DE85EB325B', 'No tiene uso real', 0, 1, 'E8A79F7F-6F0E-4AD8-ADCA-08DE85EB1BC9', GETUTCDATE(), 0),
  ('B4B0A662-D1C2-48DF-07B9-08DE85EB325B', 'No se sabe', 0, 2, 'E8A79F7F-6F0E-4AD8-ADCA-08DE85EB1BC9', GETUTCDATE(), 0);
GO

-- Opciones: Ruby pregunta 1
INSERT INTO OpcionesRespuesta (Id, Texto, EsCorrecta, Orden, PreguntaId, CreatedAt, IsDeleted)
VALUES
  ('DD19277F-81B8-49F7-7751-08DE85F6B032', '[1, 2, 3]',  1, 0, '322C25A7-BC1C-4882-C77D-08DE85F6B02F', GETUTCDATE(), 0),
  ('7F2EAB92-4FD6-4B0F-7752-08DE85F6B032', '[2, 4, 6]',  0, 1, '322C25A7-BC1C-4882-C77D-08DE85F6B02F', GETUTCDATE(), 0),
  ('5E9225DB-3118-4C2F-7753-08DE85F6B032', 'nil',         0, 2, '322C25A7-BC1C-4882-C77D-08DE85F6B02F', GETUTCDATE(), 0),
  ('C21FDD58-943C-4E16-7754-08DE85F6B032', 'Error',       0, 3, '322C25A7-BC1C-4882-C77D-08DE85F6B02F', GETUTCDATE(), 0);
GO

-- Opciones: Ruby pregunta 2
INSERT INTO OpcionesRespuesta (Id, Texto, EsCorrecta, Orden, PreguntaId, CreatedAt, IsDeleted)
VALUES
  ('044041C6-B931-47E1-7755-08DE85F6B032', 'each modifica el array original', 0, 0, '309578F7-BFAA-4913-C77E-08DE85F6B02F', GETUTCDATE(), 0),
  ('7FDA3436-4B01-46DE-7756-08DE85F6B032', 'map devuelve un nuevo array transformado', 1, 1, '309578F7-BFAA-4913-C77E-08DE85F6B02F', GETUTCDATE(), 0),
  ('7EC33C86-58E3-411D-7757-08DE85F6B032', 'map no acepta bloques', 0, 2, '309578F7-BFAA-4913-C77E-08DE85F6B02F', GETUTCDATE(), 0),
  ('2545B2E3-089B-475F-7758-08DE85F6B032', 'No hay diferencia', 0, 3, '309578F7-BFAA-4913-C77E-08DE85F6B02F', GETUTCDATE(), 0);
GO

-- ── Proceso de Seleccion ────────────────────────────────────────────────────
INSERT INTO ProcesosSeleccion (Id, Nombre, Descripcion, Puesto, Estado, FechaLimite, CreadorId, CreatedAt, IsDeleted)
VALUES
  ('E6029A84-3F74-4ED1-6A3B-08DE85EADB27', 'Programador Junior',
   N'Proceso de seleccion para programador jr', 'Programador', 1,
   '2026-04-23', '2C91CCBE-3D8D-44F8-5F38-08DE85D4D4D2', GETUTCDATE(), 0);
GO

-- Asignar candidatos al proceso
INSERT INTO ProcesoCandidatos (Id, ProcesoId, CandidatoId, CreatedAt, IsDeleted)
VALUES
  ('D885A621-F53F-4805-6914-08DE85EDB8A7', 'E6029A84-3F74-4ED1-6A3B-08DE85EADB27', '7EE164B5-1289-40D3-4111-08DE85E0E732', GETUTCDATE(), 0),
  ('E2344F01-F572-492A-5246-08DE85FC486D', 'E6029A84-3F74-4ED1-6A3B-08DE85EADB27', '0C2D4E60-10D1-483A-E897-08DE85FC0853', GETUTCDATE(), 0),
  ('31669AE4-6DE8-4A44-3B44-08DE86007328', 'E6029A84-3F74-4ED1-6A3B-08DE85EADB27', '87915631-AF4F-47DE-AD63-08DE8600634A', GETUTCDATE(), 0),
  ('DABC8F82-32E4-41FE-0AEE-08DE860393F0', 'E6029A84-3F74-4ED1-6A3B-08DE85EADB27', 'F65BCF55-15E3-45BB-CFC6-08DE8601CDB8', GETUTCDATE(), 0),
  ('575B2F09-182D-426F-0AEF-08DE860393F0', 'E6029A84-3F74-4ED1-6A3B-08DE85EADB27', 'BED29FD1-0926-4F1B-CFC5-08DE8601CDB8', GETUTCDATE(), 0);
GO

-- Asignar evaluaciones al proceso
INSERT INTO ProcesoEvaluaciones (Id, ProcesoId, EvaluacionId, CreatedAt, IsDeleted)
VALUES
  ('D5805768-2B1A-42D4-7A2F-08DE85EC08E7', 'E6029A84-3F74-4ED1-6A3B-08DE85EADB27', '4F878C8B-AA75-4A5D-D4A7-08DE85EAEA0E', GETUTCDATE(), 0),
  ('7E171E1F-A9F7-46D1-3186-08DE85F70067', 'E6029A84-3F74-4ED1-6A3B-08DE85EADB27', '53D6BBB2-F82E-4D65-0A9E-08DE85F45835', GETUTCDATE(), 0);
GO

PRINT 'Seed data inserted successfully.';
GO


