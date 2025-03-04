﻿using Lexplosion.Logic.Management.Instances;
using Lexplosion.WPF.NewInterface.Mvvm.Models.Mvvm.InstanceModel;
using System;
using System.Collections.Generic;

namespace Lexplosion.WPF.NewInterface.Mvvm.ViewModels.Args
{
    public class SelectInstanceForServerArgs
    {
        public readonly Func<IEnumerable<InstanceModelBase>> GetLibraryInstances;
        public readonly Func<InstanceClient, InstanceModelBase> AddNewInstanceInLibrary;

        public SelectInstanceForServerArgs(Func<IEnumerable<InstanceModelBase>> getLibraryInstances, Func<InstanceClient, InstanceModelBase> addNewInstanceInLibrary)
        {
            GetLibraryInstances = getLibraryInstances;
            AddNewInstanceInLibrary = addNewInstanceInLibrary;
        }
    }
}
