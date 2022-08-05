using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Lexplosion.Logic.FileSystem;
using Lexplosion.Logic.Management.Instances;
using Lexplosion.Logic.Objects;
using Lexplosion.Tools;

namespace Lexplosion.Logic.Management
{
    class InstalledAddons : IDisposable
    {
        private static KeySemaphore<string> _semaphore = new KeySemaphore<string>();
        private static ConcurrentDictionary<string, InstalledAddons> _memoryStore = new ConcurrentDictionary<string, InstalledAddons>();

        private InstalledAddonsFormat _data;
        private string _instanceId;
        private int _referencesCount = 0;

        private InstalledAddons(InstalledAddonsFormat data, string instanceID)
        {
            _data = data;
            _instanceId = instanceID;
        }

        public static InstalledAddons Get(string instanceID)
        {
            InstalledAddons data;
            _semaphore.WaitOne(instanceID);
            if (_memoryStore.ContainsKey(instanceID))
            {
                data = _memoryStore[instanceID];
            }
            else
            {
                data = new InstalledAddons(DataFilesManager.GetInstalledAddons(instanceID), instanceID);
                _memoryStore[instanceID] = data;
            }
            data._referencesCount++;
            _semaphore.Release(instanceID);

            return data;
        }

        public InstalledAddonInfo this[int key]
        {
            get
            {
                _semaphore.WaitOne(_instanceId);
                InstalledAddonInfo data = null;
                if (_data.ContainsKey(key))
                {
                    data = _data[key];
                }
                _semaphore.Release(_instanceId);
                return data;
            }
            set
            {
                _semaphore.WaitOne(_instanceId);
                _data[key] = value;
                _semaphore.Release(_instanceId);
            }
        }

        public Dictionary<int, InstalledAddonInfo>.KeyCollection Keys
        {
            get
            {
                return _data.Keys;
            }
        }

        public bool ContainsKey(int addonId)
        {
            return _data.ContainsKey(addonId);
        }

        public void DisableAddon(int addonId, bool isDisable, Action<InstalledAddonInfo> onFunction, Action<InstalledAddonInfo> offFunction)
        {
            _semaphore.WaitOne(_instanceId);
            if (_data.ContainsKey(addonId))
            {
                var data = _data[addonId];
                if (isDisable)
                {
                    data.IsDisable = true;
                    offFunction(data);
                }
                else
                {
                    onFunction(data);
                    data.IsDisable = false;
                }
            }         
            _semaphore.Release(_instanceId);
        }

        public void Save()
        {
            _semaphore.WaitOne(_instanceId);
            DataFilesManager.SaveInstalledAddons(_instanceId, _data);
            _semaphore.Release(_instanceId);
        }

        public void Dispose()
        {
            _semaphore.WaitOne(_instanceId);
            _referencesCount--;
            if (_referencesCount < 1)
            {
                _memoryStore.TryRemove(_instanceId, out _);
            }
            _semaphore.Release(_instanceId);
        } 
    }
}
