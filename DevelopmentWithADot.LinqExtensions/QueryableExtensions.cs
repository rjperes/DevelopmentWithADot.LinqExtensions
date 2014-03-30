using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.UI;

namespace DevelopmentWithADot.LinqExtensions
{
	public static class QueryableExtensions
	{
		#region ThenBy
		public static IOrderedQueryable<T> ThenByPath<T>(this IOrderedQueryable<T> query, String path)
		{
			Type currentType = typeof(T);
			String[] parts = path.Split(',');

			foreach (String part in parts)
			{
				PropertyInfo propInfo = currentType.GetProperty(part);
				Type propType = propInfo.PropertyType;
				String propFetchFunctionName = "ThenBy";
				Type delegateType = typeof(Func<,>).MakeGenericType(currentType, propType);

				ParameterExpression exprParam = Expression.Parameter(currentType, "it");
				MemberExpression exprProp = Expression.Property(exprParam, part);
				LambdaExpression exprLambda = Expression.Lambda(delegateType, exprProp, new ParameterExpression[] { exprParam });

				Type orderExtensionMethodsType = typeof(Queryable);

				List<Type> fetchMethodTypes = new List<Type>();
				fetchMethodTypes.AddRange(query.GetType().GetGenericArguments().Take(2));
				fetchMethodTypes.Add(propType);

				MethodInfo fetchMethodInfo = orderExtensionMethodsType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.InvokeMethod).Where(x => x.Name == propFetchFunctionName && x.GetParameters().Length == 2).Single();
				fetchMethodInfo = fetchMethodInfo.MakeGenericMethod(fetchMethodTypes.ToArray());

				Object[] args = new Object[] { query, exprLambda };

				query = fetchMethodInfo.Invoke(null, args) as IOrderedQueryable<T>;

				currentType = propType;
			}

			return (query);
		}

		public static IOrderedQueryable<T> ThenByDescendingPath<T>(this IOrderedQueryable<T> query, String path)
		{
			Type currentType = typeof(T);
			String[] parts = path.Split(',');

			foreach (String part in parts)
			{
				PropertyInfo propInfo = currentType.GetProperty(part);
				Type propType = propInfo.PropertyType;
				String propFetchFunctionName = "ThenByDescending";
				Type delegateType = typeof(Func<,>).MakeGenericType(currentType, propType);

				ParameterExpression exprParam = Expression.Parameter(currentType, "it");
				MemberExpression exprProp = Expression.Property(exprParam, part);
				LambdaExpression exprLambda = Expression.Lambda(delegateType, exprProp, new ParameterExpression[] { exprParam });

				Type orderExtensionMethodsType = typeof(Queryable);

				List<Type> fetchMethodTypes = new List<Type>();
				fetchMethodTypes.AddRange(query.GetType().GetGenericArguments().Take(2));
				fetchMethodTypes.Add(propType);

				MethodInfo fetchMethodInfo = orderExtensionMethodsType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.InvokeMethod).Where(x => x.Name == propFetchFunctionName && x.GetParameters().Length == 2).Single();
				fetchMethodInfo = fetchMethodInfo.MakeGenericMethod(fetchMethodTypes.ToArray());

				Object[] args = new Object[] { query, exprLambda };

				query = fetchMethodInfo.Invoke(null, args) as IOrderedQueryable<T>;

				currentType = propType;
			}

			return (query);
		}
		#endregion

		#region Compare
		public enum Operand
		{
			Equal,
			NotEqual,
			GreaterThan,
			GreaterThanOrEqual,
			LessThan,
			LessThanOrEqual,
			TypeIs,
			IsNull,
			IsNotNull
		}

		public static IQueryable<TSource> Compare<TSource>(this IQueryable<TSource> query, Operand op, String propertyName, Object value = null)
		{
			Type type = typeof(TSource);
			ParameterExpression pe = Expression.Parameter(type, "p");
			MemberExpression propertyReference = Expression.Property(pe, propertyName);
			ConstantExpression constantReference = Expression.Constant(value);

			switch (op)
			{
				case Operand.Equal:
					return (query.Where(Expression.Lambda<Func<TSource, Boolean>>(Expression.Equal(propertyReference, constantReference), new ParameterExpression[] { pe })));

				case Operand.NotEqual:
					return (query.Where(Expression.Lambda<Func<TSource, Boolean>>(Expression.NotEqual(propertyReference, constantReference), new ParameterExpression[] { pe })));

				case Operand.GreaterThan:
					return (query.Where(Expression.Lambda<Func<TSource, Boolean>>(Expression.GreaterThan(propertyReference, constantReference), new ParameterExpression[] { pe })));

				case Operand.GreaterThanOrEqual:
					return (query.Where(Expression.Lambda<Func<TSource, Boolean>>(Expression.GreaterThanOrEqual(propertyReference, constantReference), new ParameterExpression[] { pe })));

				case Operand.LessThan:
					return (query.Where(Expression.Lambda<Func<TSource, Boolean>>(Expression.LessThan(propertyReference, constantReference), new ParameterExpression[] { pe })));

				case Operand.LessThanOrEqual:
					return (query.Where(Expression.Lambda<Func<TSource, Boolean>>(Expression.LessThanOrEqual(propertyReference, constantReference), new ParameterExpression[] { pe })));

				case Operand.TypeIs:
					return (query.Where(Expression.Lambda<Func<TSource, Boolean>>(Expression.TypeIs(propertyReference, value as Type))));

				case Operand.IsNull:
					return (query.Where(Expression.Lambda<Func<TSource, Boolean>>(Expression.Equal(propertyReference, Expression.Constant(null)), new ParameterExpression[] { pe })));

				case Operand.IsNotNull:
					return (query.Where(Expression.Lambda<Func<TSource, Boolean>>(Expression.NotEqual(propertyReference, Expression.Constant(null)), new ParameterExpression[] { pe })));
			}

			throw (new NotImplementedException("Operand is not implemented"));
		}
		#endregion

		#region OrderBy
		public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> query, String propertyName)
		{
			PropertyInfo propInfo = typeof(T).GetProperty(propertyName);
			Type propType = propInfo.PropertyType;
			Type delegateType = typeof(Func<,>).MakeGenericType(typeof(T), propType);
			ParameterExpression exprParam = Expression.Parameter(typeof(T), "it");
			MemberExpression exprProp = Expression.Property(exprParam, propertyName);
			LambdaExpression exprLambda = Expression.Lambda(delegateType, exprProp, new ParameterExpression[] { exprParam });
			Object[] args = new Object[] { query, exprLambda };

			MethodInfo fetchMethodInfo = typeof(Queryable).GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.InvokeMethod).Where(x => x.Name == "OrderBy" && x.GetParameters().Length == 2).Single();
			fetchMethodInfo = fetchMethodInfo.MakeGenericMethod(typeof(T), propType);

			query = fetchMethodInfo.Invoke(null, args) as IOrderedQueryable<T>;

			return (query as IOrderedQueryable<T>);
		}

		public static IOrderedQueryable<T> OrderByDescending<T>(this IQueryable<T> query, String propertyName)
		{
			PropertyInfo propInfo = typeof(T).GetProperty(propertyName);
			Type propType = propInfo.PropertyType;
			Type delegateType = typeof(Func<,>).MakeGenericType(typeof(T), propType);
			ParameterExpression exprParam = Expression.Parameter(typeof(T), "it");
			MemberExpression exprProp = Expression.Property(exprParam, propertyName);
			LambdaExpression exprLambda = Expression.Lambda(delegateType, exprProp, new ParameterExpression[] { exprParam });
			Object[] args = new Object[] { query, exprLambda };

			MethodInfo fetchMethodInfo = typeof(Queryable).GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.InvokeMethod).Where(x => x.Name == "OrderByDescending" && x.GetParameters().Length == 2).Single();
			fetchMethodInfo = fetchMethodInfo.MakeGenericMethod(typeof(T), propType);

			query = fetchMethodInfo.Invoke(null, args) as IOrderedQueryable<T>;

			return (query as IOrderedQueryable<T>);
		}

		public static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> query, String propertyName)
		{
			PropertyInfo propInfo = typeof(T).GetProperty(propertyName);
			Type propType = propInfo.PropertyType;
			Type delegateType = typeof(Func<,>).MakeGenericType(typeof(T), propType);
			ParameterExpression exprParam = Expression.Parameter(typeof(T), "it");
			MemberExpression exprProp = Expression.Property(exprParam, propertyName);
			LambdaExpression exprLambda = Expression.Lambda(delegateType, exprProp, new ParameterExpression[] { exprParam });
			Object[] args = new Object[] { query, exprLambda };

			MethodInfo fetchMethodInfo = typeof(Queryable).GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.InvokeMethod).Where(x => x.Name == "ThenBy" && x.GetParameters().Length == 2).Single();
			fetchMethodInfo = fetchMethodInfo.MakeGenericMethod(typeof(T), propType);

			query = fetchMethodInfo.Invoke(null, args) as IOrderedQueryable<T>;

			return (query as IOrderedQueryable<T>);
		}

		public static IOrderedQueryable<T> ThenByDescending<T>(this IOrderedQueryable<T> query, String propertyName)
		{
			PropertyInfo propInfo = typeof(T).GetProperty(propertyName);
			Type propType = propInfo.PropertyType;
			Type delegateType = typeof(Func<,>).MakeGenericType(typeof(T), propType);
			ParameterExpression exprParam = Expression.Parameter(typeof(T), "it");
			MemberExpression exprProp = Expression.Property(exprParam, propertyName);
			LambdaExpression exprLambda = Expression.Lambda(delegateType, exprProp, new ParameterExpression[] { exprParam });
			Object[] args = new Object[] { query, exprLambda };

			MethodInfo fetchMethodInfo = typeof(Queryable).GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.InvokeMethod).Where(x => x.Name == "ThenByDescending" && x.GetParameters().Length == 2).Single();
			fetchMethodInfo = fetchMethodInfo.MakeGenericMethod(typeof(T), propType);

			query = fetchMethodInfo.Invoke(null, args) as IOrderedQueryable<T>;

			return (query as IOrderedQueryable<T>);
		}		
		#endregion

		#region Between
		public static IQueryable<TSource> Between<TSource, TKey>(this IQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector, TKey low, TKey high) where TKey : IComparable<TKey>
		{
			// Get a ParameterExpression node of the TSource that is used in the expression tree 
			ParameterExpression sourceParameter = Expression.Parameter(typeof(TSource));

			// Get the body and parameter of the lambda expression 
			Expression body = keySelector.Body;
			ParameterExpression parameter = null;

			if (keySelector.Parameters.Count > 0)
			{
				parameter = keySelector.Parameters[0];
			}

			// Get the Compare method of the type of the return value 
			MethodInfo compareMethod = typeof(TKey).GetMethod("CompareTo", new Type[] { typeof(TKey) });

			// Expression.LessThanOrEqual and Expression.GreaterThanOrEqual method are only used in 
			// the numeric comparision. If we want to compare the non-numeric type, we can't directly  
			// use the two methods.  
			// So we first use the Compare method to compare the objects, and the Compare method  
			// will return a int number. Then we can use the LessThanOrEqual and GreaterThanOrEqua method. 
			// For this reason, we ask all the TKey type implement the IComparable<> interface. 
			Expression upper = Expression.LessThanOrEqual(Expression.Call(body, compareMethod, Expression.Constant(high)), Expression.Constant(0, typeof(Int32)));
			Expression lower = Expression.GreaterThanOrEqual(Expression.Call(body, compareMethod, Expression.Constant(low)), Expression.Constant(0, typeof(Int32)));

			Expression andExpression = Expression.AndAlso(upper, lower);

			// Get the Where method expression. 
			MethodCallExpression whereCallExpression = Expression.Call
			(
				typeof(Queryable),
				"Where",
				new Type[] { source.ElementType },
				source.Expression,
				Expression.Lambda<Func<TSource, Boolean>>(andExpression, new ParameterExpression[] { parameter })
			);

			return (source.Provider.CreateQuery<TSource>(whereCallExpression));
		}
		#endregion

		#region Where
		public static IQueryable<T> Where<T>(this IQueryable<T> query, String restriction, params Object[] values)
		{
			Assembly asm = typeof(UpdatePanel).Assembly;
			Type dynamicExpressionType = asm.GetType("System.Web.Query.Dynamic.DynamicExpression");
			MethodInfo parseLambdaMethod = dynamicExpressionType.GetMethods(BindingFlags.Public | BindingFlags.Static).Where(m => (m.Name == "ParseLambda") && (m.GetParameters().Length == 2)).Single().MakeGenericMethod(typeof(T), typeof(Boolean));
			Expression<Func<T, Boolean>> expression = parseLambdaMethod.Invoke(null, new Object[] { restriction, values }) as Expression<Func<T, Boolean>>;

			return (query.Where(expression));
		}

		public static IOrderedQueryable<T> Where<T>(this IOrderedQueryable<T> query, String restriction, params Object[] values)
		{
			return (Where(query, restriction, values) as IOrderedQueryable<T>);
		}
		#endregion

		#region GroupBy
		public static IQueryable GroupBy<T>(this IQueryable<T> query, String keySelector, String elementSelector, params Object[] values)
		{
			Assembly asm = typeof(UpdatePanel).Assembly;
			Type dynamicExpressionType = asm.GetType("System.Web.Query.Dynamic.DynamicQueryable");
			MethodInfo groupByMethod = dynamicExpressionType.GetMethod("GroupBy", BindingFlags.Public | BindingFlags.Static);

			return (groupByMethod.Invoke(null, new Object[] { query, keySelector, elementSelector, values }) as IQueryable);
		}

		#endregion

		#region Select
		public static IQueryable Select<T>(this IQueryable<T> query, params String [] propertyNames)
		{
			var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.SetProperty).Where(x => propertyNames.Contains(x.Name));
			var anonType = RuntimeTypeBuilder.GetDynamicType(Guid.NewGuid().ToString(), props);
			


			return Queryable.Select(query, Expression.Lambda(Expression.New(anonType.GetConstructor(props.Select(x => x.PropertyType).ToArray()), props.Select(x => Expression.MakeMemberAccess() )));
		}

		public static IQueryable Select<T>(this IQueryable<T> query, String projection, params Object [] values)
		{
			Assembly asm = typeof(UpdatePanel).Assembly;
			Type dynamicExpressionType = asm.GetType("System.Web.Query.Dynamic.DynamicQueryable");
			MethodInfo selectMethod = dynamicExpressionType.GetMethod("Select", BindingFlags.Public | BindingFlags.Static);

			return (selectMethod.Invoke(null, new Object[] { query, projection, values }) as IQueryable);
		}
		#endregion
	}
}
