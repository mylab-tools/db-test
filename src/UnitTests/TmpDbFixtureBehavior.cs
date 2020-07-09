using System;
using System.Threading.Tasks;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Mapping;
using MyLab.DbTest;
using Xunit;
using Xunit.Abstractions;

namespace UnitTests
{
    public class TmpDbFixtureBehavior : IClassFixture<TmpDbFixture<TmpDbFixtureBehavior.TestDbInitializer>>
    {
        private readonly TmpDbFixture _fxt;

        public TmpDbFixtureBehavior(ITestOutputHelper outputHelper, TmpDbFixture<TestDbInitializer> fxt)
        {
            fxt.Output = outputHelper;
            _fxt = fxt;
        }

        [Fact]
        public async Task ShouldContainsPredefinedEntities()
        {
            //Arrange
            var mgr = await _fxt.CreateDbAsync();

            //Act
            var entities = await mgr.DoOnce().GetTable<Entity>().ToArrayAsync();

            //Assert
            Assert.Equal(2, entities.Length);
            Assert.Equal(1, entities[0].Id);
            Assert.Equal("foo", entities[0].Value);
            Assert.Equal(2, entities[1].Id);
            Assert.Equal("bar", entities[1].Value);
        }

        [Fact]
        public async Task ShouldContainsPredefinedWhenAddInitializer()
        {
            //Arrange
            var additionalInitializer= new AdditionalTestDbInitializer();
            var mgr = await _fxt.CreateDbAsync(additionalInitializer);

            //Act
            var entities = await mgr.DoOnce().GetTable<Entity>().ToArrayAsync();

            //Assert
            Assert.Equal(4, entities.Length);
            Assert.Equal(1, entities[0].Id);
            Assert.Equal("foo", entities[0].Value);
            Assert.Equal(2, entities[1].Id);
            Assert.Equal("bar", entities[1].Value);
        }

        [Fact]
        public async Task ShouldContainsAdditionalWhenAddInitializer()
        {
            //Arrange
            var additionalInitializer = new AdditionalTestDbInitializer();
            var mgr = await _fxt.CreateDbAsync(additionalInitializer);

            //Act
            var entities = await mgr.DoOnce().GetTable<Entity>().ToArrayAsync();

            //Assert
            Assert.Equal(4, entities.Length);
            Assert.Equal(3, entities[2].Id);
            Assert.Equal("baz", entities[2].Value);
            Assert.Equal(4, entities[3].Id);
            Assert.Equal("qux", entities[3].Value);
        }

        public class TestDbInitializer : ITestDbInitializer
        {
            public async Task InitializeAsync(DataConnection dataConnection)
            {
                var t = await dataConnection.CreateTableAsync<Entity>();
                await t.InsertAsync(() => new Entity { Value = "foo" });
                await t.InsertAsync(() => new Entity { Value = "bar" });
            }
        }

        public class AdditionalTestDbInitializer : ITestDbInitializer
        {
            public async Task InitializeAsync(DataConnection dataConnection)
            {
                var t = dataConnection.GetTable<Entity>();
                await t.InsertAsync(() => new Entity { Value = "baz" });
                await t.InsertAsync(() => new Entity { Value = "qux" });
            }
        }

        [Table("FooTable")]
        class Entity
        {
            [PrimaryKey, Identity]
            [Column]
            public int Id { get; set; }

            [Column]
            public string Value { get; set; }
        }
    }

    
}
