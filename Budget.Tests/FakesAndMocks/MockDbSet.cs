using Moq;
using System.Data.Entity;
using System.Linq;

namespace Budget.API.Tests.FakesAndMocks
{
    class MockDbSet<T> where T : class
    {

        private IQueryable<T> DataSet;

        public MockDbSet<T> UsingDataSet(IQueryable<T> dataSet)
        {
            DataSet = dataSet;
            return this;
        }

        public Mock<DbSet<T>> Mock()
        {
            Mock<DbSet<T>> theMock = new Mock<DbSet<T>>();
            theMock.As<IQueryable<T>>().Setup(m => m.Provider).Returns(DataSet.Provider);
            theMock.As<IQueryable<T>>().Setup(m => m.Expression).Returns(DataSet.Expression);
            theMock.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(DataSet.ElementType);
            theMock.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(DataSet.GetEnumerator());
            return theMock;
        }
    }
}
