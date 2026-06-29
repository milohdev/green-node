-- ============================================================
-- GreenNode — init.sql
-- Estructura completa de la base de datos
-- PostgreSQL 16
-- ============================================================

-- ============================================================
-- 1. EXTENSIONES
-- ============================================================

CREATE EXTENSION IF NOT EXISTS "uuid-ossp";


-- ============================================================
-- 2. TIPOS ENUM
-- ============================================================

-- Rol del usuario en el sistema
DO $$ BEGIN
    CREATE TYPE user_role AS ENUM ('Admin', 'Company');
EXCEPTION WHEN duplicate_object THEN NULL;
END $$;

-- Estados del ciclo de vida de una cotización
DO $$ BEGIN
    CREATE TYPE quote_status AS ENUM (
        'Pending',
        'UnderReview',
        'Approved',
        'Rejected'
    );
EXCEPTION WHEN duplicate_object THEN NULL;
END $$;

-- Estados del ciclo de vida de un proyecto
DO $$ BEGIN
    CREATE TYPE project_status AS ENUM ('Active', 'Completed');
EXCEPTION WHEN duplicate_object THEN NULL;
END $$;

-- Resultado de la validación IA de una evidencia fotográfica
DO $$ BEGIN
    CREATE TYPE ai_validation_status AS ENUM (
        'Pending',
        'Validated',
        'Rejected'
    );
EXCEPTION WHEN duplicate_object THEN NULL;
END $$;


-- ============================================================
-- 3. TABLAS
-- El orden importa por las FK: primero las independientes
-- ============================================================

-- ------------------------------------------------------------
-- Users: usuarios del sistema (admins y representantes de empresa)
-- ------------------------------------------------------------
CREATE TABLE IF NOT EXISTS "Users" (
    "Id"           UUID         NOT NULL DEFAULT uuid_generate_v4(),
    "Email"        VARCHAR(256) NOT NULL,
    "PasswordHash" TEXT         NOT NULL,
    "Role"         user_role    NOT NULL,
    "CreatedAt"    TIMESTAMPTZ  NOT NULL DEFAULT NOW(),

    CONSTRAINT "PK_Users" PRIMARY KEY ("Id"),
    CONSTRAINT "UQ_Users_Email" UNIQUE ("Email")
);

-- ------------------------------------------------------------
-- Companies: empresas registradas en el sistema
-- Relación 1:1 con Users (un usuario = una empresa)
-- ------------------------------------------------------------
CREATE TABLE IF NOT EXISTS "Companies" (
    "Id"           UUID         NOT NULL DEFAULT uuid_generate_v4(),
    "UserId"       UUID         NOT NULL,
    "Name"         VARCHAR(200) NOT NULL,
    "NIT"          VARCHAR(20)  NOT NULL,
    "ContactPhone" VARCHAR(20)  NOT NULL,
    "ContactEmail" VARCHAR(256) NOT NULL,
    "CreatedAt"    TIMESTAMPTZ  NOT NULL DEFAULT NOW(),

    CONSTRAINT "PK_Companies"        PRIMARY KEY ("Id"),
    CONSTRAINT "UQ_Companies_NIT"    UNIQUE ("NIT"),
    CONSTRAINT "UQ_Companies_UserId" UNIQUE ("UserId"),
    CONSTRAINT "FK_Companies_Users"  FOREIGN KEY ("UserId")
        REFERENCES "Users" ("Id")
        ON DELETE RESTRICT
);

-- ------------------------------------------------------------
-- Territories: territorios colombianos disponibles para siembra
-- ------------------------------------------------------------
CREATE TABLE IF NOT EXISTS "Territories" (
    "Id"         UUID         NOT NULL DEFAULT uuid_generate_v4(),
    "Name"       VARCHAR(200) NOT NULL,
    "Department" VARCHAR(100) NOT NULL,
    "Region"     VARCHAR(100) NOT NULL,
    "CreatedAt"  TIMESTAMPTZ  NOT NULL DEFAULT NOW(),

    CONSTRAINT "PK_Territories" PRIMARY KEY ("Id")
);

-- ------------------------------------------------------------
-- Quotes: cotizaciones solicitadas por empresas
-- ------------------------------------------------------------
CREATE TABLE IF NOT EXISTS "Quotes" (
    "Id"          UUID          NOT NULL DEFAULT uuid_generate_v4(),
    "CompanyId"   UUID          NOT NULL,
    "TerritoryId" UUID          NOT NULL,
    "Sector"      VARCHAR(100)  NOT NULL,
    "TreeCount"   INTEGER       NOT NULL,
    "Status"      quote_status  NOT NULL DEFAULT 'Pending',
    "AiProposal"  TEXT,
    "RequestedAt" TIMESTAMPTZ   NOT NULL DEFAULT NOW(),
    "ReviewedAt"  TIMESTAMPTZ,
    "CreatedAt"   TIMESTAMPTZ   NOT NULL DEFAULT NOW(),

    CONSTRAINT "PK_Quotes"              PRIMARY KEY ("Id"),
    CONSTRAINT "CK_Quotes_TreeCount"    CHECK ("TreeCount" > 0),
    CONSTRAINT "FK_Quotes_Companies"    FOREIGN KEY ("CompanyId")
        REFERENCES "Companies" ("Id")
        ON DELETE RESTRICT,
    CONSTRAINT "FK_Quotes_Territories"  FOREIGN KEY ("TerritoryId")
        REFERENCES "Territories" ("Id")
        ON DELETE RESTRICT
);

-- ------------------------------------------------------------
-- Projects: proyectos generados al aprobar una cotización
-- Relación 1:1 con Quotes (una cotización aprobada = un proyecto)
-- CompanyId desnormalizado para filtros rápidos por empresa
-- ------------------------------------------------------------
CREATE TABLE IF NOT EXISTS "Projects" (
    "Id"                 UUID            NOT NULL DEFAULT uuid_generate_v4(),
    "QuoteId"            UUID            NOT NULL,
    "CompanyId"          UUID            NOT NULL,
    "Status"             project_status  NOT NULL DEFAULT 'Active',
    "ValidatedTreeCount" INTEGER         NOT NULL DEFAULT 0,
    "ProgressPercent"    NUMERIC(5,2)    NOT NULL DEFAULT 0,
    "EstimatedCO2Tons"   NUMERIC(10,4)   NOT NULL DEFAULT 0,
    "StartedAt"          TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    "CompletedAt"        TIMESTAMPTZ,
    "CreatedAt"          TIMESTAMPTZ     NOT NULL DEFAULT NOW(),

    CONSTRAINT "PK_Projects"           PRIMARY KEY ("Id"),
    CONSTRAINT "UQ_Projects_QuoteId"   UNIQUE ("QuoteId"),
    CONSTRAINT "FK_Projects_Quotes"    FOREIGN KEY ("QuoteId")
        REFERENCES "Quotes" ("Id")
        ON DELETE RESTRICT,
    CONSTRAINT "FK_Projects_Companies" FOREIGN KEY ("CompanyId")
        REFERENCES "Companies" ("Id")
        ON DELETE RESTRICT
);

-- ------------------------------------------------------------
-- Evidences: fotografías de evidencia subidas por el admin
-- La validación IA corre en background (AiValidationStatus inicia en Pending)
-- ------------------------------------------------------------
CREATE TABLE IF NOT EXISTS "Evidences" (
    "Id"                   UUID                 NOT NULL DEFAULT uuid_generate_v4(),
    "ProjectId"            UUID                 NOT NULL,
    "FileUrl"              VARCHAR(500)         NOT NULL,
    "AiValidationStatus"   ai_validation_status NOT NULL DEFAULT 'Pending',
    "AiEstimatedTreeCount" INTEGER,
    "AiValidationNotes"    TEXT,
    "UploadedAt"           TIMESTAMPTZ          NOT NULL DEFAULT NOW(),
    "AiValidatedAt"        TIMESTAMPTZ,
    "CreatedAt"            TIMESTAMPTZ          NOT NULL DEFAULT NOW(),

    CONSTRAINT "PK_Evidences"          PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Evidences_Projects" FOREIGN KEY ("ProjectId")
        REFERENCES "Projects" ("Id")
        ON DELETE CASCADE
);

-- ------------------------------------------------------------
-- Certificates: certificados PDF generados al completar un proyecto
-- Relación 1:1 con Projects
-- ------------------------------------------------------------
CREATE TABLE IF NOT EXISTS "Certificates" (
    "Id"               UUID         NOT NULL DEFAULT uuid_generate_v4(),
    "ProjectId"        UUID         NOT NULL,
    "FileUrl"          VARCHAR(500) NOT NULL,
    "VerificationCode" VARCHAR(50)  NOT NULL,
    "IssuedAt"         TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    "CreatedAt"        TIMESTAMPTZ  NOT NULL DEFAULT NOW(),

    CONSTRAINT "PK_Certificates"                  PRIMARY KEY ("Id"),
    CONSTRAINT "UQ_Certificates_ProjectId"        UNIQUE ("ProjectId"),
    CONSTRAINT "UQ_Certificates_VerificationCode" UNIQUE ("VerificationCode"),
    CONSTRAINT "FK_Certificates_Projects"         FOREIGN KEY ("ProjectId")
        REFERENCES "Projects" ("Id")
        ON DELETE RESTRICT
);


-- ============================================================
-- 4. ÍNDICES DE RENDIMIENTO
-- (los únicos ya están como UNIQUE constraints arriba)
-- ============================================================

-- Búsquedas frecuentes de cotizaciones por empresa y estado
CREATE INDEX IF NOT EXISTS "IX_Quotes_CompanyId"
    ON "Quotes" ("CompanyId");

CREATE INDEX IF NOT EXISTS "IX_Quotes_Status"
    ON "Quotes" ("Status");

CREATE INDEX IF NOT EXISTS "IX_Quotes_CompanyId_Status"
    ON "Quotes" ("CompanyId", "Status");

-- Búsquedas frecuentes de proyectos por empresa y estado
CREATE INDEX IF NOT EXISTS "IX_Projects_CompanyId"
    ON "Projects" ("CompanyId");

CREATE INDEX IF NOT EXISTS "IX_Projects_Status"
    ON "Projects" ("Status");

-- Búsquedas de evidencias por proyecto
CREATE INDEX IF NOT EXISTS "IX_Evidences_ProjectId"
    ON "Evidences" ("ProjectId");

-- Búsquedas de evidencias pendientes de validación IA
CREATE INDEX IF NOT EXISTS "IX_Evidences_AiValidationStatus"
    ON "Evidences" ("AiValidationStatus");


-- ============================================================
-- FIN — GreenNode init.sql
-- ============================================================
