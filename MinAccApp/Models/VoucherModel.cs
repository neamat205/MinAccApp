using System.ComponentModel.DataAnnotations;

namespace MinAccApp.Models
{
    public class VoucherModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Voucher type is required.")]
        [Display(Name = "Voucher Type")]
        public string VoucherType { get; set; } = "Journal"; // Default

        [Required(ErrorMessage = "Date is required.")]
        [DataType(DataType.Date)]
        public DateTime? Date { get; set; } = DateTime.Today;

        [Display(Name = "Reference No.")]
        public string? ReferenceNo { get; set; }

        [Required(ErrorMessage = "At least one voucher entry is required.")]
        public List<VoucherEntryModel> Entries { get; set; } = new List<VoucherEntryModel>();
    }
}
