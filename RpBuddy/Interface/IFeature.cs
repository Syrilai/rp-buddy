using System;

namespace RpBuddy.Interface;

public interface IFeature : IDisposable
{
    bool IsEnabled { get; }
}