﻿using System;
using Grace.DependencyInjection.Impl;

namespace Grace.DependencyInjection
{
   
    /// <summary>
    /// Represents a configuration information needed to construct an injection scope
    /// </summary>
    public interface IInjectionScopeConfiguration
    {
        /// <summary>
        /// internal method used by the container
        /// </summary>
        /// <param name="scope">owning scope</param>
        void SetInjectionScope(IInjectionScope scope);

        /// <summary>
        /// This the containers internal DI container. If you want to change any implementation you would add them here
        /// </summary>
        ImplementationFactory Implementation { get; }

        /// <summary>
        /// Allows you to configure how to construct compiled exports.
        /// </summary>
        ExportCompilationBehavior Behaviors { get; }

        /// <summary>
        /// Catch exceptions on disposal, false by default
        /// </summary>
        bool CatchDisposalExceptions { get; }

        /// <summary>
        /// Size of the array used to cache execution delegates. By default it's 64, if you wish to change this make sure it's a power of 2
        /// </summary>
        int CacheArraySize { get; }

        /// <summary>
        /// Size of array used to cache export strategies. By default it's 16, if you wish to change this make sure it's a power of 2
        /// </summary>
        int ExportStrategyArraySize { get; }

        /// <summary>
        /// Register concrete implementation that are unknown
        /// </summary>
        bool AutoRegisterUnknown { get; }

        /// <summary>
        /// Export as type and base implementations, true by default
        /// </summary>
        bool ExportAsBase { get; }

        /// <summary>
        /// Override which disposal scope is used for tracking disposables, null by default
        /// </summary>
        IDisposalScopeProvider DisposalScopeProvider { get; }

        /// <summary>
        /// Function that filters out interface types.
        /// First type arg is interface, second type arg is implementing, return true if should filter out
        /// Note: by default IDisposable and _Attribute are filter out
        /// </summary>
        Func<Type,Type,bool> ExportByInterfaceFilter { get; }

        /// <summary>
        /// Clone configuration
        /// </summary>
        /// <returns></returns>
        IInjectionScopeConfiguration Clone();
    }
}
