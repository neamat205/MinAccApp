using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using MinAccApp.Models;
using System.Diagnostics;


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
    }
}
