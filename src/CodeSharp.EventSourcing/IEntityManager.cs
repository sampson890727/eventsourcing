using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace CodeSharp.EventSourcing
{
    /// <summary>
    /// 定义CQRS架构中查询端的实体的CRUD操作
    /// </summary>
    public interface IEntityManager
    {
        /// <summary>
        /// 根据某个源对象创建T类型的实体。
        /// <remarks>
        /// 实现逻辑：以基于约定的方式自动将源对象中的属性赋值到要创建的T类型的实体的相同名称的属性上，
        /// </remarks>
        /// </summary>
        /// <typeparam name="T">要创建的实体的类型</typeparam>
        /// <param name="source">源对象，包含要创建的对象的数据</param>
        T Build<T>(object source) where T : class, new();
        /// <summary>
        /// 根据某个源对象创建并持久化T类型的实体。
        /// <remarks>
        /// 实现逻辑：以基于约定的方式自动将源对象中的属性赋值到要创建的T类型的实体的相同名称的属性上，
        /// 属性都赋值完成后，最后将新创建的T实体持久化。
        /// </remarks>
        /// </summary>
        /// <typeparam name="T">要创建的实体的类型</typeparam>
        /// <param name="source">源对象，包含要创建的对象的数据</param>
        void BuildAndSave<T>(object source) where T : class, new();
        /// <summary>
        /// 根据源对象更新目标对象，目标对象要更新的属性由源对象指定。
        /// <remarks>
        /// 实现逻辑：以基于约定的方式自动将源对象中通过当前方法第三个参数所提供的Lambda表达式所对应的所有属性更新到目标对象，
        /// 只要当前Lambda表达式所表示的属性在目标对象上存在，则进行更新。
        /// </remarks>
        /// </summary>
        /// <typeparam name="TSource">源对象的类型</typeparam>
        /// <param name="targetObject">目标对象</param>
        /// <param name="sourceObject">源对象</param>
        /// <param name="propertyExpressions">源对象要更新目标对象的相关属性信息集合</param>
        void Update<TSource>(object targetObject, object sourceObject, params Expression<Func<TSource, object>>[] propertyExpressions) where TSource : class;
        /// <summary>
        /// 根据源对象更新目标对象并持久化目标对象，目标对象要更新的属性由源对象指定。
        /// <remarks>
        /// 实现逻辑：以基于约定的方式自动将源对象中通过当前方法第三个参数所提供的Lambda表达式所对应的所有属性更新到目标对象，
        /// 只要当前Lambda表达式所表示的属性在目标对象上存在，则进行更新。
        /// 属性都赋值完成后，最后将目标对象持久化。
        /// </remarks>
        /// </summary>
        /// <typeparam name="TSource">源对象的类型</typeparam>
        /// <param name="targetObject">目标对象</param>
        /// <param name="sourceObject">源对象</param>
        /// <param name="propertyExpressions">源对象要更新目标对象的相关属性信息集合</param>
        void UpdateAndSave<TSource>(object targetObject, object sourceObject, params Expression<Func<TSource, object>>[] propertyExpressions) where TSource : class;
        /// <summary>
        /// 根据ID获取一个唯一的指定类型的对象
        /// </summary>
        /// <typeparam name="T">要获取的对象的类型</typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        T GetById<T>(object id) where T : class;
        /// <summary>
        /// 根据某个包含查询条件的匿名对象获取一个唯一的指定类型的对象，如果不存在，则抛出异常
        /// </summary>
        /// <typeparam name="T">要获取的对象的类型</typeparam>
        /// <param name="queryObject">包含查询条件的匿名对象</param>
        /// <returns></returns>
        T GetSingle<T>(object queryObject) where T : class;
        /// <summary>
        /// 根据某个包含查询条件的匿名对象获取一个唯一的指定类型的对象，如果不存在，则返回T类型的默认值
        /// </summary>
        /// <typeparam name="T">要获取的对象的类型</typeparam>
        /// <param name="queryObject">包含查询条件的匿名对象</param>
        /// <returns></returns>
        T GetSingleOrDefault<T>(object queryObject) where T : class;
        /// <summary>
        /// 根据某个包含查询条件的匿名对象获取多个的指定类型的对象
        /// </summary>
        /// <typeparam name="T">要获取的对象的类型</typeparam>
        /// <param name="queryObject">包含查询条件的匿名对象</param>
        /// <returns></returns>
        IEnumerable<T> Query<T>(object queryObject) where T : class;
        /// <summary>
        /// 新建一个指定的对象
        /// </summary>
        /// <param name="obj">要新建的对象</param>
        void Create(object obj);
        /// <summary>
        /// 更新一个指定的对象
        /// </summary>
        /// <param name="obj">要更新的对象</param>
        void Update(object obj);
        /// <summary>
        /// 新建或更新一个指定的对象，如果持久层不存在该对象，则新建；如果存在，则更新
        /// </summary>
        /// <param name="obj">要新建或更新的对象</param>
        void SaveOrUpdate(object obj);
        /// <summary>
        /// 删除一个指定的对象
        /// </summary>
        /// <param name="obj">要删除的对象</param>
        void Delete(object obj);
        /// <summary>
        /// 根据对象唯一标识删除指定类型的对象
        /// </summary>
        /// <typeparam name="T">要删除的对象的类型</typeparam>
        /// <param name="id">对象唯一标识</param>
        void Delete<T>(object id) where T : class;
        /// <summary>
        /// 根据某个包含查询条件的匿名对象删除符合条件的对象
        /// </summary>
        /// <typeparam name="T">要删除的对象的类型</typeparam>
        /// <param name="queryObject">包含查询条件的匿名对象</param>
        void DeleteByQuery<T>(object queryObject) where T : class;
    }
}