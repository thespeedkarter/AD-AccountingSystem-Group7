using AccountingSystem.Models;

namespace AccountingSystem.Services
{
    public static class AccountingMath
    {
        // Returns the signed effect on account balance given a debit/credit on that account.
        // Asset/Expense: Debit increases, Credit decreases
        // Liability/Equity/Revenue: Credit increases, Debit decreases
        public static decimal SignedImpact(NormalSide normalSide, decimal debit, decimal credit)
        {
            if (normalSide == NormalSide.Debit)
                return debit - credit;

            // Credit-normal accounts
            return credit - debit;
        }
    }
}
