# Guía de Pruebas y Funcionamiento del Sistema

Hola Karim,

Aquí tienes una guía detallada para probar las correcciones de errores y las nuevas funcionalidades que he implementado en el sistema KIOSKO_ITH.

---

## A. Verificación de Corrección de Errores

El objetivo de esta sección es asegurarse de que los problemas que reportaste (errores de compilación y de ejecución) ya no ocurran.

### 1. **Prueba de Compilación y Arranque**
   - **Objetivo:** Confirmar que el proyecto compila sin errores y la aplicación se inicia correctamente.
   - **Pasos a seguir:**
     1. Abre el proyecto en Visual Studio.
     2. Ve al menú `Build` (Compilar) y selecciona `Rebuild Solution` (Recompilar Solución).
     3. **Resultado esperado:** La compilación debe finalizar con "Rebuild All succeeded" (Recompilación correcta) y sin ningún error en la ventana de "Error List" (Lista de Errores).
     4. Inicia la aplicación (presionando F5 o el botón "Start").
     5. **Resultado esperado:** Debe aparecer la ventana de inicio de sesión sin ningún problema.

### 2. **Prueba del Historial de Inventario (Error de Columna)**
   - **Objetivo:** Verificar que el error "Invalid column name 'TOTAL_PRODUCTOS'" ha sido solucionado.
   - **Pasos a seguir:**
     1. Inicia sesión con un usuario que tenga permisos de **Administrador** o **Supervisor**.
     2. En la pantalla principal, haz clic en el botón `📥 Inventario`.
     3. En la ventana de gestión de inventario, busca la opción para ver el historial o los movimientos.
     4. **Resultado esperado:** La ventana debe cargar y mostrar el historial de entradas de inventario sin mostrar ningún error. La tabla ahora debe mostrar el **nombre del producto** y la **cantidad** de la entrada.

---

## B. Pruebas de las Nuevas Funcionalidades

Esta sección se enfoca en verificar que el nuevo módulo de reportes y los cambios en la interfaz principal funcionen como se espera.

### 1. **Prueba de la Nueva Interfaz Principal**
   - **Objetivo:** Confirmar que el diseño de la pantalla principal se ha actualizado.
   - **Pasos a seguir:**
     1. Inicia sesión con un usuario **Administrador** o **Supervisor**.
     2. **Observa el encabezado:**
        - **Resultado esperado:** Deberías ver los botones `📦 Productos`, `📥 Inventario` y `📈 Reportes` claramente visibles en la parte superior, al lado del título "Kioskito ITH". El antiguo botón de "Historial" ya no debería estar.
     3. **Observa la esquina superior derecha:**
        - **Resultado esperado:** El nombre del usuario y el botón de "Cerrar sesión" deben aparecer juntos de forma ordenada.

### 2. **Prueba del Módulo de Reportes (Funcionalidad General)**
   - **Objetivo:** Asegurarse de que el nuevo módulo de reportes se abre correctamente.
   - **Pasos a seguir:**
     1. Desde la pantalla principal, haz clic en el botón `📈 Reportes`.
     2. **Resultado esperado:** Se debe abrir una nueva ventana titulada "Módulo de Reportes" con dos pestañas: "Reporte de Ventas Detalladas" y "Corte de Caja Diario".

### 3. **Prueba del Reporte de Ventas Detalladas y Exportación a CSV**
   - **Objetivo:** Verificar la generación y exportación del reporte de ventas.
   - **Pasos a seguir:**
     1. En el módulo de reportes, asegúrate de estar en la pestaña **"Reporte de Ventas Detalladas"**.
     2. Selecciona un rango de fechas en los campos "Desde" y "Hasta" donde sepas que existen ventas registradas.
     3. Haz clic en el botón **"Generar Reporte"**.
     4. **Resultado esperado:** La tabla se debe llenar con los datos de las ventas de ese período. Deberías ver columnas como `VentaID`, `FechaVenta`, `NombreEmpleado`, `NombreProducto`, `Cantidad`, `Subtotal`, etc.
     5. Con los datos en pantalla, haz clic en el botón **"Exportar a CSV"**.
     6. Se abrirá una ventana para guardar el archivo. Elige una ubicación y un nombre, y haz clic en "Guardar".
     7. **Resultado esperado:** El sistema debe confirmar que la exportación fue exitosa. Busca el archivo CSV en tu computadora y ábrelo (con Excel, por ejemplo). Los datos deben coincidir con lo que viste en pantalla.

### 4. **Prueba del Corte de Caja Diario y Exportación a PDF**
   - **Objetivo:** Verificar la generación y exportación del corte de caja.
   - **Pasos a seguir:**
     1. Cambia a la pestaña **"Corte de Caja Diario"**.
     2. Selecciona una fecha en la que se hayan realizado ventas.
     3. Haz clic en el botón **"Generar Corte"**.
     4. **Resultados esperados:**
        - La tabla se llenará con un resumen de las ventas de ese día.
        - En la parte inferior, la etiqueta de total se actualizará mostrando el **Total del Día** y el desglose por **Efectivo** y **Tarjeta**.
     5. Con el corte generado, haz clic en el botón **"Exportar a PDF"**.
     6. Guarda el archivo en tu computadora.
     7. **Resultado esperado:** El sistema confirmará la exportación. Abre el archivo PDF. Debe contener un reporte bien formateado con el resumen de totales y la lista de ventas del día.

