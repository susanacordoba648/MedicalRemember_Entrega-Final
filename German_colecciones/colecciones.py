# -*- coding: utf-8 -*-
"""
colecciones.py - Colecciones de Python y la funcion de cumplimiento
"""

from utilidades import mostrar, titulo


# ======================================================================
# LA FUNCION (seccion 7): misma regla que HistorialServicio.CalcularCumplimiento (C#)
# ======================================================================
def calcular_cumplimiento(estados):
    """Recibe una lista de estados ('Tomado', 'Omitido', ...) y devuelve
    (tomados, total, porcentaje)."""
    total = len(estados)
    tomados = sum(1 for e in estados if "tomado" in e.lower())
    porcentaje = (tomados * 100.0 / total) if total > 0 else 0.0
    return tomados, total, porcentaje


# ======================================================================
# DEMOSTRACIONES (cada funcion devuelve datos para el PDF)
# ======================================================================
def demo_tupla(hoy):
    titulo("4) TUPLA (inmutable) - una toma = (paciente, medicamento, hora)")
    if not hoy:
        mostrar("(sin datos)")
        return []
    tomas = [(h["Paciente"], h["Medicamento"], h["Hora"]) for h in hoy]
    paciente, medicamento, hora = tomas[0]                    # desempaquetado
    mostrar("Primera toma: %s" % (tomas[0],))
    mostrar("Desempaquetada -> paciente=%s | medicamento=%s | hora=%s" % (paciente, medicamento, hora))
    try:
        tomas[0][2] = "00:00"
    except TypeError as e:
        mostrar("Intentar modificarla da error (es inmutable): TypeError - %s" % e)
    return tomas


def demo_diccionario(tratamientos, historial):
    titulo("5) DICCIONARIO - medicamentos por paciente y conteo por estado")
    por_paciente = {}
    for t in tratamientos:
        por_paciente.setdefault(t["Paciente"], []).append(t["Medicamento"])
    for pac, meds in por_paciente.items():
        mostrar("%s -> %s" % (pac, ", ".join(meds)))
    conteo = {}
    for h in historial:
        e = h["Estado"]
        conteo[e] = conteo.get(e, 0) + 1
    mostrar("Conteo de estados del historial: " + str(conteo))
    return por_paciente, conteo


def demo_conjunto(por_paciente):
    titulo("6) CONJUNTO (set) - medicamentos unicos y operaciones")
    todos = set()
    for meds in por_paciente.values():
        todos |= set(meds)
    mostrar("Medicamentos distintos en el sistema (%d): %s" % (len(todos), ", ".join(sorted(todos))))
    resultado = {"todos": sorted(todos), "comunes": [], "solo_a": [], "solo_b": [], "a": "", "b": ""}
    nombres = list(por_paciente.keys())
    if len(nombres) >= 2:
        a, b = nombres[0], nombres[1]
        sa, sb = set(por_paciente[a]), set(por_paciente[b])
        resultado.update(a=a, b=b, comunes=sorted(sa & sb), solo_a=sorted(sa - sb), solo_b=sorted(sb - sa))
        mostrar("Entre %s y %s:" % (a, b))
        mostrar("  Interseccion (en comun): " + (", ".join(sorted(sa & sb)) or "ninguno"))
        mostrar("  Diferencia (solo %s):    %s" % (a, ", ".join(sorted(sa - sb)) or "ninguno"))
        mostrar("  Diferencia (solo %s):    %s" % (b, ", ".join(sorted(sb - sa)) or "ninguno"))
    return resultado


def demo_funcion(historial):
    titulo("7) FUNCION calcular_cumplimiento(estados)")
    estados = [h["Estado"] for h in historial]
    tomados, total, pct = calcular_cumplimiento(estados)
    mostrar("Registros: %d | Tomados: %d | Cumplimiento: %.1f%%" % (total, tomados, pct))
    return tomados, total, pct
