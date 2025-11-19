# Informe Técnico: KIOSKO_ITH

Este documento sirve como una guía técnica detallada del proyecto KIOSKO_ITH, un sistema de Punto de Venta (POS) desarrollado en C# con Windows Forms.

## Sugerencias de Organización y Mejora

A continuación, se presentan una serie de sugerencias para mejorar la estructura, mantenibilidad y escalabilidad del proyecto.

### 1. Adoptar un ORM (Object-Relational Mapper)
- **Sugerencia:** Reemplazar el acceso a datos manual con ADO.NET por un ORM moderno como **Entity Framework Core**.
- **Beneficios:** Simplifica el código, mejora la mantenibilidad y aumenta la seguridad al prevenir inyección SQL.

### 2. Implementar Inyección de Dependencias (DI)
- **Sugerencia:** Utilizar un contenedor de DI para gestionar la creación y el ciclo de vida de los objetos.
- **Beneficios:** Desacopla los componentes, facilita las pruebas unitarias y aumenta la flexibilidad del sistema.

### 3. Crear un Proyecto de Pruebas Unitarias
- **Sugerencia:** Añadir un proyecto de pruebas (utilizando MSTest, xUnit o NUnit) para validar la lógica de negocio de forma aislada.
- **Beneficios:** Aumenta la fiabilidad del código y ayuda a prevenir regresiones.

### 4. Separar las Capas en Proyectos Distintos
- **Sugerencia:** Migrar las carpetas `Modelos`, `Datos` y `BLL` a proyectos de Biblioteca de Clases (`.csproj`) separados.
- **Beneficios:** Refuerza la separación de responsabilidades y promueve la reutilización de código.

### 5. Centralizar la Configuración
- **Sugerencia:** Mover la cadena de conexión de la clase `Conexion` a un archivo `App.config`.
- **Beneficios:** Facilita la gestión de la configuración para diferentes entornos sin necesidad de recompilar.

### 6. Utilizar NuGet para la Gestión de Dependencias
- **Sugerencia:** Administrar librerías como `iTextSharp` a través del gestor de paquetes NuGet.
- **Beneficios:** Simplifica la gestión de versiones y la configuración de nuevos entornos de desarrollo.

---

## Arquitectura del Sistema

El proyecto sigue una **arquitectura de 3 capas** clásica, diseñada para separar las responsabilidades y mejorar la organización del código.

1.  **Capa de Presentación (UI):**
    - Compuesta por los formularios de Windows Forms (`Form*.cs`).
    - Es responsable de toda la interacción con el usuario y de mostrar los datos.
    - No contiene lógica de negocio; su función es invocar a la capa BLL y presentar los resultados.

2.  **Capa de Lógica de Negocio (BLL - Business Logic Layer):**
    - Ubicada en la carpeta `/BLL`.
    - Contiene las reglas de negocio, validaciones y orquestación de operaciones.
    - Actúa como intermediario entre la capa de presentación y la capa de acceso a datos.

3.  **Capa de Acceso a Datos (DAL - Data Access Layer):**
    - Ubicada en la carpeta `/Datos`.
    - Es la única capa que interactúa directamente con la base de datos.
    - Se encarga de ejecutar las consultas SQL para crear, leer, actualizar y eliminar registros (CRUD).

---

## Desglose de Componentes

### Capa de Modelos (`/Modelos`)
Contiene las clases POCO (Plain Old CLR Object) que representan las entidades de la base de datos.

- **`Producto.cs`**: Representa un artículo a la venta.
- **`Venta.cs`**: Representa la transacción principal de una venta.
- **`DetalleVenta.cs`**: Representa una línea de producto dentro de una venta.
- **`Empleado.cs`**: Modela a un usuario del sistema.
- **`Inventario.cs`**: Representa una entrada de stock.
- **`Reporte.cs`**: Modela los metadatos de un reporte de ventas guardado.

### Capa de Acceso a Datos (`/Datos`)
Contiene la lógica para interactuar con la base de datos SQL Server.

- **`Conexion.cs`**:
  - **Función:** Gestiona y proporciona la cadena de conexión a la base de datos.
  - **Método Clave:** `ObtenerConexion()`: Retorna un objeto `SqlConnection` listo para ser usado.

- **`ProductoDAL.cs`**:
  - **Función:** Gestiona las operaciones CRUD para la tabla `PRODUCTO`.
  - **Métodos Clave:** `ObtenerProductos()`, `AgregarProducto()`, `ActualizarProducto()`, `EliminarProducto()`.

- **`VentaDAL.cs`**:
  - **Función:** Gestiona las operaciones relacionadas con las ventas.
  - **Método Clave:** `CrearVenta(Venta venta)`: Orquesta una transacción SQL para insertar la venta y sus detalles, y actualizar el stock de los productos.

- **`ReporteDAL.cs`**:
  - **Función:** Gestiona la lectura de reportes guardados.
  - **Método Clave:** `ObtenerTodosLosReportes()`: Lee y devuelve todos los registros de la tabla `Reportes`.

### Capa de Lógica de Negocio (`/BLL`)
Contiene la lógica de negocio que consume la capa de datos.

- **`ProductoBLL.cs`**:
  - **Función:** Aplica reglas de negocio a los productos (ej. filtrado).
  - **Método Clave:** `FiltrarProductos()`: Filtra la lista de productos por nombre y/o categoría.

- **`VentaBLL.cs`**:
  - **Función:** Orquesta las operaciones de venta y la generación de documentos.
  - **Métodos Clave:**
    - `RegistrarVenta()`: Valida y procesa una nueva venta.
    - `ExportarTicketPDF()`: Utiliza `iTextSharp` para generar un recibo de venta en PDF.

- **`ReporteBLL.cs`**:
  - **Función:** Gestiona la lógica de los reportes.
  - **Métodos Clave:**
    - `ObtenerTodosLosReportes()`: Obtiene la lista de reportes guardados.
    - `ExportarReportesCSV()`: Convierte la lista de reportes a un archivo CSV.

### Capa de Presentación (Formularios)
Los formularios principales que gestionan la interacción con el usuario.

- **`FormLogin.cs`**:
  - **Función:** Gestiona la autenticación del usuario.
  - **Flujo:** Valida las credenciales contra la base de datos a través de `EmpleadoBLL` y, si son correctas, abre el formulario principal.

- **`FormPrincipalPOS.cs`**:
  - **Función:** Punto de venta principal para registrar ventas.
  - **Flujo de Venta:** Permite al usuario seleccionar productos, añadirlos a un carrito, procesar el pago y registrar la venta en el sistema, invocando a `VentaBLL`.

- **`FormVerReportes.cs`**:
  - **Función:** Muestra los reportes de ventas que han sido guardados previamente en la base de datos.
  - **Flujo:** Al cargarse, invoca a `ReporteBLL.ObtenerTodosLosReportes()` y muestra los resultados en una tabla. Permite exportar esta lista a CSV.

---

## Dependencias Externas

- **`iTextSharp`**: Librería utilizada para la creación de documentos PDF, específicamente para los tickets de venta.
