-- Script para crear la tabla de Plan Contable (PCGE) en PostgreSQL
-- Este script define la estructura y agrega algunos datos iniciales de ejemplo basados en el PCGE Peruano

-- 1. Crear tabla si no existe
CREATE TABLE IF NOT EXISTS plan_contable (
    id SERIAL PRIMARY KEY,
    empresa_id INT NOT NULL REFERENCES empresas(id),
    elemento VARCHAR(2),
    cta VARCHAR(5),
    cuenta VARCHAR(20) NOT NULL, 
    descripcion VARCHAR(200) NOT NULL,
    nivel INT NOT NULL,
    clase_cuenta VARCHAR(5),
    tipo_anexo VARCHAR(10),
    cuenta_monetaria BOOLEAN DEFAULT FALSE,
    ajuste_dif_cambio BOOLEAN DEFAULT FALSE,
    requiere_centro_costo BOOLEAN DEFAULT FALSE,
    codigo_eeff_estand VARCHAR(20),
    codigo_eeff_trib VARCHAR(20),
    clasificacion_bien_serv VARCHAR(20),
    
    cargo_1 VARCHAR(20),
    abono_1 VARCHAR(20),
    porcentaje_1 DECIMAL(5,2) DEFAULT 0,
    cargo_2 VARCHAR(20),
    abono_2 VARCHAR(20),
    porcentaje_2 DECIMAL(5,2) DEFAULT 0,
    cargo_3 VARCHAR(20),
    abono_3 VARCHAR(20),
    porcentaje_3 DECIMAL(5,2) DEFAULT 0,
    cuenta_cierre VARCHAR(20),
    
    activo BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP,
    UNIQUE(empresa_id, cuenta)
);

-- 2. Insertar datos de ejemplo del PCGE 
-- IMPORTANTE: Reemplazar el ID '1' en empresa_id por el ID de la empresa real

INSERT INTO plan_contable (
    empresa_id, elemento, cta, cuenta, descripcion, nivel, clase_cuenta, 
    cuenta_monetaria, requiere_centro_costo, activo
) VALUES 
-- ELEMENTO 1 - ACTIVO DISPONIBLE Y EXIGIBLE
(1, '01', '10', '10', 'EFECTIVO Y EQUIVALENTES DE EFECTIVO', 1, '01', FALSE, FALSE, TRUE),
(1, '01', '101', '101', 'Caja', 2, '01', TRUE, FALSE, TRUE),
(1, '01', '101', '1011', 'Caja M/N', 3, '01', TRUE, FALSE, TRUE),
(1, '01', '101', '1012', 'Caja M/E', 3, '01', TRUE, FALSE, TRUE),
(1, '01', '104', '104', 'Cuentas corrientes en instituciones financieras', 2, '01', TRUE, FALSE, TRUE),
(1, '01', '104', '1041', 'Cuentas corrientes operativas', 3, '01', TRUE, FALSE, TRUE),

(1, '01', '12', '12', 'CUENTAS POR COBRAR COMERCIALES – TERCEROS', 1, '01', FALSE, FALSE, TRUE),
(1, '01', '121', '121', 'Facturas, boletas y otros comprobantes por cobrar', 2, '01', TRUE, FALSE, TRUE),
(1, '01', '121', '1212', 'Emitidas en cartera', 3, '01', TRUE, FALSE, TRUE),
(1, '01', '121', '12121', 'Emitidas en cartera M/N', 4, '01', TRUE, FALSE, TRUE),
(1, '01', '121', '12122', 'Emitidas en cartera M/E', 4, '01', TRUE, FALSE, TRUE),

-- ELEMENTO 2 - ACTIVO REALIZABLE
(1, '02', '20', '20', 'MERCADERÍAS', 1, '02', FALSE, FALSE, TRUE),
(1, '02', '201', '201', 'Mercaderías', 2, '02', FALSE, FALSE, TRUE),
(1, '02', '201', '2011', 'Mercaderías', 3, '02', FALSE, FALSE, TRUE),
(1, '02', '201', '20111', 'Costo', 4, '02', FALSE, FALSE, TRUE),

-- ELEMENTO 4 - PASIVO
(1, '04', '40', '40', 'TRIBUTOS, CONTRAPRESTACIONES Y APORTES', 1, '04', FALSE, FALSE, TRUE),
(1, '04', '401', '401', 'Gobierno Central', 2, '04', FALSE, FALSE, TRUE),
(1, '04', '401', '4011', 'Impuesto general a las ventas', 3, '04', FALSE, FALSE, TRUE),
(1, '04', '401', '40111', 'IGV - Cuenta propia', 4, '04', FALSE, FALSE, TRUE),

(1, '04', '42', '42', 'CUENTAS POR PAGAR COMERCIALES – TERCEROS', 1, '04', FALSE, FALSE, TRUE),
(1, '04', '421', '421', 'Facturas, boletas y otros comprobantes por pagar', 2, '04', TRUE, FALSE, TRUE),
(1, '04', '421', '4212', 'Emitidas', 3, '04', TRUE, FALSE, TRUE),
(1, '04', '421', '42121', 'Emitidas M/N', 4, '04', TRUE, FALSE, TRUE),
(1, '04', '421', '42122', 'Emitidas M/E', 4, '04', TRUE, FALSE, TRUE),

-- ELEMENTO 6 - GASTOS POR NATURALEZA (CON DESTINOS)
(1, '06', '60', '60', 'COMPRAS', 1, '06', FALSE, FALSE, TRUE),
(1, '06', '601', '601', 'Mercaderías', 2, '06', FALSE, FALSE, TRUE),
(1, '06', '601', '6011', 'Mercaderías', 3, '06', FALSE, FALSE, TRUE),

(1, '06', '61', '61', 'VARIACIÓN DE EXISTENCIAS', 1, '06', FALSE, FALSE, TRUE),
(1, '06', '611', '611', 'Mercaderías', 2, '06', FALSE, FALSE, TRUE),
(1, '06', '611', '6111', 'Mercaderías', 3, '06', FALSE, FALSE, TRUE),

(1, '06', '63', '63', 'GASTOS DE SERVICIOS PRESTADOS POR TERCEROS', 1, '06', FALSE, TRUE, TRUE),
(1, '06', '631', '631', 'Transporte, correos y gastos de viaje', 2, '06', FALSE, TRUE, TRUE),

-- ELEMENTO 7 - INGRESOS POR NATURALEZA
(1, '07', '70', '70', 'VENTAS', 1, '07', FALSE, FALSE, TRUE),
(1, '07', '701', '701', 'Mercaderías', 2, '07', FALSE, FALSE, TRUE),
(1, '07', '701', '7011', 'Mercaderías - Terceros', 3, '07', FALSE, FALSE, TRUE),

-- ELEMENTO 9 - CUENTAS ANALÍTICAS DE EXPLOTACIÓN (DESTINOS)
(1, '09', '94', '94', 'GASTOS ADMINISTRATIVOS', 1, '09', FALSE, FALSE, TRUE),
(1, '09', '95', '95', 'GASTOS DE VENTAS', 1, '09', FALSE, FALSE, TRUE)
ON CONFLICT (empresa_id, cuenta) DO NOTHING;

-- 3. Actualizar configuraciones de destinos (El asiento de destino automático)
-- Para compras de mercadería (Cargo a 20111, Abono a 6111)
UPDATE plan_contable 
SET cargo_1 = '20111', abono_1 = '6111', porcentaje_1 = 100.00
WHERE cuenta = '6011' AND empresa_id = 1;

-- Para gastos de servicios (Asumiendo destino 50% admin, 50% ventas para el ejemplo)
UPDATE plan_contable 
SET cargo_1 = '94', abono_1 = '791', porcentaje_1 = 50.00,
    cargo_2 = '95', abono_2 = '791', porcentaje_2 = 50.00
WHERE cuenta = '631' AND empresa_id = 1;

-- Asegurar que la cuenta 79 exista para el destino de gastos
INSERT INTO plan_contable (
    empresa_id, elemento, cta, cuenta, descripcion, nivel, clase_cuenta, activo
) VALUES 
(1, '07', '79', '79', 'CARGAS IMPUTABLES A CUENTAS DE COSTOS Y GASTOS', 1, '07', TRUE),
(1, '07', '791', '791', 'Cargas imputables a cuentas de costos y gastos', 2, '07', TRUE)
ON CONFLICT (empresa_id, cuenta) DO NOTHING;