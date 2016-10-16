﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Grace.Data.Immutable;
using Grace.DependencyInjection.Impl.Expressions;

namespace Grace.DependencyInjection.Impl
{
    public class ActivationStrategyCompiler : IActivationStrategyCompiler
    {
        private readonly IInjectionScopeConfiguration _configuration;
        private readonly IActivationExpressionBuilder _builder;
        private readonly IAttributeDiscoveryService _attributeDiscoveryService;
        private readonly ILifestyleExpressionBuilder _exportExpressionBuilder;
        private readonly IInjectionContextCreator _injectionContextCreator;
        private readonly IExpressionConstants _constants;

        public ActivationStrategyCompiler(IInjectionScopeConfiguration configuration,
                                          IActivationExpressionBuilder builder,
                                          IAttributeDiscoveryService attributeDiscoveryService,
                                          ILifestyleExpressionBuilder exportExpressionBuilder,
                                          IInjectionContextCreator injectionContextCreator,
                                          IExpressionConstants constants)
        {
            _configuration = configuration;
            _builder = builder;
            _attributeDiscoveryService = attributeDiscoveryService;
            _constants = constants;
            _exportExpressionBuilder = exportExpressionBuilder;
            _injectionContextCreator = injectionContextCreator;

            _builder.SetCompiler(this);
        }

        public int MaxObjectGraphDepth => _configuration.Behaviors.MaxObjectGraphDepth();

        public virtual IActivationExpressionRequest CreateNewRequest(Type activationType, int objectGraphDepth)
        {
            if (activationType == null) throw new ArgumentNullException(nameof(activationType));

            return new ActivationExpressionRequest(activationType,
                                                   RequestType.Root,
                                                   new ActivationServices(this, _builder, _attributeDiscoveryService, _exportExpressionBuilder, _injectionContextCreator),
                                                   _constants,
                                                   objectGraphDepth);
        }

        public virtual IActivationExpressionResult CreateNewResult(IActivationExpressionRequest request, Expression expression = null)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            return new ActivationExpressionResult(request) { Expression = expression };
        }

        public virtual ActivationStrategyDelegate FindDelegate(IInjectionScope scope, Type locateType, object key)
        {
            var activationDelegate = LocateStrategyFromCollectionContainers(scope, locateType, key);

            if (activationDelegate != null)
            {
                return activationDelegate;
            }

            activationDelegate = LocateEnumerableStrategy(scope, locateType, key);

            if (activationDelegate != null)
            {
                return activationDelegate;
            }

            lock (scope.GetLockObject(RootInjectionScope.ActivationStrategyAddLockName))
            {
                activationDelegate = LocateStrategyFromCollectionContainers(scope, locateType, key);

                if (activationDelegate != null)
                {
                    return activationDelegate;
                }

                ProcessMissingStrategyProviders(scope, CreateNewRequest(locateType, 1));

                activationDelegate = LocateStrategyFromCollectionContainers(scope, locateType, key);

                if (activationDelegate != null)
                {
                    return activationDelegate;
                }
            }

            return null;
        }

        private ActivationStrategyDelegate LocateEnumerableStrategy(IInjectionScope scope, Type locateType, object key)
        {
            if (locateType.IsArray ||
                (locateType.IsConstructedGenericType &&
                 locateType.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
            {
                var result = _builder.GetActivationExpression(scope, CreateNewRequest(locateType, 1));

                return CompileDelegate(scope, result);
            }

            return null;
        }


        /// <summary>
        /// Locate a strategy from collection containers
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="locateType"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        protected virtual ActivationStrategyDelegate LocateStrategyFromCollectionContainers(IInjectionScope scope, Type locateType,
            object key)
        {
            if (key != null)
            {
                return FindKeyedDelegate(scope, locateType, key);
            }

            var strategyCollection = scope.StrategyCollectionContainer.GetActivationStrategyCollection(locateType);

            if (strategyCollection != null)
            {
                var primary = strategyCollection.GetPrimary();

                if (primary != null)
                {
                    return primary.GetActivationStrategyDelegate(scope, this, locateType);
                }

                var strategy = GetStrategyFromCollection(strategyCollection, scope, locateType);

                if (strategy != null)
                {
                    return strategy.GetActivationStrategyDelegate(scope, this, locateType);
                }
            }

            bool isGeneric = locateType.IsConstructedGenericType;

            if (isGeneric)
            {
                var generic = locateType.GetGenericTypeDefinition();

                strategyCollection = scope.StrategyCollectionContainer.GetActivationStrategyCollection(generic);

                if (strategyCollection != null)
                {
                    var primary = strategyCollection.GetPrimary();

                    if (primary != null)
                    {
                        return primary.GetActivationStrategyDelegate(scope, this, locateType);
                    }

                    var strategy = GetStrategyFromCollection(strategyCollection, scope, locateType);

                    if (strategy != null)
                    {
                        return strategy.GetActivationStrategyDelegate(scope, this, locateType);
                    }
                }
            }

            var wrapperCollection = scope.WrapperCollectionContainer.GetActivationStrategyCollection(locateType);

            if (wrapperCollection != null)
            {
                var primary = wrapperCollection.GetPrimary();

                if (primary != null)
                {
                    return primary.GetActivationStrategyDelegate(scope, this, locateType);
                }

                var strategy = GetStrategyFromCollection(strategyCollection, scope, locateType);

                if (strategy != null)
                {
                    return strategy.GetActivationStrategyDelegate(scope, this, locateType);
                }
            }

            if (isGeneric)
            {
                var generic = locateType.GetGenericTypeDefinition();

                wrapperCollection = scope.WrapperCollectionContainer.GetActivationStrategyCollection(generic);

                if (wrapperCollection != null)
                {
                    var primary = wrapperCollection.GetPrimary();

                    if (primary != null)
                    {
                        return primary.GetActivationStrategyDelegate(scope, this, locateType);
                    }

                    var strategy = GetStrategyFromCollection(strategyCollection, scope, locateType);

                    if (strategy != null)
                    {
                        return strategy.GetActivationStrategyDelegate(scope, this, locateType);
                    }
                }
            }

            return null;
        }

        private T GetStrategyFromCollection<T>(IActivationStrategyCollection<T> strategyCollection, IInjectionScope scope, Type locateType) where T : IActivationStrategy
        {
            foreach (var strategy in strategyCollection.GetStrategies())
            {
                if (strategy.HasConditions)
                {
                    var pass = true;

                    foreach (var condition in strategy.Conditions)
                    {
                        if (!condition.MeetsCondition(strategy, new StaticInjectionContext(locateType)))
                        {
                            pass = false;
                            break;
                        }
                    }

                    if (!pass)
                    {
                        continue;
                    }
                }

                return strategy;
            }

            return default(T);
        }

        protected virtual ActivationStrategyDelegate FindKeyedDelegate(IInjectionScope scope, Type locateType, object key)
        {
            var collection = scope.StrategyCollectionContainer.GetActivationStrategyCollection(locateType);

            var strategy = collection?.GetKeyedStrategy(key);

            return strategy?.GetActivationStrategyDelegate(scope, this, locateType);
        }

        public virtual ActivationStrategyDelegate CompileDelegate(IInjectionScope scope, IActivationExpressionResult expressionContext)
        {
            Expression compileExpression;

            if (expressionContext.Request.InjectionContextRequired())
            {
                AddInjectionContextExpression(expressionContext);
            }

            var finalExpression = expressionContext.Expression;

            if (!finalExpression.Type.IsByRef)
            {
                finalExpression = Expression.Convert(finalExpression, typeof(object));
            }

            var parameters = expressionContext.ExtraParameters();
            var extraExpressions = expressionContext.ExtraExpressions();

            if (parameters == ImmutableLinkedList<ParameterExpression>.Empty &&
                extraExpressions == ImmutableLinkedList<Expression>.Empty)
            {
                compileExpression = finalExpression;
            }
            else
            {
                var list = new List<Expression>(expressionContext.ExtraExpressions())
                {
                    finalExpression
                };

                compileExpression = Expression.Block(expressionContext.ExtraParameters(), list);
            }

            var compiled =
                Expression.Lambda<ActivationStrategyDelegate>(compileExpression,
                                                              expressionContext.Request.Constants.ScopeParameter,
                                                              expressionContext.Request.Constants.RootDisposalScope,
                                                              expressionContext.Request.Constants.InjectionContextParameter)
                                                              .Compile();

            return compiled;
        }

        public void ProcessMissingStrategyProviders(IInjectionScope scope, IActivationExpressionRequest request)
        {
            foreach (var strategyProvider in scope.MissingExportStrategyProviders)
            {
                foreach (var activationStrategy in strategyProvider.ProvideExports(scope, request))
                {
                    if (activationStrategy is ICompiledExportStrategy)
                    {
                        scope.StrategyCollectionContainer.AddStrategy(activationStrategy as ICompiledExportStrategy);
                    }
                    else if (activationStrategy is ICompiledWrapperStrategy)
                    {
                        scope.WrapperCollectionContainer.AddStrategy(activationStrategy as ICompiledWrapperStrategy);
                    }
                    else if (activationStrategy is ICompiledDecoratorStrategy)
                    {
                        scope.DecoratorCollectionContainer.AddStrategy(activationStrategy as ICompiledDecoratorStrategy);
                    }
                }
            }
        }

        private void AddInjectionContextExpression(IActivationExpressionResult expressionContext)
        {
            var method = typeof(IInjectionContextCreator).GetRuntimeMethod("CreateContext",
                    new[]
                    {
                        typeof(Type),
                        typeof(object)
                    });


            var newExpression = Expression.Call(Expression.Constant(_injectionContextCreator),
                                                method,
                                                Expression.Constant(expressionContext.Request.ActivationType),
                                                Expression.Constant(null, typeof(object)));

            var assign =
                Expression.Assign(expressionContext.Request.Constants.InjectionContextParameter, newExpression);

            var ifThen =
                Expression.IfThen(
                    Expression.Equal(expressionContext.Request.Constants.InjectionContextParameter,
                        Expression.Constant(null, typeof(IInjectionContext))),
                    assign);

            expressionContext.AddExtraExpression(ifThen);
        }
    }
}
