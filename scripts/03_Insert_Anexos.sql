-- Script para crear la tabla de Anexos (Maestro de Socios de Negocio en Sistema Contable Peruano)

CREATE TABLE IF NOT EXISTS anexos (
    id SERIAL PRIMARY KEY,
    empresa_id INT NOT NULL REFERENCES empresas(id),
    tipo_anexo VARCHAR(2) NOT NULL, 
    codigo_anexo VARCHAR(20) NOT NULL, 
    tipo_documento_id VARCHAR(1) NOT NULL, 
    numero_documento VARCHAR(15) NOT NULL,
    tipo_persona VARCHAR(2) NOT NULL, 
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

-- Ejemplos de Anexos

INSERT INTO anexos (
    empresa_id, tipo_anexo, codigo_anexo, tipo_documento_id, numero_documento, 
    tipo_persona, razon_social, direccion, activo
) VALUES 
-- Cliente Jurídico (Factura)
(1, '02', '20100070970', '6', '20100070970', '02', 'SUPERMERCADOS PERUANOS S.A.', 'AV. AVIACION 2405 SAN BORJA', TRUE),

-- Proveedor Jurídico
(1, '03', '20100128218', '6', '20100128218', '02', 'ALICORP SAA', 'AV. ARGENTINA 4793 CALLAO', TRUE)
ON CONFLICT DO NOTHING;

INSERT INTO anexos (
    empresa_id, tipo_anexo, codigo_anexo, tipo_documento_id, numero_documento, 
    tipo_persona, nombres, apellido_paterno, apellido_materno, sexo, nacionalidad, direccion, activo
) VALUES 
-- Cliente Natural (Boleta)
(1, '02', '12345678', '1', '12345678', '01', 'JUAN PABLO', 'PEREZ', 'GARCIA', '1', '9589', 'CALLE LOS PINOS 123 - LIMA', TRUE),

-- Empleado (Planilla / Recibo Honorarios)
(1, '04', '87654321', '1', '87654321', '01', 'MARIA ROSA', 'GONZALEZ', 'SOTO', '0', '9589', 'AV. LOS ALAMOS 456 - LIMA', TRUE)
ON CONFLICT DO NOTHING;