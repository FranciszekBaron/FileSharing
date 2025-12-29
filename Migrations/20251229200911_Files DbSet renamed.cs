using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileSharing.Migrations
{
    /// <inheritdoc />
    public partial class FilesDbSetrenamed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Files_Files_ParentId",
                table: "Files");

            migrationBuilder.DropForeignKey(
                name: "FK_Files_Users_OwnerId",
                table: "Files");

            migrationBuilder.DropForeignKey(
                name: "FK_FilesAccesess_Files_FileItemId",
                table: "FilesAccesess");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Files",
                table: "Files");

            migrationBuilder.RenameTable(
                name: "Files",
                newName: "FileItems");

            migrationBuilder.RenameIndex(
                name: "IX_Files_Starred",
                table: "FileItems",
                newName: "IX_FileItems_Starred");

            migrationBuilder.RenameIndex(
                name: "IX_Files_ParentId",
                table: "FileItems",
                newName: "IX_FileItems_ParentId");

            migrationBuilder.RenameIndex(
                name: "IX_Files_OwnerId",
                table: "FileItems",
                newName: "IX_FileItems_OwnerId");

            migrationBuilder.RenameIndex(
                name: "IX_Files_ModifiedDate",
                table: "FileItems",
                newName: "IX_FileItems_ModifiedDate");

            migrationBuilder.RenameIndex(
                name: "IX_Files_Deleted",
                table: "FileItems",
                newName: "IX_FileItems_Deleted");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FileItems",
                table: "FileItems",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FileItems_FileItems_ParentId",
                table: "FileItems",
                column: "ParentId",
                principalTable: "FileItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FileItems_Users_OwnerId",
                table: "FileItems",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FilesAccesess_FileItems_FileItemId",
                table: "FilesAccesess",
                column: "FileItemId",
                principalTable: "FileItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FileItems_FileItems_ParentId",
                table: "FileItems");

            migrationBuilder.DropForeignKey(
                name: "FK_FileItems_Users_OwnerId",
                table: "FileItems");

            migrationBuilder.DropForeignKey(
                name: "FK_FilesAccesess_FileItems_FileItemId",
                table: "FilesAccesess");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FileItems",
                table: "FileItems");

            migrationBuilder.RenameTable(
                name: "FileItems",
                newName: "Files");

            migrationBuilder.RenameIndex(
                name: "IX_FileItems_Starred",
                table: "Files",
                newName: "IX_Files_Starred");

            migrationBuilder.RenameIndex(
                name: "IX_FileItems_ParentId",
                table: "Files",
                newName: "IX_Files_ParentId");

            migrationBuilder.RenameIndex(
                name: "IX_FileItems_OwnerId",
                table: "Files",
                newName: "IX_Files_OwnerId");

            migrationBuilder.RenameIndex(
                name: "IX_FileItems_ModifiedDate",
                table: "Files",
                newName: "IX_Files_ModifiedDate");

            migrationBuilder.RenameIndex(
                name: "IX_FileItems_Deleted",
                table: "Files",
                newName: "IX_Files_Deleted");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Files",
                table: "Files",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Files_Files_ParentId",
                table: "Files",
                column: "ParentId",
                principalTable: "Files",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Files_Users_OwnerId",
                table: "Files",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FilesAccesess_Files_FileItemId",
                table: "FilesAccesess",
                column: "FileItemId",
                principalTable: "Files",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
