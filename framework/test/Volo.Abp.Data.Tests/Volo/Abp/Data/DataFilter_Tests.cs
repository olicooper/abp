using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;
using System.Threading.Tasks;
using Volo.Abp.Modularity;
using Volo.Abp.Testing;
using Xunit;

namespace Volo.Abp.Data
{
    public class DataFilter_Tests : AbpIntegratedTest<DataFilter_Tests.TestModule>
    {
        private readonly IDataFilter _dataFilter;
        private readonly AbpDataFilterOptions _dataFilterOptions;

        public DataFilter_Tests()
        {
            _dataFilter = ServiceProvider.GetRequiredService<IDataFilter>();
            _dataFilterOptions = ServiceProvider
                .GetRequiredService<IOptions<AbpDataFilterOptions>>().Value;
        }

        [Fact]
        public void Should_Allow_Default_Filter_State_To_Be_Updated()
        {
            _dataFilter.ReadOnlyFilters.ContainsKey(typeof(ISoftDelete)).ShouldBe(false);
            _dataFilter.ReadOnlyFilters.ContainsKey(typeof(IGenericTestInterface)).ShouldBe(false);

            _dataFilterOptions.DefaultFilterState = false;
            _dataFilter.IsEnabled<ISoftDelete>().ShouldBe(false);

            _dataFilterOptions.DefaultFilterState = true;
            _dataFilter.IsEnabled<IGenericTestInterface>().ShouldBe(true);
        }

        [Fact]
        public void Should_Override_Default_Filter_State_For_Single_Filter()
        {
            _dataFilterOptions.DefaultFilterState.ShouldBe(true);

            _dataFilterOptions.DefaultStates.ContainsKey(typeof(IGenericTestInterface)).ShouldBe(false);

            _dataFilterOptions.DefaultStates.Add(typeof(IGenericTestInterface), new DataFilterState(false));

            _dataFilter.IsEnabled<IGenericTestInterface>().ShouldBe(false);
            _dataFilter.IsEnabled<ISoftDelete>().ShouldBe(true);
        }

        [Fact]
        public void Should_Disable_Filter()
        {
            _dataFilter.IsEnabled<ISoftDelete>().ShouldBe(true);

            using (_dataFilter.Disable<ISoftDelete>())
            {
                _dataFilter.IsEnabled<ISoftDelete>().ShouldBe(false);
            }

            _dataFilter.IsEnabled<ISoftDelete>().ShouldBe(true);
        }

        [Fact]
        public async Task Should_Handle_Nested_Filters()
        {
            _dataFilter.IsEnabled<ISoftDelete>().ShouldBe(true);

            using (_dataFilter.Disable<ISoftDelete>())
            {
                await Task.Run(() =>
                {
                    using (_dataFilter.Enable<ISoftDelete>())
                    {
                        _dataFilter.IsEnabled<ISoftDelete>().ShouldBe(true);
                    }
                });

                _dataFilter.IsEnabled<ISoftDelete>().ShouldBe(false);
            }

            _dataFilter.IsEnabled<ISoftDelete>().ShouldBe(true);
        }

        [Fact]
        public void Should_Handle_Dynamic_Filters()
        {
            _dataFilter.IsEnabled(typeof(ISoftDelete)).ShouldBe(true);

            using (_dataFilter.Disable<ISoftDelete<TestSoftDeleteClass>>())
            {
                _dataFilter.IsEnabled<ISoftDelete<TestSoftDeleteClass>>().ShouldBe(false);
            }

            _dataFilter.IsEnabled<ISoftDelete<TestSoftDeleteClass>>().ShouldBe(true);

            ShouldThrowExtensions.ShouldThrow(
                () => _dataFilter.IsEnabled(null),
                typeof(AbpException)
            );

            ShouldThrowExtensions.ShouldThrow(
                () => _dataFilter.IsEnabled(typeof(TestSoftDeleteClass)),
                typeof(AbpException)
            );

            ShouldThrowExtensions.ShouldThrow(
                () => _dataFilter.IsEnabled(typeof(ISoftDelete<ISoftDelete<TestSoftDeleteClass>>)),
                typeof(AbpException)
            );
        }

        [Fact]
        public void Should_Cache_Filter()
        {
            _dataFilter.ReadOnlyFilters.ContainsKey(typeof(ISoftDelete)).ShouldBe(false);

            _dataFilter.IsEnabled<ISoftDelete>().ShouldBe(true);

            _dataFilter.ReadOnlyFilters.ContainsKey(typeof(ISoftDelete)).ShouldBe(true);
        }

        [Fact]
        public void Should_Not_Cache_Filter()
        {
            _dataFilter.ReadOnlyFilters.ContainsKey(typeof(ISoftDelete)).ShouldBe(false);

            _dataFilter.IsEnabled<ISoftDelete>(false).ShouldBe(true);

            _dataFilter.ReadOnlyFilters.ContainsKey(typeof(ISoftDelete)).ShouldBe(false);
        }

        class TestSoftDeleteClass : ISoftDelete<TestSoftDeleteClass>
        {
            public bool IsDeleted { get; set; }
        }

        interface IGenericTestInterface { }

        [DependsOn(typeof(AbpDataModule))]
        public class TestModule : AbpModule
        {

        }
    }
}
