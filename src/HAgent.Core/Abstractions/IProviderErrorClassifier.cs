using System;
using HAgent.Models;

namespace HAgent.Abstractions
{
    public interface IProviderErrorClassifier
    {
        ProviderErrorKind Classify(Exception exception);
    }
}
