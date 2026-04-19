using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace FinanceCrudApp.Onnx;

public sealed class OnnxModelService : IDisposable
{
    private InferenceSession? _session;

    public bool IsModelLoaded { get; private set; }
    public string StatusMessage { get; private set; } = "ONNX not initialized.";
    public string ModelPath { get; }

    public OnnxModelService()
    {
        ModelPath = Path.Combine(AppContext.BaseDirectory, "onnx_models", "transaction_category_proba.onnx");
    }

    public void Initialize()
    {
        try
        {
            if (!File.Exists(ModelPath))
            {
                IsModelLoaded = false;
                StatusMessage = $"ONNX model not found: {ModelPath}";
                return;
            }

            _session = new InferenceSession(ModelPath);
            IsModelLoaded = true;

            var inputNames = string.Join(", ", _session.InputMetadata.Keys);
            var outputNames = string.Join(", ", _session.OutputMetadata.Keys);

            StatusMessage = $"ONNX loaded. Inputs: {inputNames}. Outputs: {outputNames}.";
        }
        catch (Exception ex)
        {
            IsModelLoaded = false;
            StatusMessage = $"ONNX load failed: {ex.Message}";
            _session?.Dispose();
            _session = null;
        }
    }

    public bool TrySuggestCategoryWithConfidence(
        decimal amount,
        string transactionType,
        int merchantId,
        int accountId,
        out OnnxCategorySuggestionResult? result,
        out string message)
    {
        result = null;

        if (_session == null)
        {
            message = StatusMessage;
            return false;
        }

        try
        {
            float isIncome = transactionType == "Income" ? 1f : 0f;

            var inputTensor = new DenseTensor<float>(
                new float[]
                {
                    (float)amount,
                    isIncome,
                    merchantId,
                    accountId
                },
                new[] { 1, 4 });

            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("float_input", inputTensor)
            };

            using var outputs = _session.Run(inputs);

            var labelOutput = outputs.FirstOrDefault(o =>
                o.Name.Contains("label", StringComparison.OrdinalIgnoreCase))
                ?? outputs.First();

            var probabilityOutput = outputs.FirstOrDefault(o =>
                o.Name.Contains("prob", StringComparison.OrdinalIgnoreCase));

            long label = labelOutput.AsTensor<long>().ToArray().FirstOrDefault();

            if (probabilityOutput == null)
            {
                message = "ONNX probability output not found.";
                return false;
            }

            var probs = probabilityOutput.AsTensor<float>().ToArray();

            if (probs.Length < 3)
            {
                message = "ONNX probability output is incomplete.";
                return false;
            }

            string? suggestedCategory = MapLabel(label);

            if (string.IsNullOrWhiteSpace(suggestedCategory))
            {
                message = "ONNX returned an unknown category label.";
                return false;
            }

            float food = probs[0];
            float transport = probs[1];
            float salary = probs[2];
            float confidence = Math.Max(food, Math.Max(transport, salary));

            result = new OnnxCategorySuggestionResult
            {
                SuggestedCategory = suggestedCategory,
                Confidence = confidence,
                FoodProbability = food,
                TransportProbability = transport,
                SalaryProbability = salary
            };

            message = result.ToDisplayText();
            return true;
        }
        catch (Exception ex)
        {
            message = $"ONNX inference failed: {ex.Message}";
            return false;
        }
    }

    private string? MapLabel(long label)
    {
        return label switch
        {
            1 => "Food",
            2 => "Transport",
            3 => "Salary",
            _ => null
        };
    }

    public void Dispose()
    {
        _session?.Dispose();
    }
}
