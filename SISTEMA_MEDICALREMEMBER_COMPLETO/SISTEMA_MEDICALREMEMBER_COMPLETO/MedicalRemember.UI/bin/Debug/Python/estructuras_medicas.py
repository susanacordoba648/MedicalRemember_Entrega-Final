# -*- coding: utf-8 -*-
"""
MEDICAL REMEMBER - Modulo Python (complemento del proyecto WinForms en C#)
==========================================================================
Materia : Estructuras de datos
Contenido que pide el profesor y donde esta en este archivo:

  * 1 FUNCION ............... calcular_cumplimiento()            (seccion 7)
  * 3 ESTRUCTURAS PROPIAS ... ListaEnlazada, Pila, Cola          (secciones 1, 2 y 3)
  * COLECCIONES ............. tupla, diccionario, conjunto       (secciones 4, 5 y 6)
                              (+ lista, pila y cola de arriba)
  * ESTRUCTURA NO LINEAL .... GRAFO de interacciones entre medicamentos (seccion 8)
  * RECURSIVIDAD ............ GrafoInteracciones.dfs_recursivo()
  * REPORTE ................. PDF generado con la libreria reportlab

Como se usa
-----------
1) Desde el sistema (WinForms): boton "Modulo Python" del menu lateral.
   C# exporta los datos de SQL Server a CSV y llama a este script.
2) Solo desde VS Code (para exponer): abrir esta carpeta y ejecutar el archivo
   sin argumentos. En ese caso usa DATOS DE EJEMPLO.

       python estructuras_medicas.py
       python estructuras_medicas.py --tratamientos t.csv --hoy h.csv --historial hi.csv --salida reporte.pdf
"""

import argparse
import csv
import math
import os
import re
import sys
from datetime import datetime


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
# ESTRUCTURA 1: LISTA ENLAZADA (lineal)
# Cada elemento es un Nodo que guarda un dato y apunta al siguiente.
# ======================================================================
class Nodo:
    def __init__(self, dato):
        self.dato = dato
        self.siguiente = None


class ListaEnlazada:
    """Lista simplemente enlazada. Uso en el sistema: medicamentos de un paciente."""

    def __init__(self):
        self.cabeza = None
        self._tamano = 0

    def agregar_al_inicio(self, dato):
        nuevo = Nodo(dato)
        nuevo.siguiente = self.cabeza
        self.cabeza = nuevo
        self._tamano += 1

    def agregar_al_final(self, dato):
        nuevo = Nodo(dato)
        if self.cabeza is None:
            self.cabeza = nuevo
        else:
            actual = self.cabeza
            while actual.siguiente is not None:     # recorre hasta el ultimo nodo
                actual = actual.siguiente
            actual.siguiente = nuevo
        self._tamano += 1

    def buscar(self, dato):
        """Devuelve la posicion (0, 1, 2...) del dato o -1 si no esta."""
        actual, pos = self.cabeza, 0
        while actual is not None:
            if actual.dato == dato:
                return pos
            actual = actual.siguiente
            pos += 1
        return -1

    def eliminar(self, dato):
        """Elimina la primera aparicion del dato. Devuelve True si lo encontro."""
        actual, anterior = self.cabeza, None
        while actual is not None:
            if actual.dato == dato:
                if anterior is None:
                    self.cabeza = actual.siguiente
                else:
                    anterior.siguiente = actual.siguiente
                self._tamano -= 1
                return True
            anterior, actual = actual, actual.siguiente
        return False

    def __iter__(self):
        actual = self.cabeza
        while actual is not None:
            yield actual.dato
            actual = actual.siguiente

    def __len__(self):
        return self._tamano

    def __str__(self):
        return " -> ".join(str(d) for d in self) + " -> None" if self.cabeza else "(lista vacia)"


# ======================================================================
# ESTRUCTURA 2: PILA (LIFO: el ultimo en entrar es el primero en salir)
# ======================================================================
class Pila:
    """Pila con nodos enlazados. Uso en el sistema: historial de tomas confirmadas
    (la cima es la ultima toma registrada, la que se 'deshace' primero)."""

    def __init__(self):
        self._cima = None
        self._tamano = 0

    def apilar(self, dato):                      # push
        nuevo = Nodo(dato)
        nuevo.siguiente = self._cima
        self._cima = nuevo
        self._tamano += 1

    def desapilar(self):                         # pop
        if self.esta_vacia():
            raise IndexError("La pila esta vacia")
        dato = self._cima.dato
        self._cima = self._cima.siguiente
        self._tamano -= 1
        return dato

    def cima(self):                              # peek
        if self.esta_vacia():
            raise IndexError("La pila esta vacia")
        return self._cima.dato

    def esta_vacia(self):
        return self._cima is None

    def __len__(self):
        return self._tamano

    def __iter__(self):                          # de la cima hacia el fondo
        actual = self._cima
        while actual is not None:
            yield actual.dato
            actual = actual.siguiente


# ======================================================================
# ESTRUCTURA 3: COLA (FIFO: el primero en entrar es el primero en salir)
# ======================================================================
class Cola:
    """Cola con nodos enlazados (frente y final). Uso en el sistema: tomas
    pendientes del dia, ordenadas por hora; se atiende primero la mas proxima."""

    def __init__(self):
        self._frente = None
        self._final = None
        self._tamano = 0

    def encolar(self, dato):                     # enqueue
        nuevo = Nodo(dato)
        if self._final is None:
            self._frente = self._final = nuevo
        else:
            self._final.siguiente = nuevo
            self._final = nuevo
        self._tamano += 1

    def desencolar(self):                        # dequeue
        if self.esta_vacia():
            raise IndexError("La cola esta vacia")
        dato = self._frente.dato
        self._frente = self._frente.siguiente
        if self._frente is None:
            self._final = None
        self._tamano -= 1
        return dato

    def frente(self):
        if self.esta_vacia():
            raise IndexError("La cola esta vacia")
        return self._frente.dato

    def esta_vacia(self):
        return self._frente is None

    def __len__(self):
        return self._tamano

    def __iter__(self):
        actual = self._frente
        while actual is not None:
            yield actual.dato
            actual = actual.siguiente


# ======================================================================
# ESTRUCTURA NO LINEAL: GRAFO (no dirigido, con pesos de gravedad)
# Vertices = medicamentos. Aristas = "estos dos medicamentos interactuan".
# Se guarda como DICCIONARIO DE ADYACENCIA:  {med: {otro_med: gravedad}}
# ======================================================================
GRAVEDAD_ORDEN = {"Alta": 3, "Media": 2, "Baja": 1}

# Interacciones de EJEMPLO con fines academicos (no reemplazan al criterio medico).
INTERACCIONES = [
    ("Warfarina", "Aspirina (AAS)", "Alta", "Mayor riesgo de sangrado"),
    ("Warfarina", "Clopidogrel", "Alta", "Mayor riesgo de sangrado"),
    ("Warfarina", "Enoxaparina", "Alta", "Doble anticoagulacion"),
    ("Warfarina", "Levotiroxina", "Media", "Puede aumentar el efecto anticoagulante"),
    ("Warfarina", "Paracetamol", "Baja", "El uso continuo puede elevar el INR"),
    ("Aspirina (AAS)", "Clopidogrel", "Media", "Suma efecto antiagregante"),
    ("Aspirina (AAS)", "Enoxaparina", "Media", "Mayor riesgo de sangrado"),
    ("Aspirina (AAS)", "Alendronato", "Media", "Irritacion gastrica"),
    ("Clopidogrel", "Omeprazol", "Media", "Omeprazol puede reducir su efecto"),
    ("Enalapril", "Losartan", "Media", "Doble bloqueo: riesgo de potasio alto"),
    ("Enalapril", "Furosemida", "Media", "Riesgo de hipotension / funcion renal"),
    ("Glibenclamida", "Insulina NPH", "Media", "Hipoglucemia aditiva"),
    ("Insulina NPH", "Insulina Glargina", "Media", "Duplicidad de insulina"),
    ("Atorvastatina", "Amlodipino", "Baja", "Leve aumento del nivel de estatina"),
    ("Levotiroxina", "Omeprazol", "Baja", "Menor absorcion de levotiroxina"),
]


def normalizar(nombre):
    """Quita tildes y pasa a minusculas para comparar nombres (Losartán == Losartan)."""
    tabla = str.maketrans("áéíóúÁÉÍÓÚüÜ", "aeiouAEIOUuU")
    return (nombre or "").translate(tabla).strip().lower()


class GrafoInteracciones:
    def __init__(self):
        self.adyacencia = {}          # diccionario de diccionarios
        self.detalle = {}             # (a, b) ordenado -> descripcion
        self._nombres = {}            # nombre normalizado -> nombre oficial

    # ---- construccion -------------------------------------------------
    def agregar_vertice(self, med):
        if med not in self.adyacencia:
            self.adyacencia[med] = {}
            self._nombres[normalizar(med)] = med

    def agregar_interaccion(self, a, b, gravedad, descripcion=""):
        self.agregar_vertice(a)
        self.agregar_vertice(b)
        self.adyacencia[a][b] = gravedad
        self.adyacencia[b][a] = gravedad                     # no dirigido
        self.detalle[tuple(sorted((a, b)))] = descripcion

    def nombre_oficial(self, med):
        return self._nombres.get(normalizar(med))

    def vecinos(self, med):
        return list(self.adyacencia.get(med, {}).keys())

    def total_aristas(self):
        return sum(len(v) for v in self.adyacencia.values()) // 2

    # ---- recorrido en ANCHURA (BFS) usando nuestra Cola ---------------
    def bfs(self, inicio):
        visitados, orden = {inicio}, []
        cola = Cola()
        cola.encolar(inicio)
        while not cola.esta_vacia():
            actual = cola.desencolar()
            orden.append(actual)
            for v in self.vecinos(actual):
                if v not in visitados:
                    visitados.add(v)
                    cola.encolar(v)
        return orden

    # ---- recorrido en PROFUNDIDAD (DFS) con RECURSIVIDAD --------------
    def dfs_recursivo(self, actual, visitados=None, orden=None):
        if visitados is None:                  # primera llamada
            visitados, orden = set(), []
        visitados.add(actual)
        orden.append(actual)
        for v in self.vecinos(actual):
            if v not in visitados:             # llamada recursiva
                self.dfs_recursivo(v, visitados, orden)
        return orden

    # ---- camino mas corto (en numero de saltos) con BFS ---------------
    def camino_mas_corto(self, origen, destino):
        if origen not in self.adyacencia or destino not in self.adyacencia:
            return []
        padre = {origen: None}
        cola = Cola()
        cola.encolar(origen)
        while not cola.esta_vacia():
            actual = cola.desencolar()
            if actual == destino:
                break
            for v in self.vecinos(actual):
                if v not in padre:
                    padre[v] = actual
                    cola.encolar(v)
        if destino not in padre:
            return []
        camino, nodo = [], destino
        while nodo is not None:
            camino.append(nodo)
            nodo = padre[nodo]
        camino.reverse()
        return camino

    # ---- uso real: conflictos entre los medicamentos de UN paciente ---
    def conflictos(self, medicamentos):
        """Recibe los medicamentos de un paciente y devuelve las parejas que interactuan."""
        oficiales = []
        for m in medicamentos:
            o = self.nombre_oficial(m)
            if o and o not in oficiales:
                oficiales.append(o)
        encontrados = []
        for i in range(len(oficiales)):
            for j in range(i + 1, len(oficiales)):
                a, b = oficiales[i], oficiales[j]
                if b in self.adyacencia[a]:
                    encontrados.append((a, b, self.adyacencia[a][b],
                                        self.detalle[tuple(sorted((a, b)))]))
        encontrados.sort(key=lambda c: -GRAVEDAD_ORDEN[c[2]])
        return encontrados


def construir_grafo(todos_los_medicamentos=()):
    g = GrafoInteracciones()
    for a, b, grav, desc in INTERACCIONES:
        g.agregar_interaccion(a, b, grav, desc)
    for m in todos_los_medicamentos:          # medicamentos sin interacciones conocidas
        if not g.nombre_oficial(m):
            g.agregar_vertice(m)
    return g


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


# ======================================================================
# DEMOSTRACION DE CADA ESTRUCTURA  (cada funcion devuelve datos para el PDF)
# ======================================================================
def demo_lista(tratamientos):
    titulo("1) LISTA ENLAZADA - medicamentos del primer paciente")
    if not tratamientos:
        mostrar("(sin tratamientos)")
        return "", "(sin datos)"
    paciente = tratamientos[0]["Paciente"]
    lista = ListaEnlazada()
    for t in tratamientos:
        if t["Paciente"] == paciente:
            lista.agregar_al_final(t["Medicamento"])
    mostrar("Paciente: " + paciente)
    original = str(lista)                      # copia en texto antes de eliminar
    mostrar("Lista:    " + original)
    mostrar("Tamano:   " + str(len(lista)))
    primero = lista.cabeza.dato if lista.cabeza else None
    if primero:
        mostrar("buscar('%s') -> posicion %d" % (primero, lista.buscar(primero)))
        mostrar("eliminar('%s') -> %s" % (primero, lista.eliminar(primero)))
        mostrar("Lista despues de eliminar: " + str(lista))
    return paciente, original


def demo_pila(historial):
    titulo("2) PILA (LIFO) - historial de tomas confirmadas")
    pila = Pila()
    confirmadas = [h for h in historial if "tomado" in h["Estado"].lower()]
    for h in sorted(confirmadas, key=lambda x: x["Programado"]):
        pila.apilar("%s | %s | %s" % (h["Programado"], h["Paciente"], h["Medicamento"]))
    mostrar("Tomas apiladas: %d" % len(pila))
    filas = list(pila)
    if pila.esta_vacia():
        mostrar("(la pila esta vacia)")
        return filas, None
    mostrar("Cima (ultima toma registrada): " + pila.cima())
    deshecha = pila.desapilar()
    mostrar("desapilar() -> 'deshacer' simulado: " + deshecha)
    mostrar("Nueva cima: " + (pila.cima() if not pila.esta_vacia() else "(vacia)"))
    return filas, deshecha


def demo_cola(hoy):
    titulo("3) COLA (FIFO) - tomas pendientes de hoy")
    cola = Cola()
    pendientes = [h for h in hoy if "pendiente" in h["Estado"].lower()]
    for h in sorted(pendientes, key=lambda x: x["Hora"]):
        cola.encolar((h["Hora"], h["Paciente"], h["Medicamento"], h["Dosis"]))
    mostrar("Pendientes en cola: %d" % len(cola))
    filas = [list(x) for x in cola]
    if cola.esta_vacia():
        mostrar("(no hay tomas pendientes)")
        return filas, None
    mostrar("Frente (la siguiente): %s" % (cola.frente(),))
    atendida = cola.desencolar()
    mostrar("desencolar() -> atendida: %s" % (atendida,))
    mostrar("Quedan en la cola: %d" % len(cola))
    return filas, atendida


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


def demo_grafo(grafo, por_paciente):
    titulo("8) GRAFO - interacciones entre medicamentos")
    mostrar("Vertices (medicamentos): %d | Aristas (interacciones): %d" % (len(grafo.adyacencia), grafo.total_aristas()))
    inicio = "Warfarina"
    recorridos = {}
    if inicio in grafo.adyacencia:
        recorridos["bfs"] = grafo.bfs(inicio)
        recorridos["dfs"] = grafo.dfs_recursivo(inicio)
        mostrar("BFS desde %s: %s" % (inicio, " > ".join(recorridos["bfs"])))
        mostrar("DFS (recursivo) desde %s: %s" % (inicio, " > ".join(recorridos["dfs"])))
        camino = grafo.camino_mas_corto("Warfarina", "Omeprazol")
        recorridos["camino"] = camino
        mostrar("Camino mas corto Warfarina -> Omeprazol: " + (" > ".join(camino) or "no existe"))
    mostrar("")
    mostrar("Conflictos detectados por paciente:")
    alertas = []
    for pac, meds in por_paciente.items():
        for a, b, grav, desc in grafo.conflictos(meds):
            alertas.append((pac, a, b, grav, desc))
            mostrar("  [%s] %s: %s + %s - %s" % (grav.upper(), pac, a, b, desc))
    if not alertas:
        mostrar("  Ningun paciente tiene medicamentos que interactuen.")
    return recorridos, alertas


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
    if not (args.tratamientos or args.hoy or args.historial):   # ejecutado a mano desde VS Code
        tratamientos, hoy, historial = datos_ejemplo()
        origen = "datos de ejemplo"

    for fila in hoy + historial:                      # 'Estado' sin emojis
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
