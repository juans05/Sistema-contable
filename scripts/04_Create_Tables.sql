-- Script para la creación de las tablas de Plan Contable (PCGE), Tipos de Comprobante (SUNAT) y Anexos (Socios de Negocio)
-- Creado para la base de datos de Sistema Contable (PostgreSQL)

-- =====================================================================================
-- 1. PLAN CONTABLE GENERAL EMPRESARIAL (PCGE)
-- =====================================================================================
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

-- =====================================================================================
-- 2. TIPOS DE COMPROBANTE / DOCUMENTO (Tabla 10 SUNAT)
-- =====================================================================================
CREATE TABLE IF NOT EXISTS tipos_comprobante (
    id SERIAL PRIMARY KEY,
    codigo VARCHAR(2) NOT NULL UNIQUE, 
    descripcion VARCHAR(100) NOT NULL, 
    codigo_sunat VARCHAR(2), 
    tipo VARCHAR(1), 
    activo BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- =====================================================================================
-- 3. MAESTRO DE ANEXOS / SOCIOS DE NEGOCIO (Clientes, Proveedores, Empleados)
-- =====================================================================================
CREATE TABLE IF NOT EXISTS anexos (
    id SERIAL PRIMARY KEY,
    empresa_id INT NOT NULL REFERENCES empresas(id),
    tipo_anexo VARCHAR(2) NOT NULL, 
    codigo_anexo VARCHAR(20) NOT NULL, 
    tipo_documento_id VARCHAR(1) NOT NULL, -- Tabla 2 SUNAT (1: DNI, 6: RUC, etc)
    numero_documento VARCHAR(15) NOT NULL,
    tipo_persona VARCHAR(2) NOT NULL,      -- (01: Natural, 02: Jurídica)
    razon_social VARCHAR(200),
    nombres VARCHAR(100),
    apellido_paterno VARCHAR(100),
    apellido_materno VARCHAR(100),
    sexo VARCHAR(1), 
    nacionalidad VARCHAR(4),
    direccion VARCHAR(250),
    correo VARCHAR(100),
    activo BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP,
    UNIQUE(empresa_id, tipo_anexo, codigo_anexo)
);

-- =====================================================================================
-- Para el resto de entidades ya creadas (Marca y Categoría)
-- Estos deben existir o se asume su creación via migraciones previas
-- =====================================================================================

-- Añadir nuevas columnas a Categorias si no existen
DO $$ 
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='categorias' AND column_name='codigo') THEN
        ALTER TABLE categorias ADD COLUMN codigo VARCHAR(50);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='categorias' AND column_name='categoria_padre_id') THEN
        ALTER TABLE categorias ADD COLUMN categoria_padre_id INT REFERENCES categorias(id);
    END IF;
END $$;

-- Añadir nuevas columnas a Marcas si no existen
DO $$ 
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='marcas' AND column_name='codigo') THEN
        ALTER TABLE marcas ADD COLUMN codigo VARCHAR(50);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='marcas' AND column_name='descripcion') THEN
        ALTER TABLE marcas ADD COLUMN descripcion VARCHAR(250);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='marcas' AND column_name='origen') THEN
        ALTER TABLE marcas ADD COLUMN origen VARCHAR(100);
    END IF;
END $$;