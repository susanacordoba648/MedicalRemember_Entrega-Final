"""
grafo.py - Estructura NO lineal: grafo de interacciones entre medicamentos

"""

from estructuras_lineales import Cola
from utilidades import mostrar, titulo


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

    # ---- construccion --------------
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

    # ---- recorrido en ANCHURA (BFS) usando nuestra Cola ----
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

    # ---- recorrido en PROFUNDIDAD (DFS) con RECURSIVIDAD ----
    def dfs_recursivo(self, actual, visitados=None, orden=None):
        if visitados is None:                  # primera llamada
            visitados, orden = set(), []
        visitados.add(actual)
        orden.append(actual)
        for v in self.vecinos(actual):
            if v not in visitados:             # llamada recursiva
                self.dfs_recursivo(v, visitados, orden)
        return orden

    # ---- camino mas corto (en numero de saltos) con BFS ----
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
# DEMOSTRACION
# ======================================================================
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
