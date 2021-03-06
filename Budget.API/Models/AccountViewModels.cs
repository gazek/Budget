﻿using Budget.DAL.Models;
using System;

namespace Budget.API.Models
{
    public class AccountViewModel
    {
        public int Id { get; set; }
        public int FinancialInstitutionId { get; set; }
        public int RoutingNumber { get; set; }
        public string Number { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public decimal StartBalance { get; set; }
        public BalanceViewModel Balance { get; set; }
        public TransactionSummaryViewModel TransactionSummary { get; set; }
    }

    public class AccountListViewModel
    {
        public int FinancialInstitutionId { get; set; }
        public string Number { get; set; }
        public string Name { get; set; }
        public AccountType Type { get; set; }
        public int RoutingNumber { get; set; }
    }
}