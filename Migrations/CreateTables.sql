-- Eliminar tablas si existen
DROP TABLE IF EXISTS venta_items CASCADE;
DROP TABLE IF EXISTS ventas CASCADE;
DROP TABLE IF EXISTS combo_detalles CASCADE;
DROP TABLE IF EXISTS combos CASCADE;
DROP TABLE IF EXISTS productos CASCADE;

-- Crear tabla productos
CREATE TABLE productos (
    producto_id SERIAL PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL,
    cantidad_libras DECIMAL(18,2) NOT NULL,
    precio_por_libra DECIMAL(18,2) NOT NULL,
    tipo_empaque VARCHAR(50),
    esta_activo BOOLEAN NOT NULL DEFAULT true,
    ultima_actualizacion TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Crear tabla combos
CREATE TABLE combos (
    combo_id SERIAL PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL,
    descripcion VARCHAR(500) NOT NULL,
    precio DECIMAL(18,2) NOT NULL,
    esta_activo BOOLEAN NOT NULL DEFAULT true,
    ultima_actualizacion TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Crear tabla combo_detalles
CREATE TABLE combo_detalles (
    combo_detalle_id SERIAL PRIMARY KEY,
    combo_id INTEGER NOT NULL REFERENCES combos(combo_id) ON DELETE CASCADE,
    producto_id INTEGER NOT NULL REFERENCES productos(producto_id),
    cantidad_libras DECIMAL(18,2) NOT NULL,
    UNIQUE(combo_id, producto_id)
);

-- Crear tabla ventas
CREATE TABLE ventas (
    venta_id SERIAL PRIMARY KEY,
    cliente VARCHAR(100) NOT NULL,
    observaciones VARCHAR(500),
    fecha_venta TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    monto_total DECIMAL(18,2) NOT NULL
);

-- Crear tabla venta_items
CREATE TABLE venta_items (
    venta_item_id SERIAL PRIMARY KEY,
    venta_id INTEGER NOT NULL REFERENCES ventas(venta_id) ON DELETE CASCADE,
    tipo_item VARCHAR(20) NOT NULL CHECK (tipo_item IN ('Producto', 'Combo')),
    item_id INTEGER NOT NULL,
    cantidad INTEGER NOT NULL,
    precio_unitario DECIMAL(18,2) NOT NULL
);

-- Crear índices para mejorar el rendimiento
CREATE INDEX idx_productos_nombre ON productos(nombre);
CREATE INDEX idx_productos_activo ON productos(esta_activo);
CREATE INDEX idx_combos_nombre ON combos(nombre);
CREATE INDEX idx_combos_activo ON combos(esta_activo);
CREATE INDEX idx_ventas_fecha ON ventas(fecha_venta);
CREATE INDEX idx_ventas_cliente ON ventas(cliente);
CREATE INDEX idx_venta_items_tipo ON venta_items(tipo_item, item_id);

-- Insertar algunos productos de ejemplo
INSERT INTO productos (nombre, cantidad_libras, precio_por_libra, tipo_empaque, esta_activo)
VALUES 
    ('Camarón Jumbo', 10.0, 25.99, 'Caja 5 libras', true),
    ('Camarón Mini', 1.0, 1.99, 'Caja 1 libra', true),
    ('Tiburón', 100.0, 1.99, 'Caja 100 libra', true),
    ('Kurage', 100.0, 1.99, 'Caja 100 libra', true); 