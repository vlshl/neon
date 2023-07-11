namespace Common
{
    public interface INetwork
    {
        double[] GetOutputs(int layer = 0);
        double[] Execute(double[] inputs);
        void Train(double[] inputs, double[] targets);
        object GetSettings();
        IEnumerable<double[,]> GetWeights();
        void LoadWeights(IEnumerable<double[,]>? weights);
    }
}
