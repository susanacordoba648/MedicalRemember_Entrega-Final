"""
estructuras_lineales.py
"""

from utilidades import mostrar, titulo


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

    def apilar(self, dato):                      
        nuevo = Nodo(dato)
        nuevo.siguiente = self._cima
        self._cima = nuevo
        self._tamano += 1

    def desapilar(self):                         
        if self.esta_vacia():
            raise IndexError("La pila esta vacia")
        dato = self._cima.dato
        self._cima = self._cima.siguiente
        self._tamano -= 1
        return dato

    def cima(self):                              
        if self.esta_vacia():
            raise IndexError("La pila esta vacia")
        return self._cima.dato

    def esta_vacia(self):
        return self._cima is None

    def __len__(self):
        return self._tamano

    def __iter__(self):                          
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

    def encolar(self, dato):                     
        nuevo = Nodo(dato)
        if self._final is None:
            self._frente = self._final = nuevo
        else:
            self._final.siguiente = nuevo
            self._final = nuevo
        self._tamano += 1

    def desencolar(self):                        
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
# DEMOSTRACIONES (cada funcion devuelve datos para el PDF)
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
