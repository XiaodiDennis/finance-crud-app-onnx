namespace FinanceCrudApp.Onnx;

public sealed class OnnxCategorySuggestionResult
{
    public string SuggestedCategory { get; init; } = "";
    public float Confidence { get; init; }

    public float FoodProbability { get; init; }
    public float TransportProbability { get; init; }
    public float SalaryProbability { get; init; }

    public bool IsHighConfidence => Confidence >= 0.80f;

    public string ToDisplayText()
    {
        return $"ONNX suggestion: {SuggestedCategory} | Confidence: {Confidence:P0} | " +
               $"Food: {FoodProbability:P0}, Transport: {TransportProbability:P0}, Salary: {SalaryProbability:P0}";
    }
}
