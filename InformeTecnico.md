# Informe Técnico: Sistema de Punto de Venta "Kioskito ITH"

## 1. Introducción

El presente documento describe la arquitectura y el estado de desarrollo del sistema "Kioskito ITH", una aplicación de escritorio diseñada para la gestión de un punto de venta (POS). El objetivo principal del sistema es facilitar las operaciones diarias de un kiosko, incluyendo la gestión de inventario, el procesamiento de ventas y la generación de reportes.

El sistema está desarrollado en C# utilizando Windows Forms y se conecta a una base de datos SQL Server para la persistencia de los datos.

## 2. Arquitectura del Sistema

El proyecto sigue una arquitectura de tres capas, lo que permite una clara separación de responsabilidades, facilita el mantenimiento y promueve la escalabilidad.

- **Capa de Presentación (UI):**
  - **Tecnología:** Windows Forms (.NET Framework).
  - **Descripción:** Es la interfaz con la que interactúa el usuario. Está compuesta por un conjunto de formularios diseñados para cada una de las funciones del sistema (login, venta principal, gestión de productos, etc.).
  - **Archivos Relevantes:** `FormPrincipalPOS.cs`, `FormLogin.cs`, `FormGestionProductos.cs`.

- **Capa de Lógica de Negocio (BLL - Business Logic Layer):**
  - **Descripción:** Contiene las reglas de negocio y la lógica de la aplicación. Actúa como intermediario entre la capa de presentación y la capa de acceso a datos.
  - **Archivos Relevantes:** `BLL/ProductoBLL.cs`, `BLL/VentaBLL.cs`, `BLL/EmpleadoBLL.cs`.

- **Capa de Acceso a Datos (DAL - Data Access Layer):**
  - **Descripción:** Se encarga de la comunicación directa con la base de datos. Abstrae las operaciones de inserción, actualización, eliminación y selección de datos, utilizando consultas SQL parametrizadas para prevenir inyección de SQL.
  - **Archivos Relevantes:** `Datos/ProductoDAL.cs`, `Datos/VentaDAL.cs`, `Datos/Conexion.cs`.

## 3. Base de Datos

- **SGBD:** Microsoft SQL Server.
- **Conexión:** La cadena de conexión se gestiona a través del archivo `App.config`. El sistema incluye una cadena de conexión de respaldo (fallback) en `Datos/Conexion.cs` para entornos de desarrollo.
- **Esquema:** El sistema cuenta con un mecanismo (`EnsureDatabaseSchema` en `Conexion.cs`) que verifica la existencia de las tablas necesarias al iniciar la aplicación y las crea si no existen, lo que facilita el despliegue inicial.

## 4. Funcionalidad Implementada (Avance 50%)

A la fecha, el sistema cuenta con las siguientes funcionalidades clave:

- **Autenticación de Usuarios:** Un formulario de login (`FormLogin.cs`) valida las credenciales de los empleados contra la base de datos.
- **Roles y Permisos:** El sistema reconoce diferentes roles de usuario (Administrador, Gerente, Cajero) y ajusta la visibilidad de ciertas funcionalidades según el rol del empleado autenticado.
- **Punto de Venta (POS):**
  - El formulario principal (`FormPrincipalPOS.cs`) permite visualizar los productos disponibles.
  - Funcionalidad de búsqueda y filtrado de productos por nombre y categoría.
  - Carrito de compras dinámico que permite agregar productos, modificar cantidades y eliminar ítems.
  - Cálculo en tiempo real del total de la venta.
  - Procesamiento de ventas con diferentes métodos de pago (Efectivo/Tarjeta).
- **Gestión de Productos (CRUD):**
  - El módulo `FormGestionProductos.cs` está diseñado para realizar operaciones de **C**rear, **L**eer, **A**ctualizar y **E**liminar (CRUD) sobre los productos.
  - **Inserción:** Se pueden agregar nuevos productos al sistema.
  - **Modificación:** Es posible editar la información de los productos existentes.
  - **Borrado:** Se pueden eliminar productos del inventario.
  - **Lectura:** Los productos se muestran en una tabla para su gestión.

## 5. Próximos Pasos y Mejoras

- **Módulo de Reportes:** Finalizar la implementación de la generación y visualización de reportes de ventas.
- **Gestión de Inventario:** Mejorar el formulario de inventario para permitir ajustes de stock manuales.
- **Refactorización y Robustez:**
  - Mejorar el manejo de excepciones en toda la aplicación.
  - Refactorizar el código de la interfaz de usuario para una mayor mantenibilidad (por ejemplo, utilizando componentes de usuario).
  - Implementar logging para registrar eventos importantes y errores.
