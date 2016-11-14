using Budget.DAL;
using Budget.DAL.Models;
using Moq;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System;

namespace Budget.API.Tests.FakesAndMocks
{
    public class MockDbContext
    {
        public Mock<IApplicationDbContext> Context { get; set; }
        public Mock<DbSet<AccountModel>> Accounts { get; set; }
        public Mock<DbSet<BalanceModel>> Balances { get; set; }
        public Mock<DbSet<CategoryModel>> Categories { get; set; }
        public Mock<DbSet<FinancialInstitutionModel>> FinancialInstitutions { get; set; }
        public Mock<DbSet<PayeeModel>> Payees { get; set; }
        public Mock<DbSet<TransactionModel>> Transactions { get; set; }

        public MockDbContext()
        {
            Context = new Mock<IApplicationDbContext>();
        }

        public IApplicationDbContext GetMock()
        {
            // Setup gets to DbSets
            Finalize();

            // return mock
            return Context.Object;
        }

        public MockDbContext Finalize()
        {
            Context.SetupGet(x => x.Accounts).Returns(Accounts?.Object);
            Context.SetupGet(x => x.Balances).Returns(Balances?.Object);
            Context.SetupGet(x => x.Categories).Returns(Categories?.Object);
            Context.SetupGet(x => x.FinancialInstitutions).Returns(FinancialInstitutions?.Object);
            Context.SetupGet(x => x.Payees).Returns(Payees?.Object);
            Context.SetupGet(x => x.Transactions).Returns(Transactions?.Object);
            return this;
        }

        public MockDbContext WithData(List<FinancialInstitutionModel> data)
        {
            FinancialInstitutions = new MockDbSet<FinancialInstitutionModel>()
                .UsingDataSet(data.AsQueryable())
                .Mock();
            return this;
        }

        public MockDbContext WithData(List<AccountModel> data)
        {
            Accounts = new MockDbSet<AccountModel>()
                .UsingDataSet(data.AsQueryable())
                .Mock();
            return this;
        }

        public MockDbContext SetupAdd(FinancialInstitutionModel add, FinancialInstitutionModel returns)
        {
            FinancialInstitutions.Setup(x => x.Add(It.Is<FinancialInstitutionModel>(fi => fi.OfxFid == add.OfxFid))).Returns(returns);
            return this;
        }

        public MockDbContext SetupFind(int id, FinancialInstitutionModel returns)
        {
            FinancialInstitutions.Setup(x => x.Find(id)).Returns(returns);
            return this;
        }

        public MockDbContext SetupSaveChanges(int returns)
        {
            Context.Setup(x => x.SaveChanges()).Returns(returns);
            return this;
        }

    }
}