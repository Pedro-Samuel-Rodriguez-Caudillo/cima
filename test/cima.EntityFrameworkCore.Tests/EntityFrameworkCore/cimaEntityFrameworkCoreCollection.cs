using Xunit;

namespace cima.EntityFrameworkCore;

[CollectionDefinition(cimaTestConsts.CollectionDefinitionName)]
public class cimaEntityFrameworkCoreCollection : ICollectionFixture<cimaEntityFrameworkCoreFixture>
{

}
