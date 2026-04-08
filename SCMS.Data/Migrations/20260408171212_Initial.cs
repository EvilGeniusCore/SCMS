using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SCMS.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    MustChangePassword = table.Column<bool>(type: "INTEGER", nullable: false),
                    UserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: true),
                    SecurityStamp = table.Column<string>(type: "TEXT", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true),
                    PhoneNumber = table.Column<string>(type: "TEXT", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PageContents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PageKey = table.Column<string>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    HtmlContent = table.Column<string>(type: "TEXT", nullable: false),
                    MetaDescription = table.Column<string>(type: "TEXT", nullable: true),
                    MetaKeywords = table.Column<string>(type: "TEXT", nullable: true),
                    Visibility = table.Column<string>(type: "TEXT", nullable: false),
                    TemplateKey = table.Column<string>(type: "TEXT", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PageContents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SecurityLevels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    IsSystem = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityLevels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SocialMediaPlatforms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    IconClass = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SocialMediaPlatforms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ThemeSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    PreviewImage = table.Column<string>(type: "TEXT", nullable: true),
                    Favicon = table.Column<string>(type: "TEXT", nullable: true),
                    SetOn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThemeSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RoleId = table.Column<string>(type: "TEXT", nullable: false),
                    ClaimType = table.Column<string>(type: "TEXT", nullable: true),
                    ClaimValue = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    ClaimType = table.Column<string>(type: "TEXT", nullable: true),
                    ClaimValue = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "TEXT", nullable: false),
                    ProviderKey = table.Column<string>(type: "TEXT", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "TEXT", nullable: true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    RoleId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    LoginProvider = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MenuItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ParentId = table.Column<int>(type: "INTEGER", nullable: true),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Url = table.Column<string>(type: "TEXT", nullable: true),
                    PageContentId = table.Column<int>(type: "INTEGER", nullable: true),
                    MenuGroup = table.Column<string>(type: "TEXT", nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    IsVisible = table.Column<bool>(type: "INTEGER", nullable: false),
                    SecurityLevelId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MenuItems_PageContents_PageContentId",
                        column: x => x.PageContentId,
                        principalTable: "PageContents",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MenuItems_SecurityLevels_SecurityLevelId",
                        column: x => x.SecurityLevelId,
                        principalTable: "SecurityLevels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SecurityLevelRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SecurityLevelId = table.Column<int>(type: "INTEGER", nullable: false),
                    RoleName = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityLevelRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SecurityLevelRoles_SecurityLevels_SecurityLevelId",
                        column: x => x.SecurityLevelId,
                        principalTable: "SecurityLevels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SiteSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SiteName = table.Column<string>(type: "TEXT", nullable: false),
                    Tagline = table.Column<string>(type: "TEXT", nullable: true),
                    Logo = table.Column<string>(type: "TEXT", nullable: true),
                    ContactEmail = table.Column<string>(type: "TEXT", nullable: true),
                    ContactPhone = table.Column<string>(type: "TEXT", nullable: true),
                    ContactAddress = table.Column<string>(type: "TEXT", nullable: true),
                    Copyright = table.Column<string>(type: "TEXT", nullable: true),
                    ThemeId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SiteSettings_ThemeSettings_ThemeId",
                        column: x => x.ThemeId,
                        principalTable: "ThemeSettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SiteSocialLinks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Url = table.Column<string>(type: "TEXT", nullable: false),
                    IconColor = table.Column<string>(type: "TEXT", nullable: true),
                    SiteSettingsId = table.Column<int>(type: "INTEGER", nullable: false),
                    SocialMediaPlatformId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteSocialLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SiteSocialLinks_SiteSettings_SiteSettingsId",
                        column: x => x.SiteSettingsId,
                        principalTable: "SiteSettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SiteSocialLinks_SocialMediaPlatforms_SocialMediaPlatformId",
                        column: x => x.SocialMediaPlatformId,
                        principalTable: "SocialMediaPlatforms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "a1b2c3d4-0001-0000-0000-000000000001", "00000000-0000-0000-0000-000000000001", "Admin", "ADMIN" },
                    { "a1b2c3d4-0002-0000-0000-000000000002", "00000000-0000-0000-0000-000000000002", "Editor", "EDITOR" },
                    { "a1b2c3d4-0003-0000-0000-000000000003", "00000000-0000-0000-0000-000000000003", "User", "USER" }
                });

            migrationBuilder.InsertData(
                table: "PageContents",
                columns: new[] { "Id", "HtmlContent", "LastUpdated", "MetaDescription", "MetaKeywords", "PageKey", "TemplateKey", "Title", "UpdatedBy", "Visibility" },
                values: new object[,]
                {
                    { 1, "\r\n                    <h1 class=\"display-4 mb-4\">Welcome</h1>\r\n                    <p class=\"lead mb-4\">This is your first SCMS page. Edit it in the admin panel.</p>", new DateTime(2025, 6, 8, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "home", "Display", "Welcome", null, "Public" },
                    { 2, "\r\n                    <div class=\"d-flex align-items-center justify-content-center\">\r\n                        <div class=\"card shadow p-4\" style=\"max-width: 400px; width: 100%;\">\r\n                            <h2 class=\"text-center mb-4\">Login</h2>\r\n                            <form action=\"/Identity/Account/Login\" method=\"post\">\r\n                                <input name=\"__RequestVerificationToken\" type=\"hidden\" value=\"{{ANTIFORGERY_TOKEN}}\" />\r\n                                <div class=\"mb-3\">\r\n                                    <label for=\"email\" class=\"form-label\">Email address</label>\r\n                                    <input type=\"email\" class=\"form-control\" id=\"email\" name=\"Input.Email\" required />\r\n                                </div>\r\n                                <div class=\"mb-3\">\r\n                                    <label for=\"password\" class=\"form-label\">Password</label>\r\n                                    <input type=\"password\" class=\"form-control\" id=\"password\" name=\"Input.Password\" required />\r\n                                </div>\r\n                                <div class=\"mb-3 form-check\">\r\n                                    <input type=\"checkbox\" class=\"form-check-input\" id=\"rememberMe\" name=\"Input.RememberMe\" />\r\n                                    <label class=\"form-check-label\" for=\"rememberMe\">Remember Me</label>\r\n                                </div>\r\n                                <button type=\"submit\" class=\"btn btn-primary w-100\">Login</button>\r\n                            </form>\r\n                            <div class=\"mt-3 text-center\">\r\n                                <a href=\"/forgot-password\">Forgot Password?</a><br />\r\n                            </div>\r\n                        </div>\r\n                    </div>", new DateTime(2025, 6, 8, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "portal-access", "Display", "Login", null, "Public" },
                    { 3, "\r\n                <div class=\"container mt-5 text-center\">\r\n                    <h2>You have been logged out</h2>\r\n                    <p>Thank you for visiting. See you again soon!</p>\r\n                    <a href=\"/\" class=\"btn btn-primary mt-3\">Return Home</a>\r\n                </div>", new DateTime(2025, 6, 8, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "portal-logout", "Display", "Logout", null, "Public" },
                    { 4, "\r\n                    <div class=\"container mt-5\">\r\n                        <h2 class=\"mb-4\">Register</h2>\r\n                        <form action=\"/Identity/Account/Register\" method=\"post\">\r\n                            <div class=\"mb-3\">\r\n                                <label for=\"Input_Email\" class=\"form-label\">Email address</label>\r\n                                <input type=\"email\" class=\"form-control\" id=\"Input_Email\" name=\"Input.Email\" required />\r\n                            </div>\r\n                            <div class=\"mb-3\">\r\n                                <label for=\"Input_Password\" class=\"form-label\">Password</label>\r\n                                <input type=\"password\" class=\"form-control\" id=\"Input_Password\" name=\"Input.Password\" required />\r\n                            </div>\r\n                            <div class=\"mb-3\">\r\n                                <label for=\"Input_ConfirmPassword\" class=\"form-label\">Confirm Password</label>\r\n                                <input type=\"password\" class=\"form-control\" id=\"Input_ConfirmPassword\" name=\"Input.ConfirmPassword\" required />\r\n                            </div>\r\n                            <button type=\"submit\" class=\"btn btn-primary\">Register</button>\r\n                        </form>\r\n                        <div class=\"mt-3\">\r\n                            <a href=\"/Identity/Account/Login\">Already have an account? Login here</a>\r\n                        </div>\r\n                    </div>", new DateTime(2025, 6, 8, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "register", "Display", "Register", null, "Public" },
                    { 5, "\r\n                <div class=\"container mt-5\">\r\n                    <h2 class=\"mb-4\">Forgot your password?</h2>\r\n                    <form action=\"/Identity/Account/ForgotPassword\" method=\"post\">\r\n                        <div class=\"mb-3\">\r\n                            <label for=\"Input_Email\" class=\"form-label\">Email address</label>\r\n                            <input type=\"email\" class=\"form-control\" id=\"Input_Email\" name=\"Input.Email\" required />\r\n                        </div>\r\n                        <button type=\"submit\" class=\"btn btn-warning\">Send Password Reset Link</button>\r\n                    </form>\r\n                    <div class=\"mt-3\">\r\n                        <a href=\"/Identity/Account/Login\">Back to Login</a>\r\n                    </div>\r\n                </div>", new DateTime(2025, 6, 8, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "forgot-password", "Display", "Forgot Password", null, "Public" },
                    { 6, "\r\n                <div class=\"container mt-5\">\r\n                    <h2 class=\"mb-4\">Reset your password</h2>\r\n                    <form action=\"/Identity/Account/ResetPassword\" method=\"post\">\r\n                        <div class=\"mb-3\">\r\n                            <label for=\"Input_Email\" class=\"form-label\">Email address</label>\r\n                            <input type=\"email\" class=\"form-control\" id=\"Input_Email\" name=\"Input.Email\" required />\r\n                        </div>\r\n                        <div class=\"mb-3\">\r\n                            <label for=\"Input_Password\" class=\"form-label\">New Password</label>\r\n                            <input type=\"password\" class=\"form-control\" id=\"Input_Password\" name=\"Input.Password\" required />\r\n                        </div>\r\n                        <div class=\"mb-3\">\r\n                            <label for=\"Input_ConfirmPassword\" class=\"form-label\">Confirm New Password</label>\r\n                            <input type=\"password\" class=\"form-control\" id=\"Input_ConfirmPassword\" name=\"Input.ConfirmPassword\" required />\r\n                        </div>\r\n                        <button type=\"submit\" class=\"btn btn-success\">Reset Password</button>\r\n                    </form>\r\n                </div>", new DateTime(2025, 6, 8, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "reset-password", "Display", "Reset Password", null, "Public" },
                    { 7, "\r\n                    <div class=\"container mt-5\">\r\n                    <h2 class=\"mb-4\">Change your password</h2>\r\n                    <form action=\"/Identity/Account/Manage/ChangePassword\" method=\"post\">\r\n                        <div class=\"mb-3\">\r\n                            <label for=\"Input_OldPassword\" class=\"form-label\">Current Password</label>\r\n                            <input type=\"password\" class=\"form-control\" id=\"Input_OldPassword\" name=\"Input.OldPassword\" required />\r\n                        </div>\r\n                        <div class=\"mb-3\">\r\n                            <label for=\"Input_NewPassword\" class=\"form-label\">New Password</label>\r\n                            <input type=\"password\" class=\"form-control\" id=\"Input_NewPassword\" name=\"Input.NewPassword\" required />\r\n                        </div>\r\n                        <div class=\"mb-3\">\r\n                            <label for=\"Input_ConfirmPassword\" class=\"form-label\">Confirm New Password</label>\r\n                            <input type=\"password\" class=\"form-control\" id=\"Input_ConfirmPassword\" name=\"Input.ConfirmPassword\" required />\r\n                        </div>\r\n                        <button type=\"submit\" class=\"btn btn-success\">Change Password</button>\r\n                    </form>\r\n                </div>", new DateTime(2025, 6, 8, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "change-password", "Display", "Change Password", null, "Public" },
                    { 8, "<p>This is a secure test page. You may upload a file here to verify access control.</p>", new DateTime(2025, 6, 8, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "secure-test", "Display", "Secure Test Page", null, "Public" }
                });

            migrationBuilder.InsertData(
                table: "SecurityLevels",
                columns: new[] { "Id", "Description", "IsSystem", "Name" },
                values: new object[,]
                {
                    { 1, "Can modify site settings.", true, "Administrator" },
                    { 2, "Registered user for protected pages.", true, "User" },
                    { 3, "Public access level.", true, "Anonymous" }
                });

            migrationBuilder.InsertData(
                table: "SocialMediaPlatforms",
                columns: new[] { "Id", "IconClass", "Name" },
                values: new object[,]
                {
                    { 1, "fab fa-facebook-f", "Facebook" },
                    { 2, "fab fa-twitter", "Twitter" },
                    { 3, "fab fa-instagram", "Instagram" },
                    { 4, "fab fa-youtube", "YouTube" },
                    { 5, "fab fa-linkedin-in", "LinkedIn" },
                    { 6, "fab fa-tiktok", "TikTok" },
                    { 7, "fab fa-pinterest-p", "Pinterest" },
                    { 8, "fas fa-globe", "Bluesky" }
                });

            migrationBuilder.InsertData(
                table: "ThemeSettings",
                columns: new[] { "Id", "Description", "DisplayName", "Favicon", "Name", "PreviewImage", "SetOn" },
                values: new object[] { 1, "Clean layout with light styling", "Default Theme", "favicon.ico", "default", "/Themes/Default/preview.png", new DateTime(2025, 6, 8, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.InsertData(
                table: "MenuItems",
                columns: new[] { "Id", "IsVisible", "MenuGroup", "Order", "PageContentId", "ParentId", "SecurityLevelId", "Title", "Url" },
                values: new object[,]
                {
                    { 1, true, "Main", 9999, null, null, 1, "Admin", "#" },
                    { 2, true, "Main", 0, null, 1, 1, "Site Settings", "/admin/settings" },
                    { 3, true, "Main", 1, null, 1, 1, "Social Media", "/admin/socialmedia" },
                    { 4, true, "Main", 2, null, 1, 1, "Menu/Page Editor", "/admin/navcontent" },
                    { 5, true, "Main", 1, 8, null, 2, "Secure Test", null },
                    { 100, true, "Main", 0, 1, null, 3, "Home", null }
                });

            migrationBuilder.InsertData(
                table: "SecurityLevelRoles",
                columns: new[] { "Id", "RoleName", "SecurityLevelId" },
                values: new object[,]
                {
                    { 1, "Administrator", 1 },
                    { 2, "User", 2 }
                });

            migrationBuilder.InsertData(
                table: "SiteSettings",
                columns: new[] { "Id", "ContactAddress", "ContactEmail", "ContactPhone", "Copyright", "Logo", "SiteName", "Tagline", "ThemeId" },
                values: new object[] { 1, null, null, null, null, null, "SCMS Site", "Powered by SCMS", 1 });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_PageContentId",
                table: "MenuItems",
                column: "PageContentId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_SecurityLevelId",
                table: "MenuItems",
                column: "SecurityLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityLevelRoles_SecurityLevelId",
                table: "SecurityLevelRoles",
                column: "SecurityLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_SiteSettings_ThemeId",
                table: "SiteSettings",
                column: "ThemeId");

            migrationBuilder.CreateIndex(
                name: "IX_SiteSocialLinks_SiteSettingsId",
                table: "SiteSocialLinks",
                column: "SiteSettingsId");

            migrationBuilder.CreateIndex(
                name: "IX_SiteSocialLinks_SocialMediaPlatformId",
                table: "SiteSocialLinks",
                column: "SocialMediaPlatformId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "MenuItems");

            migrationBuilder.DropTable(
                name: "SecurityLevelRoles");

            migrationBuilder.DropTable(
                name: "SiteSocialLinks");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "PageContents");

            migrationBuilder.DropTable(
                name: "SecurityLevels");

            migrationBuilder.DropTable(
                name: "SiteSettings");

            migrationBuilder.DropTable(
                name: "SocialMediaPlatforms");

            migrationBuilder.DropTable(
                name: "ThemeSettings");
        }
    }
}
