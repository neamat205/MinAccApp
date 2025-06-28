using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Data;
using MinAccApp.Models;

namespace MinAccApp.Pages.Voucher
{
    public class CreateModel : PageModel
    {
        private readonly IConfiguration _config;
        private readonly IPermissionService _permissionService;
        public CreateModel(IConfiguration config, IPermissionService permissionService)
        {
            _config = config;
            _permissionService = permissionService;
        }

        [BindProperty]
        public VoucherModel Voucher { get; set; } = new VoucherModel();

        public List<ChartOfAccountModel> ChartOfAccounts { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var canView = await _permissionService.HasPermissionAsync(User, "Voucher", "View");

            if (!canView)
            {

                return Redirect("/Index");
            }
            await LoadChartOfAccountsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var canCreate = await _permissionService.HasPermissionAsync(User, "Voucher", "Create");

            if (!canCreate)
            {

                return Redirect("/Index");
            }
            await LoadChartOfAccountsAsync();

            if (!ModelState.IsValid)
                return Page();

            if (Voucher.Entries == null || Voucher.Entries.Count == 0)
            {
                ModelState.AddModelError("", "At least one voucher entry is required.");
                return Page();
            }

            decimal totalDebit = 0m, totalCredit = 0m;

            foreach (var entry in Voucher.Entries)
            {
                if (entry.Amount <= 0)
                {
                    ModelState.AddModelError("", "Amount must be greater than zero.");
                    return Page();
                }

                if (entry.EntryType == "Debit")
                {
                    totalDebit += entry.Amount;
                }
                else if (entry.EntryType == "Credit")
                {
                    totalCredit += entry.Amount;
                }
                else
                {
                    ModelState.AddModelError("", "Entry type must be Debit or Credit.");
                    return Page();
                }
            }

            if (totalDebit != totalCredit)
            {
                ModelState.AddModelError("", "Total Debit and Credit must be equal.");
                return Page();
            }

            await SaveVoucherAsync(Voucher);

            // Redirect to list after successful save
            return RedirectToPage("Index");
        }

        private async Task LoadChartOfAccountsAsync()
        {
            ChartOfAccounts = new List<ChartOfAccountModel>();
            using var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            using var cmd = new SqlCommand("SELECT Id, Name FROM ChartOfAccounts ORDER BY Name", conn);
            await conn.OpenAsync();

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                ChartOfAccounts.Add(new ChartOfAccountModel
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1)
                });
            }
        }

        private async Task<IActionResult> SaveVoucherAsync(VoucherModel voucher)
        {
            
            using var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            using var cmd = new SqlCommand("sp_SaveVoucher", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@VoucherType", voucher.VoucherType);
            cmd.Parameters.AddWithValue("@Date", voucher.Date);
            cmd.Parameters.AddWithValue("@ReferenceNo", (object?)voucher.ReferenceNo ?? DBNull.Value);

            var entriesTable = CreateVoucherEntriesDataTable(voucher.Entries);
            var entryParam = cmd.Parameters.AddWithValue("@Entries", entriesTable);
            entryParam.SqlDbType = SqlDbType.Structured;
            entryParam.TypeName = "VoucherEntryType";

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            return Page();
        }

        private DataTable CreateVoucherEntriesDataTable(List<VoucherEntryModel> entries)
        {
            var table = new DataTable();
            table.Columns.Add("AccountId", typeof(int));
            table.Columns.Add("Debit", typeof(decimal));
            table.Columns.Add("Credit", typeof(decimal));

            foreach (var entry in entries)
            {
                decimal debit = 0, credit = 0;

                if (entry.EntryType == "Debit")
                    debit = entry.Amount;
                else if (entry.EntryType == "Credit")
                    credit = entry.Amount;

                table.Rows.Add(entry.AccountId, debit, credit);
            }

            return table;
        }
    }
}
