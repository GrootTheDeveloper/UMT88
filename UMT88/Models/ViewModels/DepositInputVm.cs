// ViewModels/DepositInputVm.cs
using System.ComponentModel.DataAnnotations;

namespace UMT88.ViewModels
{
    public class DepositInputVm
    {
        [Display(Name = "Số tiền muốn nạp (VND)")]
        [Required, Range(1000, double.MaxValue, ErrorMessage = "Phải ít nhất 1 000 VND")]
        public decimal AmountVnd { get; set; }

        [Display(Name = "Ngân hàng")]
        [Required]
        public string Bank { get; set; } = "";
    }
}
