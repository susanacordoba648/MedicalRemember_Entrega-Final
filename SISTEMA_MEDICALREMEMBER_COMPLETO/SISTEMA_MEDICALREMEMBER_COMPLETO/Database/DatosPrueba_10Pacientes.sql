-- =====================================================================
-- DATOS DE PRUEBA - Medical Remember
-- 10 pacientes, 10 tratamientos (1 por paciente)
-- =====================================================================
USE MedicalRememberDB;
SET NOCOUNT ON;

IF EXISTS (SELECT 1 FROM (VALUES (N'Amlodipino'),(N'Aspirina (AAS)'),(N'Atorvastatina'),(N'Enalapril'),(N'Furosemida'),(N'Levotiroxina'),(N'Losartán'),(N'Metformina'),(N'Omeprazol'),(N'Vitamina B12 (Cianocobalamina)')) AS req(Nombre)
           WHERE NOT EXISTS (SELECT 1 FROM Medicamentos m WHERE m.Nombre = req.Nombre))
BEGIN
    RAISERROR('Falta el catalogo de medicamentos. Ejecuta primero Database\ImportarMedicamentos.sql', 16, 1);
    RETURN;
END

-- ---------------------------------------------------------------------
-- 1) PACIENTES (10)   TipoPaciente: 1 = Adulto mayor, 2 = Paciente cronico
-- ---------------------------------------------------------------------
INSERT INTO Pacientes (Nombre, Apellido, FechaNacimiento, Telefono, Correo, Direccion, TipoPaciente)
SELECT v.Nombre, v.Apellido, v.FechaNacimiento, v.Telefono, v.Correo, v.Direccion, v.Tipo
FROM (VALUES
 (N'Maria', N'Garcia Lopez', CAST('1955-03-15' AS DATE), N'8888-1234', N'maria@email.com', N'Granada, Nicaragua', 1),
 (N'Jose', N'Ramirez Mena', CAST('1948-07-22' AS DATE), N'8841-2210', N'jose.ramirez@correo.com', N'Granada, Nicaragua', 1),
 (N'Rosa', N'Martinez Obando', CAST('1952-11-03' AS DATE), N'8725-4410', N'rosa.martinez@correo.com', N'Granada, Nicaragua', 1),
 (N'Carlos', N'Lopez Ruiz', CAST('1960-01-19' AS DATE), N'8890-7731', N'carlos.lopez@correo.com', N'Granada, Nicaragua', 2),
 (N'Ana', N'Gutierrez Pineda', CAST('1957-05-30' AS DATE), N'8612-9054', N'ana.gutierrez@correo.com', N'Granada, Nicaragua', 2),
 (N'Luis', N'Hernandez Cruz', CAST('1945-09-12' AS DATE), N'8355-1176', N'luis.hernandez@correo.com', N'Granada, Nicaragua', 1),
 (N'Carmen', N'Sanchez Aguilar', CAST('1963-02-27' AS DATE), N'8478-3320', N'carmen.sanchez@correo.com', N'Granada, Nicaragua', 2),
 (N'Pedro', N'Castillo Flores', CAST('1950-12-08' AS DATE), N'8901-6642', N'pedro.castillo@correo.com', N'Granada, Nicaragua', 1),
 (N'Sofia', N'Mendoza Rivas', CAST('1968-06-14' AS DATE), N'8533-0987', N'sofia.mendoza@correo.com', N'Granada, Nicaragua', 2),
 (N'Miguel', N'Torres Blandon', CAST('1958-04-05' AS DATE), N'8267-5518', N'miguel.torres@correo.com', N'Granada, Nicaragua', 2)
) AS v(Nombre, Apellido, FechaNacimiento, Telefono, Correo, Direccion, Tipo)
WHERE NOT EXISTS (SELECT 1 FROM Pacientes p WHERE p.Nombre = v.Nombre AND p.Apellido = v.Apellido);

-- ---------------------------------------------------------------------
-- 2) TRATAMIENTOS (10, uno por paciente)
-- ---------------------------------------------------------------------
INSERT INTO Tratamientos (PacienteID, MedicamentoID, Dosis, FechaInicio, FechaFin, Instrucciones)
SELECT p.PacienteID, m.MedicamentoID, v.Dosis, v.Inicio, v.Fin, v.Instrucciones
FROM (VALUES
 (N'Maria', N'Garcia Lopez', N'Metformina', N'850 mg', CAST('2026-10-01' AS DATE), CAST(NULL AS DATE), N'Tomar con el desayuno y con el almuerzo'),
 (N'Jose', N'Ramirez Mena', N'Losartán', N'50 mg', CAST('2026-10-01' AS DATE), CAST(NULL AS DATE), N'Tomar en ayunas con un vaso de agua'),
 (N'Rosa', N'Martinez Obando', N'Levotiroxina', N'100 mcg', CAST('2026-10-05' AS DATE), CAST(NULL AS DATE), N'Tomar en ayunas, 30 minutos antes del desayuno'),
 (N'Carlos', N'Lopez Ruiz', N'Amlodipino', N'5 mg', CAST('2026-10-05' AS DATE), CAST(NULL AS DATE), N'Tomar a la misma hora todos los dias'),
 (N'Ana', N'Gutierrez Pineda', N'Omeprazol', N'20 mg', CAST('2026-10-10' AS DATE), CAST('2026-12-10' AS DATE), N'Tomar antes del desayuno durante 2 meses'),
 (N'Luis', N'Hernandez Cruz', N'Enalapril', N'10 mg', CAST('2026-10-10' AS DATE), CAST(NULL AS DATE), N'Controlar la presion arterial una vez por semana'),
 (N'Carmen', N'Sanchez Aguilar', N'Atorvastatina', N'20 mg', CAST('2026-10-15' AS DATE), CAST(NULL AS DATE), N'Tomar con alimentos'),
 (N'Pedro', N'Castillo Flores', N'Aspirina (AAS)', N'100 mg', CAST('2026-10-15' AS DATE), CAST(NULL AS DATE), N'Tomar despues de comer'),
 (N'Sofia', N'Mendoza Rivas', N'Vitamina B12 (Cianocobalamina)', N'1 mg', CAST('2026-10-20' AS DATE), CAST('2027-01-20' AS DATE), N'Tomar con el almuerzo durante 3 meses'),
 (N'Miguel', N'Torres Blandon', N'Furosemida', N'40 mg', CAST('2026-10-20' AS DATE), CAST(NULL AS DATE), N'Tomar por la manana; vigilar el consumo de liquidos')
) AS v(Nombre, Apellido, Medicamento, Dosis, Inicio, Fin, Instrucciones)
INNER JOIN Pacientes    p ON p.Nombre = v.Nombre AND p.Apellido = v.Apellido
INNER JOIN Medicamentos m ON m.Nombre = v.Medicamento
WHERE NOT EXISTS (SELECT 1 FROM Tratamientos t
                  WHERE t.PacienteID = p.PacienteID AND t.MedicamentoID = m.MedicamentoID AND t.Activo = 1);

-- ---------------------------------------------------------------------
-- 3) HORARIOS (todos entre las 07:00 y las 13:00; DiaSemana NULL = todos los dias)
-- ---------------------------------------------------------------------
INSERT INTO Horarios (TratamientoID, HoraToma, DiaSemana)
SELECT t.TratamientoID, v.Hora, NULL
FROM (VALUES
 (N'Maria', N'Garcia Lopez', N'Metformina', CAST('07:00' AS TIME)),
 (N'Maria', N'Garcia Lopez', N'Metformina', CAST('13:00' AS TIME)),
 (N'Jose', N'Ramirez Mena', N'Losartán', CAST('07:30' AS TIME)),
 (N'Rosa', N'Martinez Obando', N'Levotiroxina', CAST('07:00' AS TIME)),
 (N'Carlos', N'Lopez Ruiz', N'Amlodipino', CAST('08:00' AS TIME)),
 (N'Ana', N'Gutierrez Pineda', N'Omeprazol', CAST('08:30' AS TIME)),
 (N'Luis', N'Hernandez Cruz', N'Enalapril', CAST('09:00' AS TIME)),
 (N'Carmen', N'Sanchez Aguilar', N'Atorvastatina', CAST('10:00' AS TIME)),
 (N'Pedro', N'Castillo Flores', N'Aspirina (AAS)', CAST('11:00' AS TIME)),
 (N'Sofia', N'Mendoza Rivas', N'Vitamina B12 (Cianocobalamina)', CAST('12:00' AS TIME)),
 (N'Miguel', N'Torres Blandon', N'Furosemida', CAST('09:30' AS TIME)),
 (N'Miguel', N'Torres Blandon', N'Furosemida', CAST('13:00' AS TIME))
) AS v(Nombre, Apellido, Medicamento, Hora)
INNER JOIN Pacientes    p ON p.Nombre = v.Nombre AND p.Apellido = v.Apellido
INNER JOIN Medicamentos m ON m.Nombre = v.Medicamento
INNER JOIN Tratamientos t ON t.PacienteID = p.PacienteID AND t.MedicamentoID = m.MedicamentoID AND t.Activo = 1
WHERE NOT EXISTS (SELECT 1 FROM Horarios x WHERE x.TratamientoID = t.TratamientoID AND x.HoraToma = v.Hora);


SELECT (SELECT COUNT(*) FROM Pacientes    WHERE Activo = 1) AS Pacientes,
       (SELECT COUNT(*) FROM Tratamientos WHERE Activo = 1) AS Tratamientos,
       (SELECT COUNT(*) FROM Horarios     WHERE Activo = 1) AS Horarios;

SELECT p.Nombre + ' ' + p.Apellido AS Paciente, m.Nombre AS Medicamento, t.Dosis,
       CONVERT(VARCHAR(5), h.HoraToma, 108) AS Hora
FROM Horarios h
INNER JOIN Tratamientos t ON t.TratamientoID = h.TratamientoID
INNER JOIN Pacientes    p ON p.PacienteID    = t.PacienteID
INNER JOIN Medicamentos m ON m.MedicamentoID = t.MedicamentoID
ORDER BY h.HoraToma, p.Apellido;
