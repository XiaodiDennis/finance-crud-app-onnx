using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.ML.OnnxRuntime;

namespace FinanceCrudApp.Onnx;

public sealed class OnnxModelService : IDisposable
{
    private InferenceSession? _session;

    public bool IsModelLoaded { get; private set; }
    public string StatusMessage { get; private set; } = "ONNX not initialized.";
    public string ModelPath { get; }

    public OnnxModelService()
    {
        ModelPath = Path.Combine(AppContext.BaseDirectory, "onnx_models", "transaction_category.onnx");
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

    public IReadOnlyList<string> GetInputNames()
    {
        if (_session == null)
            return Array.Empty<string>();

        return _session.InputMetadata.Keys.ToList();
    }

    public IReadOnlyList<string> GetOutputNames()
    {
        if (_session == null)
            return Array.Empty<string>();

        return _session.OutputMetadata.Keys.ToList();
    }

    public void Dispose()
    {
        _session?.Dispose();
    }
}
