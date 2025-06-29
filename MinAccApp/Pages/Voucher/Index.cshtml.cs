
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using MinAccApp.Models;
using OfficeOpenXml;

namespace MinAccApp.Pages.Voucher
{
    public class IndexModel : PageModel
    {
        private readonly IConfiguration _config;
        private readonly IPermissionService _permissionService;

        public List<VoucherModel> SavedVouchers { get; set; } = new();
        public Dictionary<int, string> AccountTypeMap { get; set; } = new();

        public IndexModel(IConfiguration config, IPermissionService permissionService)
        {
            _config = config;
            _permissionService = permissionService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var canView = await _permissionService.HasPermissionAsync(User, "Voucher", "View");

            if (!canView)
            {
                return Redirect("/Index");
            }

            await LoadVouchersAsync();
            await LoadAccountTypesAsync();
            await LoadEntriesForVouchersAsync();

            return Page();
        }

        private async Task LoadVouchersAsync()
        {
            using var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            await conn.OpenAsync();

            using var cmd = new SqlCommand("SELECT Id, VoucherType, Date, ReferenceNo FROM Voucher ORDER BY Date DESC", conn);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                SavedVouchers.Add(new VoucherModel
                {
                    Id = reader.GetInt32(0),
                    VoucherType = reader.GetString(1),
                    Date = reader.GetDateTime(2),
                    ReferenceNo = reader.IsDBNull(3) ? null : reader.GetString(3),
                    Entries = new List<VoucherEntryModel>()
                });
            }
        }

        private async Task LoadAccountTypesAsync()
        {
            using var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            using var cmd = new SqlCommand("SELECT Id, Name FROM ChartOfAccounts", conn);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                int accountId = reader.GetInt32(0);
                string accountType = reader.IsDBNull(1) ? "Unknown" : reader.GetString(1);

                AccountTypeMap[accountId] = accountType;
            }
        }

        private async Task LoadEntriesForVouchersAsync()
        {
            using var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            await conn.OpenAsync();

            foreach (var voucher in SavedVouchers)
            {
                using var cmd = new SqlCommand(@"
                        SELECT AccountId, Debit, Credit 
                        FROM VoucherEntry 
                        WHERE VoucherId = @VoucherId", conn);

                cmd.Parameters.AddWithValue("@VoucherId", voucher.Id);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var debit = reader.GetDecimal(1);
                    var credit = reader.GetDecimal(2);

                    voucher.Entries.Add(new VoucherEntryModel
                    {
                        AccountId = reader.GetInt32(0),
                        EntryType = debit > 0 ? "Debit" : "Credit",
                        Amount = debit > 0 ? debit : credit
                    });
                }

                reader.Close();
            }
        }

        public async Task<IActionResult> OnPostExportAsync()
        {
            var canView = await _permissionService.HasPermissionAsync(User, "Voucher", "View");
            if (!canView)
                return Redirect("/Index");

            await LoadVouchersAsync();
            await LoadAccountTypesAsync();
            await LoadEntriesForVouchersAsync();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var stream = new MemoryStream();

            using (var package = new ExcelPackage())
            {
                var sheet = package.Workbook.Worksheets.Add("Voucher List");

              
                sheet.Cells[1, 1].Value = "Voucher Type";
                sheet.Cells[1, 2].Value = "Date";
                sheet.Cells[1, 3].Value = "Reference No";
                sheet.Cells[1, 4].Value = "Account Type";
                sheet.Cells[1, 5].Value = "Entry Type";
                sheet.Cells[1, 6].Value = "Amount";

                int row = 2;

                foreach (var voucher in SavedVouchers)
                {
                    foreach (var entry in voucher.Entries)
                    {
                        sheet.Cells[row, 1].Value = voucher.VoucherType;
                        sheet.Cells[row, 2].Value = voucher.Date?.ToString("yyyy-MM-dd");
                        sheet.Cells[row, 3].Value = voucher.ReferenceNo;

               
                        if (AccountTypeMap.TryGetValue(entry.AccountId, out var accountType))
                        {
                            sheet.Cells[row, 4].Value = accountType;
                        }
                        else
                        {
                            sheet.Cells[row, 4].Value = "Unknown";
                        }

                        sheet.Cells[row, 5].Value = entry.EntryType;
                        sheet.Cells[row, 6].Value = entry.Amount;

                        row++;
                    }
                }

                await package.SaveAsAsync(stream);
            }

            stream.Position = 0;

            return File(
                stream,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Vouchers.xlsx"
            );
        }




    }

}
