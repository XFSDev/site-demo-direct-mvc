using Models;

namespace SamplesFactory
{
    public class GridSampleCreator : SampleCreator
    {
        public override Sample CreateSample(SamplesModel sampleModel)
        {
            var sample = new BaseGridSample(sampleModel);
            return sample;
        }
    }
}