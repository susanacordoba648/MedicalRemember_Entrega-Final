
"""
reporte_pdf.py - Reporte PDF con reportlab y graficos
"""

import math
from datetime import datetime

from colecciones import calcular_cumplimiento


# ======================================================================
# GRAFICOS AUTOMATICOS (se calculan con los datos que exporta el sistema)
# ======================================================================
def _clasificar_estado(texto):
    """Agrupa el texto de 'Estado' en 4 categorias: Tomado, Tardio, Omitido o Pendiente."""
    t = (texto or "").lower()
    if "tard" in t:
        return "Tardio"
    if "tomado" in t:
        return "Tomado"
    if "omit" in t:
        return "Omitido"
    return "Pendiente"


def _nombre_corto(nombre):
    """'Maria Garcia Lopez' -> 'Maria G.' (para que quepa en el eje del grafico)."""
    partes = (nombre or "").split()
    return partes[0] + " " + partes[1][0] + "." if len(partes) > 1 else (nombre or "-")


def preparar_datos_graficos(historial, hoy):
    """Devuelve los numeros que alimentan los graficos del PDF (todo sale de los datos del sistema)."""
    estados = {"Tomado": 0, "Tardio": 0, "Omitido": 0, "Pendiente": 0}      # diccionario
    por_paciente_estados = {}
    for h in historial:
        est = _clasificar_estado(h.get("Estado", ""))
        estados[est] += 1
        por_paciente_estados.setdefault(h.get("Paciente", "-"), []).append(est)

    cumplimiento = []                                       # reutiliza calcular_cumplimiento()
    for pac in sorted(por_paciente_estados):
        tomados, total, pct = calcular_cumplimiento(por_paciente_estados[pac])
        cumplimiento.append((pac, pct, tomados, total))

    horas = {}                                              # tomas programadas hoy, por hora
    for fila in hoy:
        hh = (fila.get("Hora", "") or "")[:2]
        if hh.isdigit():
            horas[hh] = horas.get(hh, 0) + 1
    return {"estados": estados, "cumplimiento": cumplimiento, "horas": sorted(horas.items())}


# ======================================================================
# REPORTE PDF (libreria reportlab:  pip install reportlab)
# ======================================================================
def generar_pdf(ruta, usuario, datos):
    from reportlab.lib import colors
    from reportlab.lib.pagesizes import letter
    from reportlab.lib.styles import getSampleStyleSheet, ParagraphStyle
    from reportlab.lib.units import inch
    from reportlab.platypus import SimpleDocTemplate, Paragraph, Spacer, Table, TableStyle
    from reportlab.graphics.shapes import Drawing, Circle, Line, String

    azul = colors.HexColor("#195096")
    estilos = getSampleStyleSheet()
    h1 = ParagraphStyle("h1", parent=estilos["Title"], textColor=azul, fontSize=20)
    h2 = ParagraphStyle("h2", parent=estilos["Heading2"], textColor=azul, spaceBefore=12, keepWithNext=1)
    normal = estilos["BodyText"]
    chico = ParagraphStyle("chico", parent=normal, fontSize=8, textColor=colors.grey)

    def tabla(filas, encabezado, anchos=None):
        data = [encabezado] + (filas or [["(sin datos)"] + [""] * (len(encabezado) - 1)])
        t = Table(data, colWidths=anchos, repeatRows=1)
        t.setStyle(TableStyle([
            ("BACKGROUND", (0, 0), (-1, 0), azul),
            ("TEXTCOLOR", (0, 0), (-1, 0), colors.white),
            ("FONTNAME", (0, 0), (-1, 0), "Helvetica-Bold"),
            ("FONTSIZE", (0, 0), (-1, -1), 8),
            ("GRID", (0, 0), (-1, -1), 0.4, colors.lightgrey),
            ("ROWBACKGROUNDS", (0, 1), (-1, -1), [colors.white, colors.HexColor("#F5F8FC")]),
            ("VALIGN", (0, 0), (-1, -1), "TOP"),
        ]))
        return t

    def dibujar_grafo(grafo):
        """Dibuja el grafo con los vertices en circulo (solo los que tienen interacciones)."""
        nodos = [n for n in grafo.adyacencia if grafo.adyacencia[n]]
        w, h, r = 480, 330, 125
        d = Drawing(w, h)
        cx, cy = w / 2, h / 2
        pos = {}
        for i, n in enumerate(nodos):
            ang = 2 * math.pi * i / max(1, len(nodos))
            pos[n] = (cx + r * 1.45 * math.cos(ang), cy + r * math.sin(ang))
        color_g = {"Alta": colors.red, "Media": colors.orange, "Baja": colors.HexColor("#999999")}
        for a in nodos:
            for b, grav in grafo.adyacencia[a].items():
                if a < b:
                    d.add(Line(pos[a][0], pos[a][1], pos[b][0], pos[b][1],
                               strokeColor=color_g[grav], strokeWidth=1.4 if grav != "Baja" else 0.8))
        for n in nodos:
            x, y = pos[n]
            d.add(Circle(x, y, 4, fillColor=azul, strokeColor=azul))
            d.add(String(x + (6 if x >= cx else -6), y - 3, n, fontSize=6.5,
                         textAnchor="start" if x >= cx else "end"))
        return d

    doc = SimpleDocTemplate(ruta, pagesize=letter, leftMargin=0.7 * inch, rightMargin=0.7 * inch,
                            topMargin=0.7 * inch, bottomMargin=0.7 * inch,
                            title="Medical Remember - Reporte de estructuras de datos")
    e = []
    e.append(Paragraph("Medical Remember", h1))
    e.append(Paragraph("Reporte del modulo Python - Estructuras de datos", estilos["Heading3"]))
    e.append(Paragraph("Generado: %s | Usuario: %s | Origen de datos: %s" % (
        datetime.now().strftime("%d/%m/%Y %H:%M"), usuario, datos["origen"]), chico))

    tomados, total, pct = datos["cumplimiento"]
    e.append(Paragraph("1. Cumplimiento (funcion calcular_cumplimiento)", h2))
    e.append(tabla([[str(total), str(tomados), "%.1f %%" % pct]], ["Registros", "Tomados", "Cumplimiento"]))

    e.append(Paragraph("2. Cola (FIFO) - tomas pendientes de hoy", h2))
    e.append(tabla(datos["cola"], ["Hora", "Paciente", "Medicamento", "Dosis"]))

    e.append(Paragraph("3. Pila (LIFO) - tomas confirmadas (de la cima al fondo)", h2))
    e.append(tabla([[f] for f in datos["pila"][:15]], ["Toma (programado | paciente | medicamento)"]))

    e.append(Paragraph("4. Lista enlazada - medicamentos de " + (datos["lista_paciente"] or "-"), h2))
    e.append(Paragraph(str(datos["lista"]), normal))

    e.append(Paragraph("5. Diccionario y conjunto", h2))
    e.append(tabla([[p, ", ".join(m)] for p, m in datos["por_paciente"].items()], ["Paciente", "Medicamentos"],
                   [1.9 * inch, 5.0 * inch]))
    cj = datos["conjunto"]
    e.append(Spacer(1, 6))
    e.append(Paragraph("Medicamentos distintos (conjunto): " + ", ".join(cj["todos"]), normal))
    if cj["a"]:
        e.append(Paragraph("En comun entre %s y %s: %s" % (cj["a"], cj["b"], ", ".join(cj["comunes"]) or "ninguno"), normal))

    e.append(Paragraph("6. Grafo de interacciones (estructura no lineal)", h2))
    e.append(dibujar_grafo(datos["grafo"]))
    e.append(Paragraph("Rojo = gravedad alta, naranja = media, gris = baja.", chico))
    e.append(tabla([[p, a + " + " + b, g, desc] for p, a, b, g, desc in datos["alertas"]],
                   ["Paciente", "Interaccion", "Gravedad", "Detalle"],
                   [1.5 * inch, 2.0 * inch, 0.8 * inch, 2.6 * inch]))
    e.append(Spacer(1, 10))
    e.append(Paragraph("Las interacciones son un ejemplo academico y no sustituyen la indicacion de un medico o farmaceutico.", chico))

    # ---- 7. Graficos generados automaticamente a partir de los datos del sistema ----
    graf = datos.get("graficos")
    if graf:
        from reportlab.platypus import PageBreak
        from reportlab.graphics.charts.barcharts import VerticalBarChart
        from reportlab.graphics.charts.piecharts import Pie

        e.append(PageBreak())
        e.append(Paragraph("7. Graficos generados automaticamente", h2))
        e.append(Paragraph("Los graficos se calculan al ejecutar el modulo, con los datos que exporta el "
                           "sistema (origen: %s)." % datos["origen"], chico))
        e.append(Spacer(1, 8))

        def lienzo(titulo_grafico, ancho=250):
            d = Drawing(ancho, 200)
            d.add(String(ancho / 2, 186, titulo_grafico, fontSize=9, textAnchor="middle", fillColor=azul))
            return d

        def sin_datos(d):
            d.add(String(125, 95, "(sin datos)", fontSize=9, textAnchor="middle", fillColor=colors.grey))
            return d

        def barras(titulo_grafico, categorias, valores, color, maximo=None):
            d = lienzo(titulo_grafico)
            if not valores or not any(v for v in valores):
                return sin_datos(d)
            bc = VerticalBarChart()
            bc.x, bc.y, bc.width, bc.height = 32, 38, 200, 125
            bc.data = [valores]
            bc.categoryAxis.categoryNames = categorias
            bc.categoryAxis.labels.fontSize = 7
            bc.categoryAxis.labels.angle = 0 if len(categorias) <= 4 else 30
            bc.categoryAxis.labels.dy = -2 if len(categorias) <= 4 else -8
            bc.valueAxis.valueMin = 0
            bc.valueAxis.valueMax = maximo if maximo else max(valores) * 1.25 or 1
            if not maximo and max(valores) <= 10:         # conteos pequenos: eje en numeros enteros
                bc.valueAxis.valueMax = int(max(valores)) + 1
                bc.valueAxis.valueStep = 1
            bc.valueAxis.labels.fontSize = 7
            bc.bars[0].fillColor = color
            bc.barLabelFormat = "%.0f"
            bc.barLabels.fontSize = 7
            bc.barLabels.nudge = 7
            d.add(bc)
            return d

        # a) Pastel: estados de las tomas
        d1 = lienzo("Estado de las tomas (historial)")
        est = {k: v for k, v in graf["estados"].items() if v}
        if est:
            pastel = Pie()
            pastel.x, pastel.y, pastel.width, pastel.height = 60, 40, 120, 120
            pastel.data = list(est.values())
            pastel.labels = ["%s (%d)" % (k, v) for k, v in est.items()]
            pastel.sideLabels = 1
            pastel.slices.fontSize = 7
            paleta = {"Tomado": colors.HexColor("#2E9E5B"), "Tardio": colors.HexColor("#F2A541"),
                      "Omitido": colors.HexColor("#D95D4F"), "Pendiente": colors.HexColor("#8FB8CC")}
            for i, k in enumerate(est):
                pastel.slices[i].fillColor = paleta[k]
            d1.add(pastel)
        else:
            sin_datos(d1)

        # b) Barras: % de cumplimiento por paciente
        cum = graf["cumplimiento"]
        d2 = barras("Cumplimiento por paciente (%)", [_nombre_corto(c[0]) for c in cum],
                    [round(c[1], 1) for c in cum], azul, maximo=100)

        # c) Barras: tomas programadas hoy, por hora
        d3 = barras("Tomas programadas hoy, por hora", [h + ":00" for h, _ in graf["horas"]],
                    [n for _, n in graf["horas"]], colors.HexColor("#1C7293"))

        cuadro = Table([[d1, d2], [d3, ""]], colWidths=[3.45 * inch, 3.45 * inch])
        cuadro.setStyle(TableStyle([("VALIGN", (0, 0), (-1, -1), "TOP")]))
        e.append(cuadro)

    def pie(canvas, doc_):
        canvas.saveState()
        canvas.setFont("Helvetica", 8)
        canvas.setFillColor(colors.grey)
        canvas.drawString(0.7 * inch, 0.45 * inch, "Medical Remember - Modulo Python")
        canvas.drawRightString(letter[0] - 0.7 * inch, 0.45 * inch, "Pagina %d" % doc_.page)
        canvas.restoreState()

    doc.build(e, onFirstPage=pie, onLaterPages=pie)
