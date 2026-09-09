namespace Library.Service.Reports;

public sealed class LoanNotFoundException(int loanId)
    : Exception($"Loan {loanId} was not found.")
{
    public int LoanId { get; } = loanId;
}

public sealed class LoanNotReturnedException(int loanId)
    : Exception($"Loan {loanId} has not been returned yet.")
{
    public int LoanId { get; } = loanId;
}

public sealed class LoanInvalidDurationException(int loanId)
    : Exception($"Loan {loanId} must have a positive borrowing duration.")
{
    public int LoanId { get; } = loanId;
}