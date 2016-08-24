using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TJC.AppLink.DAL.ClassLibrary.Interfaces
{
    public interface IGenericRepository : IDisposable
    {
        IQueryable<T> GetAll<T>() where T : class;
        IQueryable<T> FindBy<T>(System.Linq.Expressions.Expression<Func<T, bool>> predicate) where T : class;
        T GetSingle<T>(Func<T, bool> predicate) where T : class;
        void Add<T>(T entity) where T : class;
        void Delete<T>(T entity) where T : class;
        void Edit<T>(T entity) where T : class;
        void Save();
        IEnumerable<T> ExecuteStoredProc<T>(string spName);
        IEnumerable<T> ExecuteStoredProc<T>(string spName, params object[] parameters);
        /// <summary>
        /// Execute sql command
        /// </summary>
        /// <param name="sql"></param>
        void ExecuteSqlCommand(string sql);
    }
}
