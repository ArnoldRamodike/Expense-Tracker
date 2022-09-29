using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Expense_Tracker.Models
{
    public class Transaction
    {
        [Key]
        public int TransactionId {get; set;}

        //CategoryId
        [Range(1,int.MaxValue,ErrorMessage ="Please Select a Category")]
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Amount Should be aGreater than 0.")]
        public int Amount { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;

        [Column(TypeName = "nvarchar(75)")]
        public string? Note { get; set; }

        [NotMapped]
        public string? CategoryTitleWithIcon
        {
            get
            {
                return Category == null? "" : Category.Icon + " " + Category.Title;
            }
        }
        [NotMapped]
        public string? FormatAmount
        {
            get
            {
                return ((Category == null || Category.Type == "Expense") ? "- ": "+ ") + Amount.ToString("C0");
            }
        }
    }
}
