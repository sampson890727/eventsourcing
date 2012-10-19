//Copyright (c) CodeSharp.  All rights reserved.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Castle.Core;
using Castle.Core.Configuration;
using Castle.MicroKernel;
using Castle.MicroKernel.Facilities;
using Castle.MicroKernel.ModelBuilder.Inspectors;

namespace CodeSharp.EventSourcing.Castles
{
    public class TransactionComponentInspector : MethodMetaInspector
    {
        private static readonly string TransactionNodeName = "eventsourcing_transaction_interceptor";
        private TransactionMetaInfoStore _metaStore;

        public override void ProcessModel(IKernel kernel, ComponentModel model)
        {
            if (_metaStore == null)
            {
                _metaStore = kernel.Resolve<TransactionMetaInfoStore>();
            }

            if (IsMarkedWithTransactional(model.Configuration))
            {
                base.ProcessModel(kernel, model);
            }
            else
            {
                AssertThereNoTransactionOnConfig(model);
                ConfigureBasedOnAttributes(model);
            }

            Validate(model, _metaStore);
            AddTransactionInterceptorIfIsTransactional(model, _metaStore);
        }
        protected override void ProcessMeta(ComponentModel model, IList<MethodInfo> methods, MethodMetaModel metaModel)
        {
            _metaStore.CreateMetaFromConfig(model.Implementation, methods, metaModel.ConfigNode);
        }
        protected override string ObtainNodeName()
        {
            return TransactionNodeName;
        }

        private void ConfigureBasedOnAttributes(ComponentModel model)
        {
            if (model.Implementation.IsDefined(typeof(TransactionalAttribute), true))
            {
                _metaStore.CreateMetaFromType(model.Implementation);
            }
        }
        private void Validate(ComponentModel model, TransactionMetaInfoStore store)
        {
            if (model.Services.Count() == 0 || model.Services.All(o => o.IsInterface))
            {
                return;
            }

            TransactionMetaInfo meta = store.GetMetaFor(model.Implementation);

            if (meta == null)
            {
                return;
            }

            ArrayList problematicMethods = new ArrayList();

            foreach (MethodInfo method in meta.Methods)
            {
                if (!method.IsVirtual)
                {
                    problematicMethods.Add(method.Name);
                }
            }

            if (problematicMethods.Count != 0)
            {
                string[] methodNames = (string[])problematicMethods.ToArray(typeof(string));

                string message = string.Format("The class {0} wants to use transaction interception, " +
                                               "however the methods must be marked as virtual in order to do so. Please correct " +
                                               "the following methods: {1}", model.Implementation.FullName,
                                               string.Join(", ", methodNames));

                throw new FacilityException(message);
            }
        }
        private bool IsMarkedWithTransactional(IConfiguration configuration)
        {
            return (configuration != null && "true" == configuration.Attributes["isTransactional"]);
        }
        private void AssertThereNoTransactionOnConfig(ComponentModel model)
        {
            IConfiguration configuration = model.Configuration;

            if (configuration != null && configuration.Children[TransactionNodeName] != null)
            {
                string message = string.Format("The class {0} has configured transaction in a child node but has not " +
                                               "specified istransaction=\"true\" on the component node.",
                                               model.Implementation.FullName);

                throw new FacilityException(message);
            }
        }
        private static void AddTransactionInterceptorIfIsTransactional(ComponentModel model, TransactionMetaInfoStore store)
        {
            TransactionMetaInfo meta = store.GetMetaFor(model.Implementation);

            if (meta == null)
            {
                return;
            }

            model.Dependencies.Add(new DependencyModel(null, typeof(TransactionInterceptor), false));
            model.Interceptors.AddFirst(new InterceptorReference("eventsourcing.transaction.interceptor"));
        }
    }
}
