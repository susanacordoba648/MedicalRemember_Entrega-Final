
"""
utilidades.py - Funciones compartidas por todos los archivos
"""

import csv
import os
import re


# ======================================================================
# UTILIDADES
# ======================================================================
SALIDA = []          # aqui se guarda todo lo que se imprime (para el PDF)


def mostrar(texto=""):
    """Imprime en consola y guarda la linea para el reporte."""
    print(texto)
    SALIDA.append(texto)


def titulo(texto):
    mostrar("")
    mostrar("=" * 70)
    mostrar(texto)
    mostrar("=" * 70)


def limpiar_texto(texto):
    """Quita emojis/simbolos que SQL Server manda en 'Estado' (ej. '✅ Tomado' -> 'Tomado')."""
    return re.sub(r"[^\w\s().,:/-]", "", texto or "").strip()


def leer_csv(ruta):
    """Lee un CSV exportado por C# (UTF-8 con BOM) y devuelve una LISTA de diccionarios."""
    if not ruta or not os.path.exists(ruta):
        return []
    with open(ruta, newline="", encoding="utf-8-sig") as f:
        return list(csv.DictReader(f))


# ======================================================================
# DATOS DE EJEMPLO (solo se usan si se ejecuta sin los CSV de C#)
# ======================================================================
def datos_ejemplo():
    trat = [
        {"Paciente": "Maria Garcia Lopez", "Medicamento": "Warfarina", "Dosis": "5 mg"},
        {"Paciente": "Maria Garcia Lopez", "Medicamento": "Aspirina (AAS)", "Dosis": "100 mg"},
        {"Paciente": "Maria Garcia Lopez", "Medicamento": "Losartán", "Dosis": "50 mg"},
        {"Paciente": "Maria Garcia Lopez", "Medicamento": "Omeprazol", "Dosis": "20 mg"},
        {"Paciente": "Jose Perez", "Medicamento": "Metformina", "Dosis": "850 mg"},
        {"Paciente": "Jose Perez", "Medicamento": "Losartán", "Dosis": "50 mg"},
        {"Paciente": "Jose Perez", "Medicamento": "Enalapril", "Dosis": "10 mg"},
        {"Paciente": "Ana Lopez", "Medicamento": "Levotiroxina", "Dosis": "100 mcg"},
        {"Paciente": "Ana Lopez", "Medicamento": "Omeprazol", "Dosis": "20 mg"},
    ]
    hoy = [
        {"Paciente": "Maria Garcia Lopez", "Medicamento": "Warfarina", "Dosis": "5 mg", "Hora": "08:00", "Estado": "✅ Tomado"},
        {"Paciente": "Jose Perez", "Medicamento": "Metformina", "Dosis": "850 mg", "Hora": "12:00", "Estado": "🕐 Pendiente"},
        {"Paciente": "Ana Lopez", "Medicamento": "Levotiroxina", "Dosis": "100 mcg", "Hora": "07:00", "Estado": "🕐 Pendiente"},
        {"Paciente": "Maria Garcia Lopez", "Medicamento": "Losartán", "Dosis": "50 mg", "Hora": "20:00", "Estado": "🕐 Pendiente"},
        {"Paciente": "Jose Perez", "Medicamento": "Enalapril", "Dosis": "10 mg", "Hora": "14:30", "Estado": "🕐 Pendiente"},
    ]
    hist = [
        {"Paciente": "Maria Garcia Lopez", "Medicamento": "Warfarina", "Dosis": "5 mg", "Programado": "2026-11-01 08:00", "Tomado": "2026-11-01 08:05", "Estado": "✅ Tomado"},
        {"Paciente": "Maria Garcia Lopez", "Medicamento": "Warfarina", "Dosis": "5 mg", "Programado": "2026-11-02 08:00", "Tomado": "2026-11-02 08:50", "Estado": "⏰ Tardio"},
        {"Paciente": "Maria Garcia Lopez", "Medicamento": "Warfarina", "Dosis": "5 mg", "Programado": "2026-11-03 08:00", "Tomado": "--", "Estado": "❌ Omitido"},
        {"Paciente": "Jose Perez", "Medicamento": "Metformina", "Dosis": "850 mg", "Programado": "2026-11-03 12:00", "Tomado": "2026-11-03 12:02", "Estado": "✅ Tomado"},
        {"Paciente": "Ana Lopez", "Medicamento": "Levotiroxina", "Dosis": "100 mcg", "Programado": "2026-11-04 07:00", "Tomado": "2026-11-04 07:01", "Estado": "✅ Tomado"},
        {"Paciente": "Ana Lopez", "Medicamento": "Levotiroxina", "Dosis": "100 mcg", "Programado": "2026-11-05 07:00", "Tomado": "2026-11-05 07:03", "Estado": "✅ Tomado"},
    ]
    return trat, hoy, hist
