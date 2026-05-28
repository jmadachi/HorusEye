-- ============================================================
-- Script: init.sql
-- Descripción: Esquema completo HorusEye - Control de Inventarios RFID
-- ============================================================

CREATE TABLE IF NOT EXISTS public."Tags" (
    "Id" VARCHAR(200) NOT NULL,
    "Estado" VARCHAR(20) NOT NULL DEFAULT 'REGISTRADO',
    "FechaRegistro" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "FechaActualizacion" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT "PK_Tags" PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS public."TagDanioHistorial" (
    "Id" BIGINT GENERATED ALWAYS AS IDENTITY,
    "TagId" VARCHAR(200) NOT NULL,
    "Descripcion" VARCHAR(600) NOT NULL,
    "FechaReporte" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "ReportadoPor" VARCHAR(300),
    CONSTRAINT "PK_TagDanioHistorial" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_TagDanioHistorial_Tags" FOREIGN KEY ("TagId") REFERENCES public."Tags" ("Id") ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS public."Activos" (
    "Id" UUID NOT NULL DEFAULT gen_random_uuid(),
    "Placa" VARCHAR(100) NOT NULL,
    "Nombre" VARCHAR(300) NOT NULL,
    "Categoria" VARCHAR(100) NOT NULL,
    "TenedorResponsable" VARCHAR(300),
    "EstadoUbicacion" VARCHAR(30) NOT NULL DEFAULT 'DENTRO_INSTALACIONES',
    "TagId" VARCHAR(200),
    "FechaRegistro" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "FechaActualizacion" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT "PK_Activos" PRIMARY KEY ("Id"),
    CONSTRAINT "UK_Activos_Placa" UNIQUE ("Placa"),
    CONSTRAINT "FK_Activos_Tags" FOREIGN KEY ("TagId") REFERENCES public."Tags" ("Id") ON DELETE SET NULL
);

CREATE INDEX IF NOT EXISTS "IX_Activos_Placa" ON public."Activos" ("Placa");
CREATE INDEX IF NOT EXISTS "IX_Activos_TagId" ON public."Activos" ("TagId");

CREATE TABLE IF NOT EXISTS public."Movimientos" (
    "Id" BIGINT GENERATED ALWAYS AS IDENTITY,
    "ActivoId" UUID NOT NULL,
    "PuntoLecturaId" VARCHAR(100),
    "TipoMovimiento" VARCHAR(10) NOT NULL,
    "Autorizado" BOOLEAN NOT NULL DEFAULT FALSE,
    "AlarmaActivada" BOOLEAN NOT NULL DEFAULT FALSE,
    "FechaRegistro" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT "PK_Movimientos" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Movimientos_Activos" FOREIGN KEY ("ActivoId") REFERENCES public."Activos" ("Id") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_Movimientos_FechaRegistro" ON public."Movimientos" ("FechaRegistro");
CREATE INDEX IF NOT EXISTS "IX_Movimientos_ActivoId" ON public."Movimientos" ("ActivoId");

CREATE TABLE IF NOT EXISTS public."AutorizacionesSalida" (
    "Id" BIGINT GENERATED ALWAYS AS IDENTITY,
    "ActivoId" UUID NOT NULL,
    "AutorizadoPor" VARCHAR(300) NOT NULL,
    "FechaAutorizacion" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "FechaVencimiento" TIMESTAMPTZ,
    "Activa" BOOLEAN NOT NULL DEFAULT TRUE,
    CONSTRAINT "PK_AutorizacionesSalida" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_AutorizacionesSalida_Activos" FOREIGN KEY ("ActivoId") REFERENCES public."Activos" ("Id") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_AutorizacionesSalida_ActivoId" ON public."AutorizacionesSalida" ("ActivoId");

CREATE TABLE IF NOT EXISTS public."RefreshTokens" (
    "Id" BIGINT GENERATED ALWAYS AS IDENTITY,
    "UserId" VARCHAR(450) NOT NULL,
    "Token" VARCHAR(500) NOT NULL,
    "FechaExpiracion" TIMESTAMPTZ NOT NULL,
    "FechaCreacion" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "Revocado" BOOLEAN NOT NULL DEFAULT FALSE,
    CONSTRAINT "PK_RefreshTokens" PRIMARY KEY ("Id"),
    CONSTRAINT "UK_RefreshTokens_Token" UNIQUE ("Token")
);

CREATE INDEX IF NOT EXISTS "IX_RefreshTokens_UserId" ON public."RefreshTokens" ("UserId");

-- ============================================================
-- ASP.NET CORE IDENTITY TABLES
-- ============================================================

CREATE TABLE IF NOT EXISTS public."AspNetRoles" (
    "Id" TEXT NOT NULL,
    "Name" VARCHAR(256),
    "NormalizedName" VARCHAR(256),
    "ConcurrencyStamp" TEXT,
    CONSTRAINT "PK_AspNetRoles" PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS public."AspNetUsers" (
    "Id" TEXT NOT NULL,
    "UserName" VARCHAR(256),
    "NormalizedUserName" VARCHAR(256),
    "Email" VARCHAR(256),
    "NormalizedEmail" VARCHAR(256),
    "EmailConfirmed" BOOLEAN NOT NULL DEFAULT FALSE,
    "PasswordHash" TEXT,
    "SecurityStamp" TEXT,
    "ConcurrencyStamp" TEXT,
    "PhoneNumber" TEXT,
    "PhoneNumberConfirmed" BOOLEAN NOT NULL DEFAULT FALSE,
    "TwoFactorEnabled" BOOLEAN NOT NULL DEFAULT FALSE,
    "LockoutEnd" TIMESTAMPTZ,
    "LockoutEnabled" BOOLEAN NOT NULL DEFAULT FALSE,
    "AccessFailedCount" INTEGER NOT NULL DEFAULT 0,
    CONSTRAINT "PK_AspNetUsers" PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS public."AspNetRoleClaims" (
    "Id" INTEGER GENERATED ALWAYS AS IDENTITY,
    "RoleId" TEXT NOT NULL,
    "ClaimType" TEXT,
    "ClaimValue" TEXT,
    CONSTRAINT "PK_AspNetRoleClaims" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_AspNetRoleClaims_AspNetRoles" FOREIGN KEY ("RoleId") REFERENCES public."AspNetRoles" ("Id") ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS public."AspNetUserClaims" (
    "Id" INTEGER GENERATED ALWAYS AS IDENTITY,
    "UserId" TEXT NOT NULL,
    "ClaimType" TEXT,
    "ClaimValue" TEXT,
    CONSTRAINT "PK_AspNetUserClaims" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_AspNetUserClaims_AspNetUsers" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS public."AspNetUserLogins" (
    "LoginProvider" TEXT NOT NULL,
    "ProviderKey" TEXT NOT NULL,
    "ProviderDisplayName" TEXT,
    "UserId" TEXT NOT NULL,
    CONSTRAINT "PK_AspNetUserLogins" PRIMARY KEY ("LoginProvider", "ProviderKey"),
    CONSTRAINT "FK_AspNetUserLogins_AspNetUsers" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS public."AspNetUserTokens" (
    "UserId" TEXT NOT NULL,
    "LoginProvider" TEXT NOT NULL,
    "Name" TEXT NOT NULL,
    "Value" TEXT,
    CONSTRAINT "PK_AspNetUserTokens" PRIMARY KEY ("UserId", "LoginProvider", "Name"),
    CONSTRAINT "FK_AspNetUserTokens_AspNetUsers" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS public."AspNetUserRoles" (
    "UserId" TEXT NOT NULL,
    "RoleId" TEXT NOT NULL,
    CONSTRAINT "PK_AspNetUserRoles" PRIMARY KEY ("UserId", "RoleId"),
    CONSTRAINT "FK_AspNetUserRoles_AspNetUsers" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_AspNetUserRoles_AspNetRoles" FOREIGN KEY ("RoleId") REFERENCES public."AspNetRoles" ("Id") ON DELETE CASCADE
);
