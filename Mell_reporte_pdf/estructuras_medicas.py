# -*- coding: utf-8 -*-
"""
MEDICAL REMEMBER - Modulo Python (complemento del proyecto WinForms en C#)
"""

import argparse
import os
import sys
from datetime import datetime

from utilidades import SALIDA, mostrar, titulo, limpiar_texto, leer_csv, datos_ejemplo
from estructuras_lineales import demo_lista, demo_pila, demo_cola
from colecciones import demo_tupla, demo_diccionario, demo_conjunto, demo_funcion
from grafo import construir_grafo, demo_grafo
from reporte_pdf import generar_pdf, preparar_datos_graficos


# ======================================================================
# PROGRAMA PRINCIPAL
# ======================================================================
def main():
    if hasattr(sys.stdout, "reconfigure"):
        sys.stdout.reconfigure(encoding="utf-8")

    ap = argparse.ArgumentParser(description="Medical Remember - modulo Python de estructuras de datos")
    ap.add_argument("--tratamientos", help="CSV con Paciente, Medicamento, Dosis...")
    ap.add_argument("--hoy", help="CSV de las tomas programadas para hoy")
    ap.add_argument("--historial", help="CSV del historial de cumplimiento")
    ap.add_argument("--salida", default="reporte_estructuras.pdf", help="ruta del PDF a generar")
    ap.add_argument("--usuario", default="(sin sesion)", help="usuario que genera el reporte")
    args = ap.parse_args()

    tratamientos = leer_csv(args.tratamientos)
    hoy = leer_csv(args.hoy)
    historial = leer_csv(args.historial)
    origen = "SQL Server (exportado por C#)"
    if not (args.tratamientos or args.hoy or args.historial):  
        tratamientos, hoy, historial = datos_ejemplo()
        origen = "datos de ejemplo"

    for fila in hoy + historial:                    
        fila["Estado"] = limpiar_texto(fila.get("Estado", ""))

    mostrar("MEDICAL REMEMBER - MODULO PYTHON")
    mostrar("Origen de datos: %s | %s" % (origen, datetime.now().strftime("%d/%m/%Y %H:%M")))
    mostrar("Tratamientos: %d | Tomas de hoy: %d | Registros de historial: %d" % (len(tratamientos), len(hoy), len(historial)))

    lista_pac, lista = demo_lista(tratamientos)
    pila_filas, _ = demo_pila(historial)
    cola_filas, _ = demo_cola(hoy)
    demo_tupla(hoy)
    por_paciente, _ = demo_diccionario(tratamientos, historial)
    conjunto = demo_conjunto(por_paciente)
    cumplimiento = demo_funcion(historial)
    todos = {m for meds in por_paciente.values() for m in meds}
    grafo = construir_grafo(todos)
    _, alertas = demo_grafo(grafo, por_paciente)

    datos = {"origen": origen, "cumplimiento": cumplimiento, "cola": cola_filas, "pila": pila_filas,
             "lista_paciente": lista_pac, "lista": lista, "por_paciente": por_paciente,
             "conjunto": conjunto, "grafo": grafo, "alertas": alertas}
    datos["graficos"] = preparar_datos_graficos(historial, hoy)             # graficos automaticos del PDF
    mostrar("Graficos automaticos: 3 (estado de las tomas, cumplimiento por paciente, tomas de hoy por hora)")

    titulo("REPORTE")
    try:
        generar_pdf(args.salida, args.usuario, datos)
        mostrar("PDF_GENERADO=" + os.path.abspath(args.salida))
    except ImportError:
        mostrar("No esta instalada la libreria reportlab. Ejecuta:  pip install reportlab")
        ruta_txt = os.path.splitext(args.salida)[0] + ".txt"
        with open(ruta_txt, "w", encoding="utf-8") as f:
            f.write("\n".join(SALIDA))
        mostrar("TXT_GENERADO=" + os.path.abspath(ruta_txt))


if __name__ == "__main__":
    main()
