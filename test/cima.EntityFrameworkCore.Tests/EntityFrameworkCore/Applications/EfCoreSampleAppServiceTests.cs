using cima.Samples;
using Xunit;

namespace cima.EntityFrameworkCore.Applications;

[Collection(cimaTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<cimaEntityFrameworkCoreTestModule>
{

}
