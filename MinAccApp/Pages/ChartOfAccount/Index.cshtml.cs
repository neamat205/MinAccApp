using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Data;
using MinAccApp.Models;


namespace MinAccApp.Pages.ChartOfAccount
{
    public class IndexModel : PageModel
    {
        private readonly IConfiguration _config;
        private readonly IPermissionService _permissionService;
        public IndexModel(IConfiguration config, IPermissionService permissionService)
        {
            _config = config;
            _permissionService = permissionService;
        }
     

        [BindProperty] public int Id { get; set; }
        [BindProperty] public string Name { get; set; }
        [BindProperty] public int? ParentId { get; set; }
        public string ErrorMessage { get; set; }
        public bool IsEdit { get; set; }
        public List<ChartOfAccountModel> FlatChartOfAccount { get; set; } = new();
        public List<ChartOfAccountModel> Hierarchy { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var canView = await _permissionService.HasPermissionAsync(User, "ChartOfAccount", "View");

            if (!canView)
            {
                ModelState.AddModelError(string.Empty, "You are not allowed to access this page.");

                return Redirect("/Index");
            }


            await LoadChartOfAccountModels();
            return Page();
        }

        public async Task<IActionResult> OnGetEditAsync(int id)
        {
            var canView = await _permissionService.HasPermissionAsync(User, "ChartOfAccount", "Update");

            if (!canView)
            {
                
                ErrorMessage = "You are not allowed to perform this operation.";

                await LoadChartOfAccountModels();
                return Page(); 

            }

            await LoadChartOfAccountModels();
            var acc = FlatChartOfAccount.FirstOrDefault(a => a.Id == id);
            if (acc != null)
            {
                Id = acc.Id;
                Name = acc.Name;
                ParentId = acc.ParentId;
                IsEdit = true;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            var canView = await _permissionService.HasPermissionAsync(User, "ChartOfAccount", "Create");

            if (!canView)
            {
                ErrorMessage = "You are not allowed to perform this operation.";

                await LoadChartOfAccountModels();
                return Page(); 

            }

            await ExecuteSP("CREATE", Name, ParentId);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateAsync()
        {
            var canView = await _permissionService.HasPermissionAsync(User, "ChartOfAccount", "Update");

            if (!canView)
            {
                ErrorMessage = "You are not allowed to perform this operation.";

                await LoadChartOfAccountModels();
                return Page(); 
            }

            await ExecuteSP("UPDATE", Name, ParentId, Id);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnGetDeleteAsync(int id)
        {
            var canDelete = await _permissionService.HasPermissionAsync(User, "ChartOfAccount", "Delete");
            if (!canDelete)
            {
                ErrorMessage = "You are not allowed to perform this operation.";
                await LoadChartOfAccountModels();
                return Page();
            }

            var (success, error) = await ExecuteSP("DELETE", null, null, id);
            if (!success)
            {
                ErrorMessage = error;
                await LoadChartOfAccountModels();
                return Page();
            }

            return RedirectToPage();
        }


        private async Task<(bool success, string errorMessage)> ExecuteSP(string action, string? name, int? parentId, int? id = null)
        {
            using var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            await conn.OpenAsync();

            if (action == "DELETE" && id.HasValue)
            {
                
                using var checkCmd = new SqlCommand("SELECT COUNT(*) FROM VoucherEntry WHERE AccountId = @AccountId", conn);
                checkCmd.Parameters.AddWithValue("@AccountId", id.Value);

                int count = (int)await checkCmd.ExecuteScalarAsync();

                if (count > 0)
                {
                    return (false, "Cannot delete this account because it is referenced by existing voucher entries.");
                }
            }

            using var cmd = new SqlCommand("sp_ManageChartOfAccounts", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@Action", action);
            cmd.Parameters.AddWithValue("@Id", (object?)id ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Name", (object?)name ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ParentId", (object?)parentId ?? DBNull.Value);

            await cmd.ExecuteNonQueryAsync();

            return (true, string.Empty);
        }


        private async Task LoadChartOfAccountModels()
        {
            var all = new List<ChartOfAccountModel>();

            using var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            using var cmd = new SqlCommand("sp_ManageChartOfAccounts", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@Action", "READ");

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                all.Add(new ChartOfAccountModel
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    ParentId = reader.IsDBNull(2) ? null : reader.GetInt32(2)
                });
            }

            FlatChartOfAccount = BuildFlat(all);
            Hierarchy = BuildTree(all);
        }

        private List<ChartOfAccountModel> BuildTree(List<ChartOfAccountModel> all)
        {
            var lookup = all.ToLookup(x => x.ParentId);
            foreach (var acc in all)
                acc.Children = lookup[acc.Id].ToList();
            return lookup[null].ToList();
        }

        private List<ChartOfAccountModel> BuildFlat(List<ChartOfAccountModel> all)
        {
            var flat = new List<ChartOfAccountModel>();
            void Flatten(ChartOfAccountModel acc, string prefix)
            {
                flat.Add(new ChartOfAccountModel { Id = acc.Id, Name = prefix + acc.Name });
                foreach (var child in acc.Children)
                    Flatten(child, prefix + "--");
            }

            var tree = BuildTree(all);
            foreach (var acc in tree)
                Flatten(acc, "");
            return flat;
        }
    }

}
