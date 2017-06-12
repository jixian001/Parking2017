using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using Parking.Auxiliary;

namespace Parking.Data
{
    public class Repository<TEntity> where TEntity:class
    {
        public DbContext _dbContext { get; set; }        

        private Repository()
        {
        }

        public Repository(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        #region Find 查找实体
        public TEntity Find(int ID)
        {
            return _dbContext.Set<TEntity>().Find(ID);
        }
        public TEntity Find(Expression<Func<TEntity,bool>> where)
        {
            return _dbContext.Set<TEntity>().FirstOrDefault(where);
        }
        #endregion

        #region FindList 查找实体列表
        public IQueryable<TEntity> FindList()
        {
            return _dbContext.Set<TEntity>();
        }

        public IQueryable<TEntity> FindList(Expression<Func<TEntity, bool>> where)
        {
            return _dbContext.Set<TEntity>().Where(where);
        }

        public IQueryable<TEntity> FindList(Expression<Func<TEntity,bool>> where,int number)
        {
            return _dbContext.Set<TEntity>().Where(where).Take(number);
        }

        /// <summary>
        /// 查找实体列表
        /// </summary>
        /// <param name="where">查询Lambda表达式</param>
        /// <param name="orderParams">排序参数</param>
        /// <param name="number">获取的记录数量【0-不启用】</param>
        /// <returns></returns>
        public IQueryable<TEntity> FindList(Expression<Func<TEntity, bool>> where, OrderParam[] orderParams, int number)
        {
            var _list = _dbContext.Set<TEntity>().Where(where);
            var _orderParames = Expression.Parameter(typeof(TEntity), "o");
            if (orderParams != null && orderParams.Length > 0)
            {
                bool _isFirstParam = true;
                for (int i = 0; i < orderParams.Length; i++)
                {
                    //根据属性名获取属性
                    var _property = typeof(TEntity).GetProperty(orderParams[i].PropertyName);
                    //创建一个访问属性的表达式
                    var _propertyAccess = Expression.MakeMemberAccess(_orderParames, _property);
                    var _orderByExp = Expression.Lambda(_propertyAccess, _orderParames);
                    string _orderName;
                    if (_isFirstParam)
                    {
                        _orderName = orderParams[i].Method == OrderMethod.Asc ? "OrderBy" : "OrderByDescending";
                        _isFirstParam = false;
                    }
                    else
                    {
                        _orderName = orderParams[i].Method == OrderMethod.Desc ? "ThenBy" : "ThenByDescending";
                    }

                    MethodCallExpression resultExp = Expression.Call(typeof(Queryable), _orderName, new Type[] { typeof(TEntity), _property.PropertyType }, _list.Expression, Expression.Quote(_orderByExp));
                    _list = _list.Provider.CreateQuery<TEntity>(resultExp);
                }
            }
            if (number > 0)
            {
                _list = _list.Take(number);
            }
            return _list;
        }

        public IQueryable<TEntity> FindList(Expression<Func<TEntity,bool>> where,OrderParam orderParam, int number)
        {
            OrderParam[] _orderParams = null;
            if (orderParam != null)
            {
                _orderParams = new OrderParam[] { orderParam };
            }
            return FindList(where, _orderParams, number);
        }

        public IQueryable<TEntity> FindList(Expression<Func<TEntity,bool>> where,OrderParam param)
        {
            return FindList(where, param, 0);
        }


        #endregion

        #region 分页 查找实体列表
        public IQueryable<TEntity> FindPageList(int pageSize, int pageIndex, out int totalNumber,OrderParam param)
        {
            return FindPageList(pageSize, pageIndex, out totalNumber, (TEntity) => true, new OrderParam[] { param });
        }

        /// <summary>
        /// 查找分页列表
        /// </summary>
        /// <param name="pageSize">每页记录数</param>
        /// <param name="pageIndex">页码。首页从1开始</param>
        /// <param name="totalNum">总记录数</param>
        /// <param name="where">查询表达式</param>
        /// <returns></returns>
        public IQueryable<TEntity> FindPageList(int pageSize,int pageIndex,out int totalNum,Expression<Func<TEntity,bool>> where,OrderParam[] orderParams)
        {
            if (pageIndex < 1)
            {
                pageIndex = 1;
            }
            if (pageSize < 1)
            {
                pageSize = 10;
            }
            IQueryable<TEntity> _List = _dbContext.Set<TEntity>().Where(where);
            var _orderParames = Expression.Parameter(typeof(TEntity), "o");
            if (orderParams != null && orderParams.Length > 0)
            {
                for (int i = 0; i < orderParams.Length; i++)
                {
                    //根据属性名获取属性
                    var _property = typeof(TEntity).GetProperty(orderParams[i].PropertyName);
                    //创建一个访问属性的表达式
                    var _propertyAccess = Expression.MakeMemberAccess(_orderParames, _property);
                    var _orderByExp = Expression.Lambda(_propertyAccess, _orderParames);
                    string _orderName = orderParams[i].Method == OrderMethod.Asc ? "OrderBy" : "OrderByDescending";
                    MethodCallExpression resultExp = Expression.Call(typeof(Queryable), _orderName, new Type[] { typeof(TEntity), _property.PropertyType }, _List.Expression, Expression.Quote(_orderByExp));
                    _List = _List.Provider.CreateQuery<TEntity>(resultExp);
                }
            }
            totalNum = _List.Count();
            return _List.Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }
        #endregion

        #region 添加实体 Add
        public int Add(TEntity entity)
        {
            return Add(entity, true);
        }

        public int Add(TEntity entity,bool isSave)
        {
            _dbContext.Set<TEntity>().Add(entity);

            //事件引发，回推数据,用于可执行作业创建时，回调至主页面中
            MainCallback<TEntity>.Instance().OnChange(entity);

            return isSave ? _dbContext.SaveChanges() : 0;
        }
        #endregion

        #region 更新实体 update
        public int Update(TEntity entity,bool isSave)
        {
            _dbContext.Set<TEntity>().Attach(entity);
            _dbContext.Entry<TEntity>(entity).State = EntityState.Modified;

            //事件引发，回推数据
            MainCallback<TEntity>.Instance().OnChange(entity);

            return isSave ? _dbContext.SaveChanges() : 0;
        }

        public int Update(TEntity entity)
        {
            int nback= Update(entity, true);           
            return nback;
        }
        #endregion

        #region 删除
        public int Delete(TEntity entity,bool isSave)
        {
            _dbContext.Set<TEntity>().Attach(entity);
            _dbContext.Entry<TEntity>(entity).State = EntityState.Deleted;
            return isSave ? _dbContext.SaveChanges() : 0;
        }

        public int Delete(TEntity entity)
        {
            return Delete(entity, true);
        }

        public int Delete(IEnumerable<TEntity> entities)
        {
            _dbContext.Set<TEntity>().RemoveRange(entities);
            return _dbContext.SaveChanges();
        }

        #endregion

        #region 查询记录数 Count
        public int Count()
        {
            return _dbContext.Set<TEntity>().Count();
        }

        public int Count(Expression<Func<TEntity,bool>> predicate)
        {
            return _dbContext.Set<TEntity>().Count(predicate);
        }
        /// <summary>
        /// 查询记录是否存在
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public bool IsContains(Expression<Func<TEntity,bool>> predicate)
        {
            return Count(predicate) > 0;
        }

        #endregion

        public int SaveChanges()
        {
            return _dbContext.SaveChanges();
        }
    }
}
