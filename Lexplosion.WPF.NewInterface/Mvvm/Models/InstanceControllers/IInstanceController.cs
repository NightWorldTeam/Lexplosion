using Lexplosion.Logic.Management.Import;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Lexplosion.WPF.NewInterface.Mvvm.Models.InstanceControllers
{
    public interface ILibraryInstanceController : IInstanceController 
    {
        public InstanceModelBase? Add(InstanceClient instanceClient, ImportData? importData);
        /// <summary>
        /// Удаляет сборку по InstanceClient
        /// </summary>
        public void Remove(InstanceClient instanceClient);
        /// <summary>
        /// Выбранная группа
        /// </summary>
        public InstancesGroup SelectedGroup { get; }
        /// <summary>
        /// Группы сборок
        /// </summary>
        public IReadOnlyCollection<InstancesGroup> InstancesGroups { get; }
        /// <summary>
        /// Метод открывающий группу сборок
        /// </summary>
        /// <param name="instancesGroup">Группа сборок</param>
        public void SelectGroup(InstancesGroup instancesGroup);
        /// <summary>
        /// Добавляет новую группу в список.
        /// </summary>
        /// <param name="instancesGroup">Группа сборок</param>
        public void AddGroup(InstancesGroup instancesGroup);
        /// <summary>
        /// Удаляет группу из списка групп
        /// </summary>
        /// <param name="instancesGroup">Удаляемая группа</param>
        public void RemoveGroup(InstancesGroup instancesGroup);
        /// <summary>
        /// Возвращает InstanceModelBase по InstanceClient;
        /// </summary>
        /// <param name="instanceClient">Instance Client</param>
        /// <returns>InstanceModelBase или null</returns>
        public InstanceModelBase? Get(InstanceClient instanceClient);
    }

    public interface IInstanceController
    {
        public event Action<InstanceModelBase> InstanceAdded;
        public event Action<InstanceModelBase> InstanceRemoved;

        public IReadOnlyCollection<InstanceModelBase> Instances { get; }

        public void Add(InstanceModelBase instanceModelBase, [CallerMemberName] string member = "");
        public InstanceModelBase Add(InstanceClient instanceClient, [CallerMemberName] string member = "");
        public void Remove(InstanceModelBase instanceModelBase);
        public void Remove(InstanceClient instanceClient);
        public void Clear();
    }
}
