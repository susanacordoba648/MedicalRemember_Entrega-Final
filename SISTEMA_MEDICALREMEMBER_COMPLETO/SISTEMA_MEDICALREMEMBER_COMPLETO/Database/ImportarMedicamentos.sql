-- =========================================================================
-- IMPORTACION: Catalogo de medicamentos comunes (Adultos Mayores / Cronicos)
-- Generado a partir de Medicamentos_Comunes.xlsx
-- =========================================================================
USE MedicalRememberDB;
GO

-- Si el medicamento ya existe (por nombre) no se vuelve a insertar

IF NOT EXISTS (SELECT 1 FROM Medicamentos WHERE Nombre = N'Metformina')
    INSERT INTO Medicamentos (Nombre, Descripcion, Unidad, ViaAdministracion)
    VALUES (N'Metformina', N'Control de glucosa en diabetes tipo 2 (Dosis habitual: 850 mg). Uso frecuente en: Crónico.', N'mg', N'Oral');
GO
IF NOT EXISTS (SELECT 1 FROM Medicamentos WHERE Nombre = N'Glibenclamida')
    INSERT INTO Medicamentos (Nombre, Descripcion, Unidad, ViaAdministracion)
    VALUES (N'Glibenclamida', N'Diabetes tipo 2 (estimula liberación de insulina) (Dosis habitual: 5 mg). Uso frecuente en: Crónico.', N'mg', N'Oral');
GO
IF NOT EXISTS (SELECT 1 FROM Medicamentos WHERE Nombre = N'Insulina NPH')
    INSERT INTO Medicamentos (Nombre, Descripcion, Unidad, ViaAdministracion)
    VALUES (N'Insulina NPH', N'Diabetes tipo 1 y tipo 2 avanzada (Dosis habitual: 100 UI/mL). Uso frecuente en: Crónico.', N'ml', N'Inyectable');
GO
IF NOT EXISTS (SELECT 1 FROM Medicamentos WHERE Nombre = N'Insulina Glargina')
    INSERT INTO Medicamentos (Nombre, Descripcion, Unidad, ViaAdministracion)
    VALUES (N'Insulina Glargina', N'Control basal de glucosa en diabetes (Dosis habitual: 100 UI/mL). Uso frecuente en: Crónico.', N'ml', N'Inyectable');
GO
IF NOT EXISTS (SELECT 1 FROM Medicamentos WHERE Nombre = N'Losartán')
    INSERT INTO Medicamentos (Nombre, Descripcion, Unidad, ViaAdministracion)
    VALUES (N'Losartán', N'Hipertensión arterial (Dosis habitual: 50 mg). Uso frecuente en: Ambos.', N'mg', N'Oral');
GO
IF NOT EXISTS (SELECT 1 FROM Medicamentos WHERE Nombre = N'Enalapril')
    INSERT INTO Medicamentos (Nombre, Descripcion, Unidad, ViaAdministracion)
    VALUES (N'Enalapril', N'Hipertensión arterial e insuficiencia cardiaca (Dosis habitual: 10 mg). Uso frecuente en: Ambos.', N'mg', N'Oral');
GO
IF NOT EXISTS (SELECT 1 FROM Medicamentos WHERE Nombre = N'Amlodipino')
    INSERT INTO Medicamentos (Nombre, Descripcion, Unidad, ViaAdministracion)
    VALUES (N'Amlodipino', N'Hipertensión arterial (Dosis habitual: 5 mg). Uso frecuente en: Adulto Mayor.', N'mg', N'Oral');
GO
IF NOT EXISTS (SELECT 1 FROM Medicamentos WHERE Nombre = N'Furosemida')
    INSERT INTO Medicamentos (Nombre, Descripcion, Unidad, ViaAdministracion)
    VALUES (N'Furosemida', N'Diurético; control de edema e hipertensión (Dosis habitual: 40 mg). Uso frecuente en: Ambos.', N'mg', N'Oral');
GO
IF NOT EXISTS (SELECT 1 FROM Medicamentos WHERE Nombre = N'Atorvastatina')
    INSERT INTO Medicamentos (Nombre, Descripcion, Unidad, ViaAdministracion)
    VALUES (N'Atorvastatina', N'Reducir el colesterol (dislipidemia) (Dosis habitual: 20 mg). Uso frecuente en: Crónico.', N'mg', N'Oral');
GO
IF NOT EXISTS (SELECT 1 FROM Medicamentos WHERE Nombre = N'Levotiroxina')
    INSERT INTO Medicamentos (Nombre, Descripcion, Unidad, ViaAdministracion)
    VALUES (N'Levotiroxina', N'Hipotiroidismo (Dosis habitual: 100 mcg). Uso frecuente en: Ambos.', N'mg', N'Oral');
GO
IF NOT EXISTS (SELECT 1 FROM Medicamentos WHERE Nombre = N'Aspirina (AAS)')
    INSERT INTO Medicamentos (Nombre, Descripcion, Unidad, ViaAdministracion)
    VALUES (N'Aspirina (AAS)', N'Prevención de eventos cardiovasculares (antiagregante) (Dosis habitual: 100 mg). Uso frecuente en: Adulto Mayor.', N'mg', N'Oral');
GO
IF NOT EXISTS (SELECT 1 FROM Medicamentos WHERE Nombre = N'Clopidogrel')
    INSERT INTO Medicamentos (Nombre, Descripcion, Unidad, ViaAdministracion)
    VALUES (N'Clopidogrel', N'Antiagregante plaquetario; prevención de infartos/ACV (Dosis habitual: 75 mg). Uso frecuente en: Crónico.', N'mg', N'Oral');
GO
IF NOT EXISTS (SELECT 1 FROM Medicamentos WHERE Nombre = N'Warfarina')
    INSERT INTO Medicamentos (Nombre, Descripcion, Unidad, ViaAdministracion)
    VALUES (N'Warfarina', N'Anticoagulante oral; prevención de trombos (Dosis habitual: 5 mg). Uso frecuente en: Crónico.', N'mg', N'Oral');
GO
IF NOT EXISTS (SELECT 1 FROM Medicamentos WHERE Nombre = N'Enoxaparina')
    INSERT INTO Medicamentos (Nombre, Descripcion, Unidad, ViaAdministracion)
    VALUES (N'Enoxaparina', N'Anticoagulante; prevención de trombosis venosa (Dosis habitual: 40 mg). Uso frecuente en: Crónico.', N'mg', N'Inyectable');
GO
IF NOT EXISTS (SELECT 1 FROM Medicamentos WHERE Nombre = N'Omeprazol')
    INSERT INTO Medicamentos (Nombre, Descripcion, Unidad, ViaAdministracion)
    VALUES (N'Omeprazol', N'Gastritis y reflujo gastroesofágico (Dosis habitual: 20 mg). Uso frecuente en: Ambos.', N'mg', N'Oral');
GO
IF NOT EXISTS (SELECT 1 FROM Medicamentos WHERE Nombre = N'Alendronato')
    INSERT INTO Medicamentos (Nombre, Descripcion, Unidad, ViaAdministracion)
    VALUES (N'Alendronato', N'Osteoporosis (Dosis habitual: 70 mg semanal). Uso frecuente en: Adulto Mayor.', N'mg', N'Oral');
GO
IF NOT EXISTS (SELECT 1 FROM Medicamentos WHERE Nombre = N'Vitamina B12 (Cianocobalamina)')
    INSERT INTO Medicamentos (Nombre, Descripcion, Unidad, ViaAdministracion)
    VALUES (N'Vitamina B12 (Cianocobalamina)', N'Deficiencia de vitamina B12, frecuente en adultos mayores (Dosis habitual: 1000 mcg). Uso frecuente en: Adulto Mayor.', N'mg', N'Inyectable');
GO
IF NOT EXISTS (SELECT 1 FROM Medicamentos WHERE Nombre = N'Ácido Fólico')
    INSERT INTO Medicamentos (Nombre, Descripcion, Unidad, ViaAdministracion)
    VALUES (N'Ácido Fólico', N'Suplemento; tratamiento de anemia (Dosis habitual: 5 mg). Uso frecuente en: Adulto Mayor.', N'mg', N'Oral');
GO
IF NOT EXISTS (SELECT 1 FROM Medicamentos WHERE Nombre = N'Amoxicilina')
    INSERT INTO Medicamentos (Nombre, Descripcion, Unidad, ViaAdministracion)
    VALUES (N'Amoxicilina', N'Antibiótico de amplio espectro (Dosis habitual: 500 mg). Uso frecuente en: Ambos.', N'mg', N'Oral');
GO
IF NOT EXISTS (SELECT 1 FROM Medicamentos WHERE Nombre = N'Paracetamol')
    INSERT INTO Medicamentos (Nombre, Descripcion, Unidad, ViaAdministracion)
    VALUES (N'Paracetamol', N'Analgésico y antipirético (Dosis habitual: 500 mg). Uso frecuente en: Ambos.', N'mg', N'Oral');
GO

PRINT 'Catalogo de medicamentos comunes importado correctamente.';
GO