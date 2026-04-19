namespace FinanceCrudApp.Onnx;

public static class OnnxRuntimeState
{
    public static OnnxModelService ModelService { get; } = new OnnxModelService();
}
