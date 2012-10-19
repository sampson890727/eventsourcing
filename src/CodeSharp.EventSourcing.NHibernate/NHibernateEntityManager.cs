//Copyright (c) CodeSharp.  All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NHibernate.Criterion;
using NHibernateNamespace = NHibernate;

namespace CodeSharp.EventSourcing.NHibernate
{
    [Transactional]
    public class NHibernateEntityManager : IEntityManager
    {
        protected INHibernateSessionManager _sessionManager = DependencyResolver.Resolve<INHibernateSessionManager>();

        T IEntityManager.Build<T>(object source)
        {
            var obj = new T();
            var propertiesFromSource = source.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            foreach (var property in properties)
            {
                var sourceProperty = propertiesFromSource.FirstOrDefault(x => x.Name == property.Name);
                if (sourceProperty != null)
                {
                    property.SetValue(obj, sourceProperty.GetValue(source, null), null);
                }
            }

            return obj;
        }
        [Transaction]
        void IEntityManager.BuildAndSave<T>(object source)
        {
            var obj = ((IEntityManager)this).Build<T>(source);
            ((IEntityManager)this).Create(obj);
        }
        void IEntityManager.Update<TSource>(object targetObject, object sourceObject, params Expression<Func<TSource, object>>[] propertyExpressions)
        {
            foreach (var lambdaExpression in propertyExpressions)
            {
                var propertyInfoFromSource = GetPropertyInfo<TSource, object>(lambdaExpression);
                var propertyInfoFromTarget = targetObject.GetType().GetProperties().SingleOrDefault(x => x.Name == propertyInfoFromSource.Name);
                if (propertyInfoFromTarget != null)
                {
                    propertyInfoFromTarget.SetValue(targetObject, propertyInfoFromSource.GetValue(sourceObject, null), null);
                }
            }
        }
        [Transaction]
        void IEntityManager.UpdateAndSave<TSource>(object targetObject, object sourceObject, params Expression<Func<TSource, object>>[] propertyExpressions)
        {
            ((IEntityManager)this).Update(targetObject, sourceObject, propertyExpressions);
            ((IEntityManager)this).Update(targetObject);
        }
        [Transaction]
        T IEntityManager.GetById<T>(object id)
        {
            return _sessionManager.OpenSession().Get<T>(id);
        }
        [Transaction]
        T IEntityManager.GetSingle<T>(object queryObject)
        {
            return GetSingle<T>(CreateCriterionByQueryObject(queryObject));
        }
        [Transaction]
        T IEntityManager.GetSingleOrDefault<T>(object queryObject)
        {
            return GetSingleOrDefault<T>(CreateCriterionByQueryObject(queryObject));
        }
        [Transaction]
        IEnumerable<T> IEntityManager.Query<T>(object queryObject)
        {
            return Query<T>(CreateCriterionByQueryObject(queryObject));
        }
        [Transaction]
        void IEntityManager.Create(object obj)
        {
            _sessionManager.OpenSession().Save(obj);
        }
        [Transaction]
        void IEntityManager.Update(object obj)
        {
            _sessionManager.OpenSession().Update(obj);
        }
        [Transaction]
        void IEntityManager.SaveOrUpdate(object obj)
        {
            _sessionManager.OpenSession().SaveOrUpdate(obj);
        }
        [Transaction]
        void IEntityManager.Delete(object obj)
        {
            _sessionManager.OpenSession().Delete(obj);
        }
        [Transaction]
        void IEntityManager.Delete<T>(object id)
        {
            var session = _sessionManager.OpenSession();
            var obj = session.Get<T>(id);
            if (obj != null)
            {
                session.Delete(obj);
            }
        }
        [Transaction]
        void IEntityManager.DeleteByQuery<T>(object queryObject)
        {
            DeleteByCriterion<T>(CreateCriterionByQueryObject(queryObject));
        }

        [Transaction]
        protected virtual T GetSingle<T>(ICriterion criterion) where T : class
        {
            return _sessionManager.OpenSession().CreateCriteria<T>().Add(criterion).UniqueResult<T>();
        }
        [Transaction]
        protected virtual T GetSingleOrDefault<T>(ICriterion criterion) where T : class
        {
            return _sessionManager.OpenSession().CreateCriteria<T>().Add(criterion).List<T>().SingleOrDefault();
        }
        [Transaction]
        protected virtual IEnumerable<T> Query<T>(ICriterion criterion) where T : class
        {
            return _sessionManager.OpenSession().CreateCriteria<T>().Add(criterion).List<T>();
        }
        [Transaction]
        protected virtual void DeleteByCriterion<T>(ICriterion criterion) where T : class
        {
            var session = _sessionManager.OpenSession();
            var objects = session.CreateCriteria<T>().Add(criterion).List<T>();
            foreach (var obj in objects)
            {
                session.Delete(obj);
            }
        }

        protected ICriterion CreateCriterionByQueryObject(object queryObject)
        {
            ICriterion criterion = null;
            var properties = queryObject.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            for (var index = 0; index < properties.Count(); index++)
            {
                var property = properties.ElementAt(index);
                if (index == 0)
                {
                    criterion = NHibernateNamespace.Criterion.Expression.Eq(property.Name, property.GetValue(queryObject, null));
                }
                else
                {
                    criterion = NHibernateNamespace.Criterion.Expression.And(criterion, NHibernateNamespace.Criterion.Expression.Eq(property.Name, property.GetValue(queryObject, null)));
                }
            }

            return criterion;
        }
        protected PropertyInfo GetPropertyInfo<TSource, TProperty>(Expression<Func<TSource, TProperty>> lambda)
        {
            Type type = typeof(TSource);
            MemberExpression memberExpression = null;

            if (lambda.Body.NodeType == ExpressionType.Convert)
            {
                memberExpression = ((UnaryExpression)lambda.Body).Operand as MemberExpression;
            }
            else if (lambda.Body.NodeType == ExpressionType.MemberAccess)
            {
                memberExpression = lambda.Body as MemberExpression;
            }

            if (memberExpression == null)
            {
                throw new ArgumentException(string.Format("Invalid Lambda Expression '{0}'.", lambda.ToString()));
            }

            PropertyInfo propInfo = memberExpression.Member as PropertyInfo;
            if (propInfo == null)
            {
                throw new ArgumentException(string.Format("Expression '{0}' refers to a field, not a property.", lambda.ToString()));
            }

            if (type != propInfo.ReflectedType && !type.IsSubclassOf(propInfo.ReflectedType))
            {
                throw new ArgumentException(string.Format("Expresion '{0}' refers to a property that is not from type {1}.", lambda.ToString(), type));
            }

            return propInfo;
        }
    }
}
