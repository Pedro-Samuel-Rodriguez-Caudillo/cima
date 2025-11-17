using cima.Samples;
using Xunit;

namespace cima.EntityFrameworkCore.Domains;

[Collection(cimaTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<cimaEntityFrameworkCoreTestModule>
{

}
