// =========================================================================
// Copyright 2020 EPAM Systems, Inc.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// =========================================================================

using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using System;

namespace Epam.CoronaResistance.Services.User.IdentityServer.Persistence.Migrations.ConfigurationDB
{
    public partial class InitialConfigurationMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "configuration");

            migrationBuilder.CreateTable(
                name: "ApiResources",
                schema: "configuration",
                columns: table => new
                {
                    Id = table.Column<int>()
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Enabled = table.Column<bool>(),
                    Name = table.Column<string>(maxLength: 200),
                    DisplayName = table.Column<string>(maxLength: 200, nullable: true),
                    Description = table.Column<string>(maxLength: 1000, nullable: true),
                    Created = table.Column<DateTime>(),
                    Updated = table.Column<DateTime>(nullable: true),
                    LastAccessed = table.Column<DateTime>(nullable: true),
                    NonEditable = table.Column<bool>()
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiResources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Clients",
                schema: "configuration",
                columns: table => new
                {
                    Id = table.Column<int>()
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Enabled = table.Column<bool>(),
                    ClientId = table.Column<string>(maxLength: 200),
                    ProtocolType = table.Column<string>(maxLength: 200),
                    RequireClientSecret = table.Column<bool>(),
                    ClientName = table.Column<string>(maxLength: 200, nullable: true),
                    Description = table.Column<string>(maxLength: 1000, nullable: true),
                    ClientUri = table.Column<string>(maxLength: 2000, nullable: true),
                    LogoUri = table.Column<string>(maxLength: 2000, nullable: true),
                    RequireConsent = table.Column<bool>(),
                    AllowRememberConsent = table.Column<bool>(),
                    AlwaysIncludeUserClaimsInIdToken = table.Column<bool>(),
                    RequirePkce = table.Column<bool>(),
                    AllowPlainTextPkce = table.Column<bool>(),
                    AllowAccessTokensViaBrowser = table.Column<bool>(),
                    FrontChannelLogoutUri = table.Column<string>(maxLength: 2000, nullable: true),
                    FrontChannelLogoutSessionRequired = table.Column<bool>(),
                    BackChannelLogoutUri = table.Column<string>(maxLength: 2000, nullable: true),
                    BackChannelLogoutSessionRequired = table.Column<bool>(),
                    AllowOfflineAccess = table.Column<bool>(),
                    IdentityTokenLifetime = table.Column<int>(),
                    AccessTokenLifetime = table.Column<int>(),
                    AuthorizationCodeLifetime = table.Column<int>(),
                    ConsentLifetime = table.Column<int>(nullable: true),
                    AbsoluteRefreshTokenLifetime = table.Column<int>(),
                    SlidingRefreshTokenLifetime = table.Column<int>(),
                    RefreshTokenUsage = table.Column<int>(),
                    UpdateAccessTokenClaimsOnRefresh = table.Column<bool>(),
                    RefreshTokenExpiration = table.Column<int>(),
                    AccessTokenType = table.Column<int>(),
                    EnableLocalLogin = table.Column<bool>(),
                    IncludeJwtId = table.Column<bool>(),
                    AlwaysSendClientClaims = table.Column<bool>(),
                    ClientClaimsPrefix = table.Column<string>(maxLength: 200, nullable: true),
                    PairWiseSubjectSalt = table.Column<string>(maxLength: 200, nullable: true),
                    Created = table.Column<DateTime>(),
                    Updated = table.Column<DateTime>(nullable: true),
                    LastAccessed = table.Column<DateTime>(nullable: true),
                    UserSsoLifetime = table.Column<int>(nullable: true),
                    UserCodeType = table.Column<string>(maxLength: 100, nullable: true),
                    DeviceCodeLifetime = table.Column<int>(),
                    NonEditable = table.Column<bool>()
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IdentityResources",
                schema: "configuration",
                columns: table => new
                {
                    Id = table.Column<int>()
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Enabled = table.Column<bool>(),
                    Name = table.Column<string>(maxLength: 200),
                    DisplayName = table.Column<string>(maxLength: 200, nullable: true),
                    Description = table.Column<string>(maxLength: 1000, nullable: true),
                    Required = table.Column<bool>(),
                    Emphasize = table.Column<bool>(),
                    ShowInDiscoveryDocument = table.Column<bool>(),
                    Created = table.Column<DateTime>(),
                    Updated = table.Column<DateTime>(nullable: true),
                    NonEditable = table.Column<bool>()
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdentityResources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApiClaims",
                schema: "configuration",
                columns: table => new
                {
                    Id = table.Column<int>()
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Type = table.Column<string>(maxLength: 200),
                    ApiResourceId = table.Column<int>()
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApiClaims_ApiResources_ApiResourceId",
                        column: x => x.ApiResourceId,
                        principalSchema: "configuration",
                        principalTable: "ApiResources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApiProperties",
                schema: "configuration",
                columns: table => new
                {
                    Id = table.Column<int>()
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Key = table.Column<string>(maxLength: 250),
                    Value = table.Column<string>(maxLength: 2000),
                    ApiResourceId = table.Column<int>()
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiProperties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApiProperties_ApiResources_ApiResourceId",
                        column: x => x.ApiResourceId,
                        principalSchema: "configuration",
                        principalTable: "ApiResources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApiScopes",
                schema: "configuration",
                columns: table => new
                {
                    Id = table.Column<int>()
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(maxLength: 200),
                    DisplayName = table.Column<string>(maxLength: 200, nullable: true),
                    Description = table.Column<string>(maxLength: 1000, nullable: true),
                    Required = table.Column<bool>(),
                    Emphasize = table.Column<bool>(),
                    ShowInDiscoveryDocument = table.Column<bool>(),
                    ApiResourceId = table.Column<int>()
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiScopes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApiScopes_ApiResources_ApiResourceId",
                        column: x => x.ApiResourceId,
                        principalSchema: "configuration",
                        principalTable: "ApiResources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApiSecrets",
                schema: "configuration",
                columns: table => new
                {
                    Id = table.Column<int>()
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Description = table.Column<string>(maxLength: 1000, nullable: true),
                    Value = table.Column<string>(maxLength: 4000),
                    Expiration = table.Column<DateTime>(nullable: true),
                    Type = table.Column<string>(maxLength: 250),
                    Created = table.Column<DateTime>(),
                    ApiResourceId = table.Column<int>()
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiSecrets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApiSecrets_ApiResources_ApiResourceId",
                        column: x => x.ApiResourceId,
                        principalSchema: "configuration",
                        principalTable: "ApiResources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClientClaims",
                schema: "configuration",
                columns: table => new
                {
                    Id = table.Column<int>()
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Type = table.Column<string>(maxLength: 250),
                    Value = table.Column<string>(maxLength: 250),
                    ClientId = table.Column<int>()
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientClaims_Clients_ClientId",
                        column: x => x.ClientId,
                        principalSchema: "configuration",
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClientCorsOrigins",
                schema: "configuration",
                columns: table => new
                {
                    Id = table.Column<int>()
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Origin = table.Column<string>(maxLength: 150),
                    ClientId = table.Column<int>()
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientCorsOrigins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientCorsOrigins_Clients_ClientId",
                        column: x => x.ClientId,
                        principalSchema: "configuration",
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClientGrantTypes",
                schema: "configuration",
                columns: table => new
                {
                    Id = table.Column<int>()
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GrantType = table.Column<string>(maxLength: 250),
                    ClientId = table.Column<int>()
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientGrantTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientGrantTypes_Clients_ClientId",
                        column: x => x.ClientId,
                        principalSchema: "configuration",
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClientIdPRestrictions",
                schema: "configuration",
                columns: table => new
                {
                    Id = table.Column<int>()
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Provider = table.Column<string>(maxLength: 200),
                    ClientId = table.Column<int>()
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientIdPRestrictions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientIdPRestrictions_Clients_ClientId",
                        column: x => x.ClientId,
                        principalSchema: "configuration",
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClientPostLogoutRedirectUris",
                schema: "configuration",
                columns: table => new
                {
                    Id = table.Column<int>()
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PostLogoutRedirectUri = table.Column<string>(maxLength: 2000),
                    ClientId = table.Column<int>()
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientPostLogoutRedirectUris", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientPostLogoutRedirectUris_Clients_ClientId",
                        column: x => x.ClientId,
                        principalSchema: "configuration",
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClientProperties",
                schema: "configuration",
                columns: table => new
                {
                    Id = table.Column<int>()
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Key = table.Column<string>(maxLength: 250),
                    Value = table.Column<string>(maxLength: 2000),
                    ClientId = table.Column<int>()
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientProperties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientProperties_Clients_ClientId",
                        column: x => x.ClientId,
                        principalSchema: "configuration",
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClientRedirectUris",
                schema: "configuration",
                columns: table => new
                {
                    Id = table.Column<int>()
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RedirectUri = table.Column<string>(maxLength: 2000),
                    ClientId = table.Column<int>()
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientRedirectUris", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientRedirectUris_Clients_ClientId",
                        column: x => x.ClientId,
                        principalSchema: "configuration",
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClientScopes",
                schema: "configuration",
                columns: table => new
                {
                    Id = table.Column<int>()
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Scope = table.Column<string>(maxLength: 200),
                    ClientId = table.Column<int>()
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientScopes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientScopes_Clients_ClientId",
                        column: x => x.ClientId,
                        principalSchema: "configuration",
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClientSecrets",
                schema: "configuration",
                columns: table => new
                {
                    Id = table.Column<int>()
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Description = table.Column<string>(maxLength: 2000, nullable: true),
                    Value = table.Column<string>(maxLength: 4000),
                    Expiration = table.Column<DateTime>(nullable: true),
                    Type = table.Column<string>(maxLength: 250),
                    Created = table.Column<DateTime>(),
                    ClientId = table.Column<int>()
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientSecrets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientSecrets_Clients_ClientId",
                        column: x => x.ClientId,
                        principalSchema: "configuration",
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IdentityClaims",
                schema: "configuration",
                columns: table => new
                {
                    Id = table.Column<int>()
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Type = table.Column<string>(maxLength: 200),
                    IdentityResourceId = table.Column<int>()
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdentityClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IdentityClaims_IdentityResources_IdentityResourceId",
                        column: x => x.IdentityResourceId,
                        principalSchema: "configuration",
                        principalTable: "IdentityResources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IdentityProperties",
                schema: "configuration",
                columns: table => new
                {
                    Id = table.Column<int>()
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Key = table.Column<string>(maxLength: 250),
                    Value = table.Column<string>(maxLength: 2000),
                    IdentityResourceId = table.Column<int>()
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdentityProperties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IdentityProperties_IdentityResources_IdentityResourceId",
                        column: x => x.IdentityResourceId,
                        principalSchema: "configuration",
                        principalTable: "IdentityResources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApiScopeClaims",
                schema: "configuration",
                columns: table => new
                {
                    Id = table.Column<int>()
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Type = table.Column<string>(maxLength: 200),
                    ApiScopeId = table.Column<int>()
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiScopeClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApiScopeClaims_ApiScopes_ApiScopeId",
                        column: x => x.ApiScopeId,
                        principalSchema: "configuration",
                        principalTable: "ApiScopes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApiClaims_ApiResourceId",
                schema: "configuration",
                table: "ApiClaims",
                column: "ApiResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_ApiProperties_ApiResourceId",
                schema: "configuration",
                table: "ApiProperties",
                column: "ApiResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_ApiResources_Name",
                schema: "configuration",
                table: "ApiResources",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApiScopeClaims_ApiScopeId",
                schema: "configuration",
                table: "ApiScopeClaims",
                column: "ApiScopeId");

            migrationBuilder.CreateIndex(
                name: "IX_ApiScopes_ApiResourceId",
                schema: "configuration",
                table: "ApiScopes",
                column: "ApiResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_ApiScopes_Name",
                schema: "configuration",
                table: "ApiScopes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApiSecrets_ApiResourceId",
                schema: "configuration",
                table: "ApiSecrets",
                column: "ApiResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientClaims_ClientId",
                schema: "configuration",
                table: "ClientClaims",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientCorsOrigins_ClientId",
                schema: "configuration",
                table: "ClientCorsOrigins",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientGrantTypes_ClientId",
                schema: "configuration",
                table: "ClientGrantTypes",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientIdPRestrictions_ClientId",
                schema: "configuration",
                table: "ClientIdPRestrictions",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientPostLogoutRedirectUris_ClientId",
                schema: "configuration",
                table: "ClientPostLogoutRedirectUris",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientProperties_ClientId",
                schema: "configuration",
                table: "ClientProperties",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientRedirectUris_ClientId",
                schema: "configuration",
                table: "ClientRedirectUris",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_ClientId",
                schema: "configuration",
                table: "Clients",
                column: "ClientId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClientScopes_ClientId",
                schema: "configuration",
                table: "ClientScopes",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientSecrets_ClientId",
                schema: "configuration",
                table: "ClientSecrets",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_IdentityClaims_IdentityResourceId",
                schema: "configuration",
                table: "IdentityClaims",
                column: "IdentityResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_IdentityProperties_IdentityResourceId",
                schema: "configuration",
                table: "IdentityProperties",
                column: "IdentityResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_IdentityResources_Name",
                schema: "configuration",
                table: "IdentityResources",
                column: "Name",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApiClaims",
                schema: "configuration");

            migrationBuilder.DropTable(
                name: "ApiProperties",
                schema: "configuration");

            migrationBuilder.DropTable(
                name: "ApiScopeClaims",
                schema: "configuration");

            migrationBuilder.DropTable(
                name: "ApiSecrets",
                schema: "configuration");

            migrationBuilder.DropTable(
                name: "ClientClaims",
                schema: "configuration");

            migrationBuilder.DropTable(
                name: "ClientCorsOrigins",
                schema: "configuration");

            migrationBuilder.DropTable(
                name: "ClientGrantTypes",
                schema: "configuration");

            migrationBuilder.DropTable(
                name: "ClientIdPRestrictions",
                schema: "configuration");

            migrationBuilder.DropTable(
                name: "ClientPostLogoutRedirectUris",
                schema: "configuration");

            migrationBuilder.DropTable(
                name: "ClientProperties",
                schema: "configuration");

            migrationBuilder.DropTable(
                name: "ClientRedirectUris",
                schema: "configuration");

            migrationBuilder.DropTable(
                name: "ClientScopes",
                schema: "configuration");

            migrationBuilder.DropTable(
                name: "ClientSecrets",
                schema: "configuration");

            migrationBuilder.DropTable(
                name: "IdentityClaims",
                schema: "configuration");

            migrationBuilder.DropTable(
                name: "IdentityProperties",
                schema: "configuration");

            migrationBuilder.DropTable(
                name: "ApiScopes",
                schema: "configuration");

            migrationBuilder.DropTable(
                name: "Clients",
                schema: "configuration");

            migrationBuilder.DropTable(
                name: "IdentityResources",
                schema: "configuration");

            migrationBuilder.DropTable(
                name: "ApiResources",
                schema: "configuration");
        }
    }
}