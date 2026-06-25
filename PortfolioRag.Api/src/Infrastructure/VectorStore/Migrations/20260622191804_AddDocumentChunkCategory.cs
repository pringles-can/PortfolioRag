using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortfolioRag.Api.Infrastructure.VectorStore.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentChunkCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "document_chunks",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_document_chunks_Category",
                table: "document_chunks",
                column: "Category");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_document_chunks_Category",
                table: "document_chunks");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "document_chunks");
        }
    }
}
