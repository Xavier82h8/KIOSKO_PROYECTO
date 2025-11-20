-- --------------------------------------------------
-- 📜 KIOSKO_ITH - Base de Datos
-- --------------------------------------------------
-- Este script contiene la estructura completa de la
-- base de datos para el proyecto KIOSKO_ITH.
-- --------------------------------------------------

-- --------------------------------------------------
-- 1. Tabla: PRODUCTO
-- Almacena la información de los productos.
-- --------------------------------------------------
CREATE TABLE PRODUCTO (
    ID_PRODUCTO INT PRIMARY KEY IDENTITY(1,1),
    NOMBRE VARCHAR(100) NOT NULL,
    CATEGORIA VARCHAR(50),
    PRECIO DECIMAL(10,2) NOT NULL,
    CANTIDAD_DISPONIBLE INT NOT NULL,
    FECHA_CADUCIDAD DATE NULL
);

-- --------------------------------------------------
-- 2. Tabla: VENTA
-- Registra cada transacción de venta.
-- --------------------------------------------------
CREATE TABLE VENTA (
    ID_VENTA INT PRIMARY KEY IDENTITY(1,1),
    FECHA DATE NOT NULL,
    HORA TIME NOT NULL,
    TOTAL DECIMAL(10,2) NOT NULL,
    METODO_PAGO VARCHAR(50) NOT NULL,
    ID_EMPLEADO INT NOT NULL,
    MontoEfectivo DECIMAL(10,2) NULL,
    MontoTarjeta DECIMAL(10,2) NULL,
    Cambio DECIMAL(10,2) NULL
);

-- --------------------------------------------------
-- 3. Tabla: DETALLE_VENTA
-- Almacena los productos específicos de cada venta.
-- --------------------------------------------------
CREATE TABLE DETALLE_VENTA (
    ID_DETALLE INT PRIMARY KEY IDENTITY(1,1),
    ID_VENTA INT NOT NULL,
    ID_PRODUCTO INT NOT NULL,
    CANTIDAD INT NOT NULL,
    PRECIO_UNITARIO DECIMAL(10,2) NOT NULL,
    SUBTOTAL DECIMAL(10,2) NOT NULL,
    FOREIGN KEY (ID_VENTA) REFERENCES VENTA(ID_VENTA),
    FOREIGN KEY (ID_PRODUCTO) REFERENCES PRODUCTO(ID_PRODUCTO)
);

-- --------------------------------------------------
-- 4. Tabla: PAGO
-- Registra los pagos asociados a cada venta.
-- --------------------------------------------------
CREATE TABLE PAGO (
    ID_PAGO INT PRIMARY KEY IDENTITY(1,1),
    FECHA_PAGO DATE NOT NULL,
    MONTO DECIMAL(10,2) NOT NULL,
    TIPO_PAGO VARCHAR(50) NOT NULL,
    ID_VENTA INT NOT NULL,
    FOREIGN KEY (ID_VENTA) REFERENCES VENTA(ID_VENTA)
);

-- --------------------------------------------------
-- 5. Tabla: INVENTARIO
-- Mantiene un registro del inventario.
-- --------------------------------------------------
CREATE TABLE INVENTARIO (
    ID_INVENTARIO INT PRIMARY KEY IDENTITY(1,1),
    FECHA_REGISTRO DATE NOT NULL,
    TOTAL_PRODUCTOS INT NOT NULL,
    OBSERVACIONES VARCHAR(255),
    PROVEEDOR VARCHAR(100)
);

-- --------------------------------------------------
-- 6. Tabla: Reportes
-- Almacena los reportes generados.
-- --------------------------------------------------
CREATE TABLE Reportes (
    IdReporte INT PRIMARY KEY IDENTITY(1,1),
    FechaGeneracion DATETIME NOT NULL DEFAULT GETDATE(),
    FechaInicio DATE NOT NULL,
    FechaFin DATE NOT NULL,
    TotalVentas DECIMAL(10,2) NOT NULL,
    GeneradoPorEmpleadoId INT NOT NULL
);
