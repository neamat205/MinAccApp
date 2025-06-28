using System.ComponentModel.DataAnnotations;

namespace MinAccApp.Models
{
    public class VoucherEntryModel
    {
        [Required(ErrorMessage = "Account is required.")]
        public int AccountId { get; set; }

        [Required(ErrorMessage = "Entry type is required.")]
        [RegularExpression("Debit|Credit", ErrorMessage = "Entry type must be 'Debit' or 'Credit'.")]
        public string EntryType { get; set; } = "Debit";

        [Required(ErrorMessage = "Amount is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
        public decimal Amount { get; set; }
   

        // These properties are computed dynamically (used only internally)
        public decimal Debit => EntryType == "Debit" ? Amount : 0m;
        public decimal Credit => EntryType == "Credit" ? Amount : 0m;
    }
}
