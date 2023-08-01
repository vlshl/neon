namespace Common
{
    public interface INetwork
    {
        double[] GetOutputs(int layer = 0);
        double[,] GetWeights(int layer = 0);
        IEnumerable<double[,]> GetAllWeights();
        int GetHiddenLayersCount();
        double[] Execute(double[] inputs);
        void Train(double[] inputs, double[] targets);
        object GetSettings();
        void LoadWeights(IEnumerable<double[,]>? weights);
    }
}
