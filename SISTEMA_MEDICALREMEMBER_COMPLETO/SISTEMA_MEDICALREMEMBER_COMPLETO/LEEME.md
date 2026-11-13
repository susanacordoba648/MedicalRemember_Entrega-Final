# Medical Remember

**Sistema de recordatorio y seguimiento de tratamientos médicos (por favor personal docente leer hasta el final)**
Hospital Amistad Japón, Granada, Nicaragua · Integrador IV · UNAN-Managua · Ingeniería en Sistemas de la Información

**Integrantes:** Susana Jeandeska Moreno Córdoba · Mell Bryan Centeno Wade · Gessler Martin Franco Calderón · Germán Josué Tercero Pérez

---

Profesor, este documento explica **qué contiene nuestro sistema** y **cómo se ejecuta paso a paso**, para que pueda probarlo sin dificultad.

## 1. ¿Qué es y qué problema resuelve?

Medical Remember es una aplicación de escritorio (Windows Forms) que ayuda a dar seguimiento a los medicamentos de adultos mayores y pacientes crónicos. Antes, el control dependía de la memoria del paciente y de anotaciones en papel. Con el sistema, cada tratamiento y cada horario queda registrado, el sistema avisa cuando se acerca una toma y se guarda un historial con el porcentaje de cumplimiento de cada paciente.

## 2. ¿Qué tiene el sistema?

| Módulo | Qué hace |
|---|---|
| **Acceso** | Inicio de sesión, registro de usuarios y recuperación de contraseña con preguntas de seguridad. Roles: Administrador y Cuidador. |
| **Inicio** | Panel principal con indicadores del sistema y alertas de tomas próximas. |
| **Pacientes** | Registro, edición y baja de pacientes. |
| **Medicamentos** | Catálogo de medicamentos comunes (20 cargados desde Excel). |
| **Tratamientos** | Asigna un medicamento y una dosis a un paciente, con fechas e instrucciones. |
| **Horarios** | Define a qué horas se toma cada tratamiento y permite marcar la toma. |
| **Historial** | Registro de cada toma: Tomado, Tardío, Omitido o Pendiente. |
| **Reporte RDLC** | Reporte con ReportViewer: filtro por fechas, parámetros (usuario, rango y cumplimiento) y exportación a PDF, Excel y Word. |
| **Módulo Python** | Estructuras de datos en Python con los datos reales del sistema, y reporte PDF con gráficos. |
| **Servicio API** | API web (ASP.NET Core) que expone los datos en JSON y se prueba con Swagger. |

## 3. Arquitectura en capas

Cada capa es un **proyecto físico independiente** dentro de la solución `MedicalRemember.sln`:

| Proyecto | Responsabilidad |
|---|---|
| `MedicalRemember.UI` | Formularios (lo que ve el usuario). Solo llama a Negocio. |
| `MedicalRemember.Negocio` | Reglas del sistema y servicios (`PacienteServicio`, `TratamientoServicio`, `HorarioServicio`, `HistorialServicio`, `UsuarioServicio`, `Seguridad`, entre otros). |
| `MedicalRemember.Datos` | Acceso a SQL Server (consultas parametrizadas). |
| `MedicalRemember.Entidades` | Clases del dominio: `Paciente`, `Medicamento`, `Tratamiento`, `Horario`, `HistorialCumplimiento`, `Usuario`. |
| `MedicalRemember.Api` | Servicio API (proyecto aparte que lee la misma base de datos). |

Las referencias van en un solo sentido: UI → Negocio → Datos → Entidades. La interfaz nunca toca la base de datos directamente.

## 4. Base de datos (SQL Server)

- **Tablas:** Pacientes, Cuidadores, Medicamentos, Tratamientos, Horarios, HistorialCumplimiento, Usuarios y AuditoriaUsuarios (más la bitácora de respaldos).
- **4 triggers:**
  - `TR_Pacientes_NoEliminar`: un DELETE se convierte en baja lógica.
  - `TR_Historial_MarcarTardio`: una toma con más de 30 minutos de retraso se marca como Tardío.
  - `TR_Usuarios_AuditarCambioClave`: registra cada cambio de contraseña.
  - `TR_Tratamientos_ValidarFechas`: rechaza una fecha de fin anterior a la de inicio.
- **Seguridad:** contraseñas y respuestas de seguridad con hash SHA-256 (nunca en texto plano) y consultas parametrizadas contra inyección SQL.
- **Respaldos:** `Database\Respaldo_Restauracion_Jobs.sql` crea los jobs de SQL Server Agent: respaldo completo diario, diferencial cada 6 horas y de log cada hora.

## 5. Requisitos

- Windows con **Visual Studio 2022** (carga de trabajo "Desarrollo de escritorio con .NET") y .NET Framework 4.7.2.
- **SQL Server** (Developer, Express o similar) y SQL Server Management Studio.
- **Python 3** (marcando "Add Python to PATH"). La librería `reportlab` se instala con un botón del propio sistema.
- Para la API: **.NET 8 SDK** (incluido en Visual Studio 2022 reciente) e internet la primera vez.

## 6. Cómo ejecutar el sistema (paso a paso)

### Paso 1. Crear la base de datos
En SQL Server Management Studio, abrir y ejecutar **en este orden**:
1. `MedicalRememberDB.sql`: crea la base `MedicalRememberDB`, las tablas, los triggers y los usuarios de prueba.
2. `Database\ImportarMedicamentos.sql`: carga el catálogo de medicamentos.
3. `Database\DatosPrueba_10Pacientes.sql`: carga **10 pacientes, 10 tratamientos y 12 horarios** (entre las 7:00 y las 13:00).
4. *(Opcional)* `Database\DatosPrueba_Historial_Opcional.sql`: historial de 7 días, para que los reportes y gráficos tengan datos.

### Paso 2. Revisar la conexión
La cadena está en `MedicalRemember.UI\App.config`:
`Server=.;Database=MedicalRememberDB;Integrated Security=True;`


### Paso 3. Abrir y compilar
1. Abrir `MedicalRemember.sln` con Visual Studio 2022. Deben aparecer los 4 proyectos.
2. Si Visual Studio indica que falta ReportViewer, instalar en `MedicalRemember.UI` el paquete NuGet `Microsoft.ReportingServices.ReportViewerControl.Winforms` versión **150.1652.0**.
3. Menú **Compilar → Recompilar solución**, y luego **F5**.

### Paso 4. Iniciar sesión y recorrer el sistema

| Usuario | Contraseña | Rol |
|---|---|---|
| `admin` | `1234` | Administrador |
| `carlos` | `carlos123` | Cuidador |

Recorrido sugerido: **Inicio → Pacientes → Tratamientos → Horarios** (marcar una toma) **→ Historial → Reporte RDLC**.

### Paso 5. Reporte RDLC
Menú lateral **Reporte RDLC**: elegir el rango de fechas y presionar **Ver reporte**. Desde la barra del visor se exporta a PDF, Excel o Word.

### Paso 6. Módulo Python (estructuras de datos)
Menú lateral **Módulo Python**:
1. **Instalar reportlab (pip)**: solo la primera vez en cada computadora.
2. **Ejecutar módulo Python**: el sistema exporta los datos reales y Python los procesa. En pantalla se ven 8 demostraciones.
3. **Abrir reporte PDF**: abre el reporte generado (se guarda en `Documentos\MedicalRemember`). Incluye **3 gráficos** automáticos al final: estado de las tomas, cumplimiento por paciente y tomas de hoy por hora.

El código está en `MedicalRemember.UI\Python\estructuras_medicas.py`. Si el reporte no muestra los cambios, hay que recompilar: el sistema ejecuta la copia del script que se actualiza al compilar.

### Paso 7. Servicio API
1. Revisar `MedicalRemember.Api\appsettings.json` (mismo servidor del Paso 2).
2. Abrir `MedicalRemember.Api\MedicalRemember.Api.csproj` en Visual Studio y presionar **F5**.
3. Se abre **Swagger** en `http://localhost:5080/swagger` para probar las rutas. Más detalle en `LEEME_API_Y_DATOS.md`.

## 7. Estructuras de datos del módulo Python

| Estructura | Para qué se usa en el sistema |
|---|---|
| Lista enlazada | Medicamentos de un paciente |
| Pila (LIFO) | Tomas confirmadas; permite deshacer la última |
| Cola (FIFO) | Tomas pendientes de hoy, ordenadas por hora |
| Tupla | Registro inmutable de una toma |
| Diccionario | Conteos por paciente y por estado |
| Conjunto | Medicamentos únicos, intersección y diferencia entre pacientes |
| Función | `calcular_cumplimiento()`: tomados, total y porcentaje |
| Recursividad | `dfs_recursivo()`: recorrido en profundidad del grafo |
| **Grafo** (no lineal) | Interacciones entre medicamentos, con gravedad Alta, Media o Baja |

Las interacciones del grafo son un ejemplo con fines académicos y no reemplazan el criterio médico.

## 8. Servicio API (resumen)

Hecho con **C#, ASP.NET Core 8, Entity Framework Core y Swagger**, con la estructura Models, `AppDbContext` y Controllers vista en clase.

| Ruta | Qué hace |
|---|---|
| `GET api/resumen` | Cada paciente con sus tratamientos y horarios |
| `GET api/horarios?desde=07:00&hasta=13:00` | Horarios dentro de un rango de horas |
| `GET api/horarios/agenda-hoy` | Tomas de hoy con su estado |
| `api/orm/pacientes` y `api/orm/medicamentos` | CRUD completo (DELETE es baja lógica) |
| `GET/POST api/tratamientos` | Consulta y asigna tratamientos |
| `GET api/historial` y `POST api/historial/{id}/tomar` | Consulta y registra tomas |
| `GET api/reportes/cumplimiento` | Reporte de cumplimiento por paciente en JSON |

## 9. Dónde está cada requisito

| Requisito | Dónde se cumple |
|---|---|
| Estructuras de datos en Python, con función, pila, cola, lista y estructura no lineal | `MedicalRemember.UI\Python\estructuras_medicas.py`, botón **Módulo Python** |
| Reporte con librería y botón dentro del proyecto integrador | Reporte PDF del módulo Python y botón del menú lateral |
| Reporte RDLC con ReportViewer | `MedicalRemember.UI\Reportes\RptHistorial.rdlc` y `Forms\FrmReporteHistorial.cs`, botón **Reporte RDLC** |
| Capa de servicios (clases) | `MedicalRemember.Negocio` |
| Servicio de API | `MedicalRemember.Api` |
| Datos de prueba | `Database\DatosPrueba_10Pacientes.sql` |

## 10. Problemas comunes

| Síntoma | Solución |

| Solo aparecen 2 de los 4 proyectos | Abrir el archivo `.sln` con Visual Studio 2022 (no la carpeta), con el zip ya extraído. |
| No inicia sesión o no conecta | Revisar que la base exista y el `Server=` del `App.config`. |
| Error de ReportViewer al compilar | Instalar el paquete NuGet del Paso 3 y recompilar. |
| El reporte PDF de Python no se genera | Presionar **Instalar reportlab (pip)** y comprobar que Python esté instalado. |
| Error al compilar por archivos bloqueados | En PowerShell: `Get-ChildItem "carpeta del proyecto" -Recurse \| Unblock-File` |

---

*Medical Remember · Noviembre de 2026*

## Un agradecimiento muy especial a ustedes profesores por su paciencia infinita, sus valiosas enseñanzas y por ponernos a prueba    constantemente para sacar lo mejor de nosotros (aunque en el proceso nos dieran uno que otro dolor de cabeza con sus trabajos).

**Por su guía: Gracias por no conformarse con lo básico y desafiarnos siempre a pensar y programar más allá de la teoría.**
**​Por la presión: Aunque los cortes evaluativos, los bugs de última hora y las fechas límite nos mantuvieron al borde del colapso, hoy entendemos que cada obstáculo fue clave para prepararnos ante los retos reales.**
​**Por su vocación: Su dedicación deja una huella imborrable en nuestra formación.**

## Gracias por tanto y perdón por tan poco.
