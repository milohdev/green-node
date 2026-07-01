-- ============================================================
-- GreenNode — init.sql
-- PostgreSQL 16
-- ============================================================

CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- ============================================================
-- TABLAS
-- ============================================================

CREATE TABLE IF NOT EXISTS "Users" (
    "Id"           UUID         NOT NULL DEFAULT uuid_generate_v4(),
    "Name"         VARCHAR(200) NOT NULL,
    "Email"        VARCHAR(200) NOT NULL,
    "PasswordHash" TEXT         NOT NULL,
    "Role"         VARCHAR(50)  NOT NULL,
    "CreatedAt"    TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    CONSTRAINT "PK_Users" PRIMARY KEY ("Id"),
    CONSTRAINT "UQ_Users_Email" UNIQUE ("Email")
);

CREATE TABLE IF NOT EXISTS "Territories" (
    "Id"                UUID         NOT NULL DEFAULT uuid_generate_v4(),
    "Name"              VARCHAR(200) NOT NULL,
    "Department"        VARCHAR(100) NOT NULL,
    "Municipality"      VARCHAR(100) NOT NULL,
    "AvailableHectares" NUMERIC(10,2) NOT NULL,
    "PricePerTree"      NUMERIC(10,2) NOT NULL,
    "CreatedAt"         TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    CONSTRAINT "PK_Territories" PRIMARY KEY ("Id"),
    CONSTRAINT "UQ_Territories_Name" UNIQUE ("Name")
);

CREATE TABLE IF NOT EXISTS "Companies" (
    "Id"        UUID         NOT NULL DEFAULT uuid_generate_v4(),
    "Name"      VARCHAR(200) NOT NULL,
    "Email"     VARCHAR(200) NOT NULL,
    "Phone"     VARCHAR(20)  NOT NULL,
    "Sector"    VARCHAR(100) NOT NULL,
    "UserId"    UUID         NOT NULL,
    "CreatedAt" TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    CONSTRAINT "PK_Companies"        PRIMARY KEY ("Id"),
    CONSTRAINT "UQ_Companies_Email"  UNIQUE ("Email"),
    CONSTRAINT "UQ_Companies_UserId" UNIQUE ("UserId"),
    CONSTRAINT "FK_Companies_Users"  FOREIGN KEY ("UserId")
        REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS "Quotes" (
    "Id"              UUID          NOT NULL DEFAULT uuid_generate_v4(),
    "CompanyId"       UUID          NOT NULL,
    "TerritoryId"     UUID          NOT NULL,
    "Sector"          VARCHAR(100)  NOT NULL,
    "NumberOfTrees"   INTEGER       NOT NULL,
    "Status"          VARCHAR(50)   NOT NULL DEFAULT 'Pending',
    "AiProposal"      TEXT,
    "RejectionReason" VARCHAR(1000),
    "TotalPrice"      NUMERIC(12,2) NOT NULL DEFAULT 0,
    "CreatedAt"       TIMESTAMPTZ   NOT NULL DEFAULT NOW(),
    CONSTRAINT "PK_Quotes"             PRIMARY KEY ("Id"),
    CONSTRAINT "CK_Quotes_Trees"       CHECK ("NumberOfTrees" > 0),
    CONSTRAINT "FK_Quotes_Companies"   FOREIGN KEY ("CompanyId")
        REFERENCES "Companies" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Quotes_Territories" FOREIGN KEY ("TerritoryId")
        REFERENCES "Territories" ("Id") ON DELETE RESTRICT
);

CREATE TABLE IF NOT EXISTS "Projects" (
    "Id"                 UUID          NOT NULL DEFAULT uuid_generate_v4(),
    "QuoteId"            UUID          NOT NULL,
    "CompanyId"          UUID          NOT NULL,
    "TerritoryId"        UUID          NOT NULL,
    "Sector"             VARCHAR(100)  NOT NULL,
    "NumberOfTrees"      INTEGER       NOT NULL,
    "ValidatedTrees"     INTEGER       NOT NULL DEFAULT 0,
    "ProgressPercentage" NUMERIC(5,2)  NOT NULL DEFAULT 0,
    "Status"             VARCHAR(50)   NOT NULL DEFAULT 'Active',
    "CertificateUrl"     VARCHAR(500),
    "CreatedAt"          TIMESTAMPTZ   NOT NULL DEFAULT NOW(),
    CONSTRAINT "PK_Projects"           PRIMARY KEY ("Id"),
    CONSTRAINT "UQ_Projects_QuoteId"   UNIQUE ("QuoteId"),
    CONSTRAINT "FK_Projects_Quotes"    FOREIGN KEY ("QuoteId")
        REFERENCES "Quotes" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Projects_Companies" FOREIGN KEY ("CompanyId")
        REFERENCES "Companies" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Projects_Territories" FOREIGN KEY ("TerritoryId")
        REFERENCES "Territories" ("Id") ON DELETE RESTRICT
);

CREATE TABLE IF NOT EXISTS "Evidences" (
    "Id"               UUID          NOT NULL DEFAULT uuid_generate_v4(),
    "ProjectId"        UUID          NOT NULL,
    "PhotoUrl"         VARCHAR(500)  NOT NULL,
    "AiValidated"      BOOLEAN       NOT NULL DEFAULT FALSE,
    "AiConfidence"     NUMERIC(5,2),
    "AiEstimatedTrees" INTEGER,
    "AiObservations"   VARCHAR(2000),
    "CreatedAt"        TIMESTAMPTZ   NOT NULL DEFAULT NOW(),
    CONSTRAINT "PK_Evidences"          PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Evidences_Projects" FOREIGN KEY ("ProjectId")
        REFERENCES "Projects" ("Id") ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS "Certificates" (
    "Id"               UUID          NOT NULL DEFAULT uuid_generate_v4(),
    "ProjectId"        UUID          NOT NULL,
    "CompanyName"      VARCHAR(200)  NOT NULL,
    "TerritoryName"    VARCHAR(200)  NOT NULL,
    "NumberOfTrees"    INTEGER       NOT NULL,
    "Co2EstimatedKg"   NUMERIC(12,2) NOT NULL,
    "PdfUrl"           VARCHAR(500)  NOT NULL,
    "VerificationCode" VARCHAR(50)   NOT NULL,
    "IssuedAt"         TIMESTAMPTZ   NOT NULL DEFAULT NOW(),
    "CreatedAt"        TIMESTAMPTZ   NOT NULL DEFAULT NOW(),
    CONSTRAINT "PK_Certificates"                  PRIMARY KEY ("Id"),
    CONSTRAINT "UQ_Certificates_ProjectId"        UNIQUE ("ProjectId"),
    CONSTRAINT "UQ_Certificates_VerificationCode" UNIQUE ("VerificationCode"),
    CONSTRAINT "FK_Certificates_Projects"         FOREIGN KEY ("ProjectId")
        REFERENCES "Projects" ("Id") ON DELETE RESTRICT
);

-- ============================================================
-- ÍNDICES
-- ============================================================

CREATE INDEX IF NOT EXISTS "IX_Quotes_CompanyId"
    ON "Quotes" ("CompanyId");
CREATE INDEX IF NOT EXISTS "IX_Quotes_Status"
    ON "Quotes" ("Status");
CREATE INDEX IF NOT EXISTS "IX_Projects_CompanyId"
    ON "Projects" ("CompanyId");
CREATE INDEX IF NOT EXISTS "IX_Projects_Status"
    ON "Projects" ("Status");
CREATE INDEX IF NOT EXISTS "IX_Evidences_ProjectId"
    ON "Evidences" ("ProjectId");
