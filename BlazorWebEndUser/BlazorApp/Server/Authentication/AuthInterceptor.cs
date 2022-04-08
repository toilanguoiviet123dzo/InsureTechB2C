using Grpc.Core;
using Grpc.Core.Interceptors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cores.Grpc.Authentication
{
    internal class AuthInterceptor : Interceptor
    {
        /// <summary>
        /// Will find and return all AuthorizeApplicationKeyAttribute that are set on the method.
        /// If more than one attribute is set, that means that the application must have at least one role for each attribute.
        /// Else the application only needs to have one of the roles in the attribute.
        /// </summary>
        /// <param name="serviceType">The service where we will get the method from</param>
        /// <param name="methodName">The method where we will find the attributes.</param>
        /// <param name="inputParameterTypes"></param>
        /// <returns>If no attributes exist on the method; null is returned, else all attributes.</returns>
        //private AuthorizeApplicationKeyAttribute[] GetAuthorizeApplicationKeyAttributeForMethod(Type serviceType, string methodName, Type[] inputParameterTypes)
        //{
        //    MethodInfo methodInfo = serviceType.GetMethod(methodName, inputParameterTypes);

        //    var attributes = methodInfo?.GetCustomAttributes(typeof(AuthorizeApplicationKeyAttribute), true);
        //    if (attributes == null || attributes.Length == 0)
        //        return null;

        //    return attributes as AuthorizeApplicationKeyAttribute[];
        //}


        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
            TRequest request,
            ServerCallContext context,
            UnaryServerMethod<TRequest, TResponse> continuation)
        {
            try
            {
                //Authentication
                var userCredential = request.GetType().GetProperty("Credential").GetValue(request, null);
                if (userCredential == null) return null;

                //Check authentication by ApiKey for partner
                string apiKey = (string)(userCredential.GetType().GetProperty("ApiKey").GetValue(userCredential, null) ?? "");
                if (String.IsNullOrWhiteSpace(apiKey) || !AuthenticationService.CheckApiKey(apiKey)) return null;


                //Authorization
                string userName = (string)(userCredential.GetType().GetProperty("Username").GetValue(userCredential, null) ?? "");
                var targetType = continuation.Target.GetType();

                // Get the service type for the service we're calling a method in.
                var serviceType = targetType.GenericTypeArguments[0];

                // Get the method name by splitting the context.Method and get the last part that should be the method name.
                var method = context.Method;
                var methodSplit = method.Split('/');
                var requestedMethodName = methodSplit.Last();
                if (requestedMethodName == "xxx")
                {
                    var unAuthorizedReponse = Activator.CreateInstance<TResponse>();
                    unAuthorizedReponse.GetType().GetProperty("ReturnCode").SetValue(unAuthorizedReponse, -1, null);
                    return unAuthorizedReponse;
                }
            }
            catch
            {
                return null;
            }
            


            // Get the input type for the method so we find the correct method even if we've two methods that are called the same but have different input parameters.
            //Type[] types = new List<Type>() { targetType.GenericTypeArguments[1], typeof(ServerCallContext) }.ToArray();

            // Get the attributes for the method. If null is returned then no attributes were found.
            //var attributes = GetAuthorizeApplicationKeyAttributeForMethod(serviceType, requestedMethodName, types);
            //if (attributes != null)
            //{
            //    var httpContext = context.GetHttpContext();
            //    var applicationKey = context.ApplicationKey();
            //    // Validate that the application key has the expected roles.
            //    // If we have multiple attributes then the application must have at least one of the roles for every attribute.
            //    foreach (AuthorizeApplicationKeyAttribute attribute in attributes)
            //    {
            //        await attribute.ValidateApplicationKey(httpContext, applicationKey);
            //    }
            //}

            return await continuation(request, context);
        }
    }
}
