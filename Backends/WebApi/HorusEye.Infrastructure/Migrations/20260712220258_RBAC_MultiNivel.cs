using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HorusEye.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RBAC_MultiNivel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DispositivoRfidId",
                table: "Movimientos",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FabricantesDispositivo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UrlDocumentacion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EndpointEjemplo = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    FechaRegistro = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FabricantesDispositivo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Proveedores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    RazonSocial = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    RUC = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    Direccion = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Telefono = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    FechaRegistro = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Proveedores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CamposPayloadFabricante",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FabricanteDispositivoId = table.Column<Guid>(type: "uuid", nullable: false),
                    NombreCampoExterno = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NombreCampoInterno = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TipoDato = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Requerido = table.Column<bool>(type: "boolean", nullable: false),
                    ValorDefecto = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    OrdenExtraccion = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CamposPayloadFabricante", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CamposPayloadFabricante_FabricantesDispositivo_FabricanteDi~",
                        column: x => x.FabricanteDispositivoId,
                        principalTable: "FabricantesDispositivo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    RazonSocial = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    RUC = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    Direccion = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Telefono = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ProveedorId = table.Column<Guid>(type: "uuid", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    FechaRegistro = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Clientes_Proveedores_ProveedorId",
                        column: x => x.ProveedorId,
                        principalTable: "Proveedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "NodosUbicacion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TipoNodo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PadreId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClienteId = table.Column<Guid>(type: "uuid", nullable: false),
                    Nivel = table.Column<int>(type: "integer", nullable: false),
                    Metadata = table.Column<string>(type: "text", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    FechaRegistro = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NodosUbicacion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NodosUbicacion_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NodosUbicacion_NodosUbicacion_PadreId",
                        column: x => x.PadreId,
                        principalTable: "NodosUbicacion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UsuariosExtendidos",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    ProveedorId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClienteId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreadoPorUserId = table.Column<string>(type: "text", nullable: true),
                    FechaRegistro = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuariosExtendidos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsuariosExtendidos_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_UsuariosExtendidos_Proveedores_ProveedorId",
                        column: x => x.ProveedorId,
                        principalTable: "Proveedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "DispositivosRfid",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Fabricante = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Modelo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DireccionIP = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    Puerto = table.Column<int>(type: "integer", nullable: true),
                    Ubicacion = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    ClienteId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProveedorId = table.Column<Guid>(type: "uuid", nullable: true),
                    NodoUbicacionId = table.Column<Guid>(type: "uuid", nullable: true),
                    TipoDispositivo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    EndpointAPI = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    MetodoHTTP = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    FabricanteDispositivoId = table.Column<Guid>(type: "uuid", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    FechaRegistro = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DispositivosRfid", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DispositivosRfid_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DispositivosRfid_FabricantesDispositivo_FabricanteDispositi~",
                        column: x => x.FabricanteDispositivoId,
                        principalTable: "FabricantesDispositivo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DispositivosRfid_NodosUbicacion_NodoUbicacionId",
                        column: x => x.NodoUbicacionId,
                        principalTable: "NodosUbicacion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DispositivosRfid_Proveedores_ProveedorId",
                        column: x => x.ProveedorId,
                        principalTable: "Proveedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Movimientos_DispositivoRfidId",
                table: "Movimientos",
                column: "DispositivoRfidId");

            migrationBuilder.CreateIndex(
                name: "IX_CamposPayloadFabricante_FabricanteDispositivoId_NombreCampo~",
                table: "CamposPayloadFabricante",
                columns: new[] { "FabricanteDispositivoId", "NombreCampoExterno" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_ProveedorId",
                table: "Clientes",
                column: "ProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_DispositivosRfid_ClienteId",
                table: "DispositivosRfid",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_DispositivosRfid_DireccionIP",
                table: "DispositivosRfid",
                column: "DireccionIP");

            migrationBuilder.CreateIndex(
                name: "IX_DispositivosRfid_FabricanteDispositivoId",
                table: "DispositivosRfid",
                column: "FabricanteDispositivoId");

            migrationBuilder.CreateIndex(
                name: "IX_DispositivosRfid_NodoUbicacionId",
                table: "DispositivosRfid",
                column: "NodoUbicacionId");

            migrationBuilder.CreateIndex(
                name: "IX_DispositivosRfid_ProveedorId",
                table: "DispositivosRfid",
                column: "ProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_NodosUbicacion_ClienteId_Nombre",
                table: "NodosUbicacion",
                columns: new[] { "ClienteId", "Nombre" });

            migrationBuilder.CreateIndex(
                name: "IX_NodosUbicacion_PadreId",
                table: "NodosUbicacion",
                column: "PadreId");

            migrationBuilder.CreateIndex(
                name: "IX_Proveedores_RUC",
                table: "Proveedores",
                column: "RUC",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosExtendidos_ClienteId",
                table: "UsuariosExtendidos",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosExtendidos_ProveedorId",
                table: "UsuariosExtendidos",
                column: "ProveedorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Movimientos_DispositivosRfid_DispositivoRfidId",
                table: "Movimientos",
                column: "DispositivoRfidId",
                principalTable: "DispositivosRfid",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Movimientos_DispositivosRfid_DispositivoRfidId",
                table: "Movimientos");

            migrationBuilder.DropTable(
                name: "CamposPayloadFabricante");

            migrationBuilder.DropTable(
                name: "DispositivosRfid");

            migrationBuilder.DropTable(
                name: "UsuariosExtendidos");

            migrationBuilder.DropTable(
                name: "FabricantesDispositivo");

            migrationBuilder.DropTable(
                name: "NodosUbicacion");

            migrationBuilder.DropTable(
                name: "Clientes");

            migrationBuilder.DropTable(
                name: "Proveedores");

            migrationBuilder.DropIndex(
                name: "IX_Movimientos_DispositivoRfidId",
                table: "Movimientos");

            migrationBuilder.DropColumn(
                name: "DispositivoRfidId",
                table: "Movimientos");
        }
    }
}
