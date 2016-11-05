﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Grace.DependencyInjection.Lifestyle;

namespace Grace.DependencyInjection.Impl
{
    /// <summary>
    /// Base class for configurin an export strategy
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class WrapperFluentExportStrategyConfiguration<T> : IFluentExportStrategyConfiguration<T>
    {
        private readonly IFluentExportStrategyConfiguration<T> _strategy;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="strategy"></param>
        protected WrapperFluentExportStrategyConfiguration(IFluentExportStrategyConfiguration<T> strategy)
        {
            _strategy = strategy;
        }

        #region IFluentExportStrategyConfiguration

        /// <summary>
        /// Apply an action to the export just after construction
        /// </summary>
        /// <param name="applyAction">action to apply to export upon construction</param>
        /// <returns>configuration object</returns>
        public IFluentExportStrategyConfiguration<T> Apply(Action<T> applyAction)
        {
            return _strategy.Apply(applyAction);
        }

        /// <summary>
        /// Export as a specific type
        /// </summary>
        /// <param name="type">type to export as</param>
        /// <returns></returns>
        public IFluentExportStrategyConfiguration<T> As(Type type)
        {
            return _strategy.As(type);
        }

        /// <summary>
        /// Export as a particular type
        /// </summary>
        /// <typeparam name="TInterface">type to export as</typeparam>
        /// <returns>configuration object</returns>
        public IFluentExportStrategyConfiguration<T> As<TInterface>()
        {
            return _strategy.As<TInterface>();
        }

        /// <summary>
        /// Export as a keyed type
        /// </summary>
        /// <typeparam name="TInterface">export type</typeparam>
        /// <param name="key">key to export under</param>
        /// <returns>configuration object</returns>
        public IFluentExportStrategyConfiguration<T> AsKeyed<TInterface>(object key)
        {
            return _strategy.AsKeyed<TInterface>(key);
        }

        /// <summary>
        /// Export the type by the interfaces it implements
        /// </summary>
        /// <returns></returns>
        public IFluentExportStrategyConfiguration<T> ByInterfaces(Func<Type, bool> filter = null)
        {
            return _strategy.ByInterfaces(filter);
        }

        /// <summary>
        /// You can provide a cleanup method to be called 
        /// </summary>
        /// <param name="disposalCleanupDelegate"></param>
        /// <returns></returns>
        public IFluentExportStrategyConfiguration<T> DisposalCleanupDelegate(Action<T> disposalCleanupDelegate)
        {
            return _strategy.DisposalCleanupDelegate(disposalCleanupDelegate);
        }

        /// <summary>
        /// Export a public member of the type (property, field or method with return value)
        /// </summary>
        /// <typeparam name="TValue">type to export</typeparam>
        /// <param name="memberExpression">member expression</param>
        /// <returns></returns>
        public IFluentExportMemberConfiguration<T> ExportMember<TValue>(Expression<Func<T, TValue>> memberExpression)
        {
            return _strategy.ExportMember(memberExpression);
        }

        /// <summary>
        /// Mark an export as externally owned means the container will not track and dispose the instance
        /// </summary>
        /// <returns></returns>
        public IFluentExportStrategyConfiguration<T> ExternallyOwned()
        {
            return _strategy.ExternallyOwned();
        }

        /// <summary>
        /// Mark specific members to be injected
        /// </summary>
        /// <param name="selector">select specific members, if null all public members will be injected</param>
        /// <returns>configuration object</returns>
        public IFluentExportStrategyConfiguration<T> ImportMembers(Func<MemberInfo, bool> selector = null)
        {
            return _strategy.ImportMembers(selector);
        }

        /// <summary>
        /// Import a specific property
        /// </summary>
        /// <typeparam name="TProp"></typeparam>
        /// <param name="property">property expression</param>
        /// <returns></returns>
        public IFluentImportPropertyConfiguration<T, TProp> ImportProperty<TProp>(Expression<Func<T, TProp>> property)
        {
            return _strategy.ImportProperty(property);
        }

        /// <summary>
        /// Import a specific method on the type
        /// </summary>
        /// <param name="method">method to import</param>
        /// <returns></returns>
        public IFluentExportStrategyConfiguration<T> ImportMethod(Expression<Action<T>> method)
        {
            return _strategy.ImportMethod(method);
        }

        /// <summary>
        /// Assign a lifestyle to this export
        /// </summary>
        public ILifestylePicker<IFluentExportStrategyConfiguration<T>> Lifestyle => _strategy.Lifestyle;

        /// <summary>
        /// Export using a specific lifestyle
        /// </summary>
        /// <param name="lifestyle">lifestlye to use</param>
        /// <returns>configuration object</returns>
        public IFluentExportStrategyConfiguration<T> UsingLifestyle(ICompiledLifestyle lifestyle)
        {
            return _strategy.UsingLifestyle(lifestyle);
        }

        /// <summary>
        /// Add a condition to when this export can be used
        /// </summary>
        public IWhenConditionConfiguration<IFluentExportStrategyConfiguration<T>> When => _strategy.When;

        /// <summary>
        /// Add a specific value for a particuar parameter in the constructor
        /// </summary>
        /// <typeparam name="TParam">type of parameter</typeparam>
        /// <param name="paramValue">Func(T) value for the parameter</param>
        /// <returns>configuration object</returns>
        public IFluentWithCtorConfiguration<T, TParam> WithCtorParam<TParam>(Func<TParam> paramValue = null)
        {
            return _strategy.WithCtorParam(paramValue);
        }

        /// <summary>
        /// Add a specific value for a particuar parameter in the constructor
        /// </summary>
        /// <typeparam name="TParam">type of parameter</typeparam>
        /// <param name="paramValue">Func(IInjectionScope, IInjectionContext, T) value for the parameter</param>
        /// <returns>configuration object</returns>
        public IFluentWithCtorConfiguration<T, TParam> WithCtorParam<TParam>(Func<IExportLocatorScope, StaticInjectionContext, IInjectionContext, TParam> paramValue)
        {
            return _strategy.WithCtorParam(paramValue);
        }

        /// <summary>
        /// Adds metadata to an export
        /// </summary>
        /// <param name="key">metadata key</param>
        /// <param name="value">metadata value</param>
        /// <returns>configuration object</returns>
        public IFluentExportStrategyConfiguration<T> WithMetadata(object key, object value)
        {
            return _strategy.WithMetadata(key, value);
        }

        #endregion
    }
}
