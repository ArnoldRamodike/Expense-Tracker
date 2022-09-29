using Expense_Tracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Expense_Tracker.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ActionResult> Index()
        {
            //Last 7 Days
            DateTime StartDate = DateTime.Today;
            DateTime EndDatae = DateTime.Today.AddDays(+30);

            List<Transaction> selectedTransaction = await _context.Transactions
                .Include(x => x.Category)
                .Where(y => y.Date >= StartDate && y.Date <= EndDatae)
                .ToListAsync();

            //Total Income
            int TotalIncome = selectedTransaction
                .Where(i => i.Category.Type =="Income")
                .Sum(j => j.Amount);
                ViewBag.TotalIncome = TotalIncome.ToString("C0");

            //Total Expense
            int TotalExpense = selectedTransaction
                .Where(i => i.Category.Type == "Expense")
                .Sum(j => j.Amount);
            ViewBag.TotalExpense = TotalExpense.ToString("C0");

            //Balance
            int Balance = TotalIncome - TotalExpense;
            CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");
            culture.NumberFormat.CurrencyNegativePattern = 1;
            ViewBag.Balance = String.Format(culture, "{0:C0}", Balance);

            //Expenses by Category
            ViewBag.DoughnutChartData = selectedTransaction
            .Where(i => i.Category.Type == "Expense")
            .GroupBy(j => j.Category.CategoryId)
            .Select(k => new
            {
                categoryTitleWithIcon = k.First().Category.Icon + " " + k.First().Category.Title,
                amount = k.Sum(j => j.Amount),
                formattedAmount = k.Sum(j => j.Amount).ToString("C0"),
            })
            .ToList();
            //SpineLine Income vs Expenses
            //Income
            List<SpinelineData> incomeSummury = selectedTransaction
                .Where(i => i.Category.Type == "Income")
                .GroupBy(j => j.Date)
                .Select(k => new SpinelineData()
                {
                    Day = k.First().Date.ToString("dd-MMM"),
                    income = k.Sum(l => l.Amount)
                })
                .ToList();

            //Expense
            List<SpinelineData> expenseSummury = selectedTransaction
                .Where(i => i.Category.Type == "Expense")
                .GroupBy(j => j.Date)
                .Select(k => new SpinelineData()
                {
                    Day = k.First().Date.ToString("dd-MMM"),
                    expense = k.Sum(l => l.Amount)
                })
                .ToList();

            //comination
            string[] Last7Days = Enumerable.Range(0, 7)
                .Select(i => StartDate.AddDays(i).ToString("dd-MMM"))
                .ToArray();

            ViewBag.SpinelineData = from day in Last7Days
                                    join income in incomeSummury on day equals income.Day into dayIcomeJoined
                                    from income in dayIcomeJoined.DefaultIfEmpty()
                                    join expense in expenseSummury on day equals expense.Day into expenseJoined
                                    from expense in expenseJoined.DefaultIfEmpty()
                                    select new
                                    {
                                        day = day,
                                        income = income == null ? 0 : income.income,
                                        expense = expense == null ? 0 : expense.expense,
                                    };

            //Recent Transactions
            ViewBag.RecentTransactions = await _context.Transactions
                .Include(i => i.Category)
                .OrderByDescending(j => j.Date)
                .Take(5)
                .ToListAsync();

            return View();
        }
    }
    public class SpinelineData
    {
        public string Day;
        public int income;
        public int expense;
    }
}
