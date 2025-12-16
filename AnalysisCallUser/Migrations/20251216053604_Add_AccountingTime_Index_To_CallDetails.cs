using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnalysisCallUser.Migrations
{
    /// <inheritdoc />
    public partial class Add_AccountingTime_Index_To_CallDetails : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        { 
            // ایندکس روی ستون AccountingTime برای افزایش سرعت جستجو بر اساس تاریخ
            migrationBuilder.CreateIndex(
                name: "IX_CallDetails_AccountingTime",
                table: "CallDetails",
                column: "AccountingTime");

            // ایندکس‌های تک ستونی برای فیلترهای دیگر
            migrationBuilder.CreateIndex(
                name: "IX_CallDetails_ANumber",
                table: "CallDetails",
                column: "ANumber");

            migrationBuilder.CreateIndex(
                name: "IX_CallDetails_BNumber",
                table: "CallDetails",
                column: "BNumber");

      

            migrationBuilder.CreateIndex(
                name: "IX_CallDetails_AccountingTime_SH",
                table: "CallDetails",
                column: "AccountingTime_SH");

            migrationBuilder.CreateIndex(
                name: "IX_CallDetails_Answer",
                table: "CallDetails",
                column: "Answer");

       
            // ایندکس‌های ترکیبی (Composite) برای بهینه‌سازی فیلترهای مبدأ و مقصد
            migrationBuilder.CreateIndex(
                name: "IX_CallDetails_Origin_Composite",
                table: "CallDetails",
                columns: new[] { "OriginCountryID", "OriginCityID", "OriginOperatorID" });

            migrationBuilder.CreateIndex(
                name: "IX_CallDetails_Dest_Composite",
                table: "CallDetails",
                columns: new[] { "DestCountryID", "DestCityID", "DestOperatorID" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CallDetails_Dest_Composite",
                table: "CallDetails");

            migrationBuilder.DropIndex(
                name: "IX_CallDetails_Origin_Composite",
                table: "CallDetails");

          

            migrationBuilder.DropIndex(
                name: "IX_CallDetails_Answer",
                table: "CallDetails");

            migrationBuilder.DropIndex(
                name: "IX_CallDetails_AccountingTime_SH",
                table: "CallDetails");

        
            migrationBuilder.DropIndex(
                name: "IX_CallDetails_BNumber",
                table: "CallDetails");

            migrationBuilder.DropIndex(
                name: "IX_CallDetails_ANumber",
                table: "CallDetails");

            migrationBuilder.DropIndex(
                name: "IX_CallDetails_AccountingTime",
                table: "CallDetails");
        }
    }
}
