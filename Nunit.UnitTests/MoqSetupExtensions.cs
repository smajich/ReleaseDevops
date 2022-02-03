﻿using Moq;
using MvcMusicStore.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace MsTest.UnitTests
{
    public static class MoqSetupExtensions
    {
        static IEnumerable<Type> domainTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes());
        public static Mock<DbSet<T>> createFakeDBSet<T>(this Mock<MusicStoreEntities> db, List<T> list = null, Func<List<T>, object[], T> find = null, bool createDerivedSets = true) where T : class
        {
            list = list ?? new List<T>();
            var data = list.AsQueryable();
            //var mockSet = MockHelper.CreateDbSet(list, find);
            var mockSet = new Mock<DbSet<T>>() { CallBase = true };
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(() => { return data.Provider; });
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(() => { return data.Expression; });
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(() => { return data.ElementType; });
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => { return list.GetEnumerator(); });

            mockSet.Setup(m => m.Add(It.IsAny<T>())).Returns<T>(i => { list.Add(i); return i; });
            mockSet.Setup(m => m.AddRange(It.IsAny<IEnumerable<T>>())).Returns<IEnumerable<T>>((i) => { list.AddRange(i); return i; });
            mockSet.Setup(m => m.Remove(It.IsAny<T>())).Returns<T>(i => { list.Remove(i); return i; });
            if (find != null) mockSet.As<IDbSet<T>>().Setup(m => m.Find(It.IsAny<object[]>())).Returns<object[]>((i) => { return find(list, i); });
            //mockSet.Setup(m => m.Create()).Returns(new T());

            db.Setup(x => x.Set<T>()).Returns(mockSet.Object);

            //Setup all derived classes            
            try
            {
                if (createDerivedSets)
                {
                    var type = typeof(T);
                    var concreteTypes = domainTypes.Where(x => type.IsAssignableFrom(x) && type != x).ToList();
                    var method = typeof(MoqSetupExtensions).GetMethod("createFakeDBSetSubType");
                    foreach (var item in concreteTypes)
                    {
                        var invokeResult = method.MakeGenericMethod(type, item)
                            .Invoke(null, new object[] { db, mockSet });
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return mockSet;
        }
        public static Mock<DbSet<SubType>> createFakeDBSetSubType<BaseType, SubType>(this Mock<MusicStoreEntities> db, Mock<DbSet<BaseType>> baseSet)
            where BaseType : class
            where SubType : class, BaseType
        {
            var dbSet = db.Object.Set<BaseType>();

            var mockSet = new Mock<DbSet<SubType>>() { CallBase = true };
            mockSet.As<IQueryable<SubType>>().Setup(m => m.Provider).Returns(() => { return dbSet.OfType<SubType>().Provider; });
            mockSet.As<IQueryable<SubType>>().Setup(m => m.Expression).Returns(() => { return dbSet.OfType<SubType>().Expression; });
            mockSet.As<IQueryable<SubType>>().Setup(m => m.ElementType).Returns(() => { return dbSet.OfType<SubType>().ElementType; });
            mockSet.As<IQueryable<SubType>>().Setup(m => m.GetEnumerator()).Returns(() => { return dbSet.OfType<SubType>().GetEnumerator(); });

            mockSet.Setup(m => m.Add(It.IsAny<SubType>())).Returns<SubType>(i => { dbSet.Add(i); return i; });
            mockSet.Setup(m => m.AddRange(It.IsAny<IEnumerable<SubType>>())).Returns<IEnumerable<SubType>>((i) => { dbSet.AddRange(i); return i; });
            mockSet.Setup(m => m.Remove(It.IsAny<SubType>())).Returns<SubType>(i => { dbSet.Remove(i); return i; });
            mockSet.As<IDbSet<SubType>>().Setup(m => m.Find(It.IsAny<object[]>())).Returns<object[]>((i) => { return dbSet.Find(i) as SubType; });

            baseSet.Setup(m => m.Create<SubType>()).Returns(() => { return mockSet.Object.Create(); });

            db.Setup(x => x.Set<SubType>()).Returns(mockSet.Object);
            return mockSet;
        }
    }
}
